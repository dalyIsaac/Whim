using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim.Core;

public class Workspace : IWorkspace
{
	private readonly IConfigContext _configContext;

	private string _name;
	public string Name
	{
		get => _name;
		set
		{
			WorkspaceRenamed?.Invoke(this, new WorkspaceRenameEventArgs(this, _name, value));
			_name = value;
		}
	}

	private readonly List<ILayoutEngine> _layoutEngines = new();
	private int _activeLayoutEngineIndex = 0;

	public ILayoutEngine ActiveLayoutEngine => _layoutEngines[_activeLayoutEngineIndex];

	private readonly HashSet<IWindow> _windows = new();
	public IEnumerable<IWindow> Windows => _windows;

	public Commander Commander { get; } = new();

	public void DoLayout()
	{
		Logger.Debug("Doing layout of workspace {Name}", Name);

		// Get the monitor for this workspace
		IMonitor? monitor = _configContext.WorkspaceManager.GetMonitorForWorkspace(this);
		if (monitor == null)
		{
			Logger.Debug("No active monitors found for workspace {Name}.", Name);
			return;
		}

		// Ensure there's at least one layout engine
		if (_layoutEngines.Count == 0)
		{
			Logger.Fatal("No layout engines found for workspace {Name}.", Name);
			throw new InvalidOperationException("No layout engines found for workspace " + Name);
		}

		using IWindowDeferPosHandle handle = WindowDeferPosHandle.Initialize(Windows.Count());

		IEnumerable<IWindowLocation> locations = ActiveLayoutEngine.DoLayout(new Area(monitor.Width, monitor.Height));
		foreach (IWindowLocation loc in locations)
		{
			// Adjust the window location to the monitor's coordinates
			loc.Location = new Location(x: loc.Location.X + monitor.X,
										y: loc.Location.Y + monitor.Y,
										width: loc.Location.Width,
										height: loc.Location.Height);

			handle.DeferWindowPos(loc);
		}
	}

	public event EventHandler<WorkspaceRenameEventArgs>? WorkspaceRenamed;
	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;
	public event EventHandler<LayoutEngineEventArgs>? LayoutEngineAdded;
	public event EventHandler<LayoutEngineEventArgs>? LayoutEngineRemoved;

	public Workspace(IConfigContext configContext, string name)
	{
		_configContext = configContext;
		_name = name;
	}

	public void NextLayoutEngine()
	{
		Logger.Debug("Switching to next layout engine for workspace {Name}", Name);

		int prevIdx = _activeLayoutEngineIndex;
		_activeLayoutEngineIndex = (_activeLayoutEngineIndex + 1) % _layoutEngines.Count;

		ActiveLayoutEngineChanged?.Invoke(this, new ActiveLayoutEngineChangedEventArgs(_layoutEngines[prevIdx], _layoutEngines[_activeLayoutEngineIndex]));

		DoLayout();
	}

	public void PreviousLayoutEngine()
	{
		Logger.Debug("Switching to previous layout engine for workspace {Name}", Name);

		int prevIdx = _activeLayoutEngineIndex;
		_activeLayoutEngineIndex = (_activeLayoutEngineIndex - 1) % _layoutEngines.Count;

		ActiveLayoutEngineChanged?.Invoke(this, new ActiveLayoutEngineChangedEventArgs(_layoutEngines[prevIdx], _layoutEngines[_activeLayoutEngineIndex]));

		DoLayout();
	}

	public bool TrySetLayoutEngine(string name)
	{
		Logger.Debug("Trying to set layout engine {name} for workspace {workspace}", name, Name);

		ILayoutEngine? layoutEngine = _layoutEngines.FirstOrDefault(x => x.Name == name);
		if (layoutEngine == null)
		{
			return false;
		}

		int prevIdx = _activeLayoutEngineIndex;
		_activeLayoutEngineIndex = _layoutEngines.IndexOf(layoutEngine);

		if (_activeLayoutEngineIndex == -1)
		{
			Logger.Error("Layout engine {name} not found for workspace {workspace}", name, Name);
			return false;
		}
		else if (_activeLayoutEngineIndex == prevIdx)
		{
			Logger.Debug("Layout engine {name} is already active for workspace {workspace}", name, Name);
			return true;
		}

		ActiveLayoutEngineChanged?.Invoke(this, new ActiveLayoutEngineChangedEventArgs(_layoutEngines[prevIdx], _layoutEngines[_activeLayoutEngineIndex]));

		DoLayout();
		return true;
	}

	public void AddLayoutEngine(ILayoutEngine layoutEngine)
	{
		Logger.Debug("Adding layout engine {name} to workspace {workspace}", layoutEngine.Name, Name);

		_layoutEngines.Add(layoutEngine);

		LayoutEngineAdded?.Invoke(this, new LayoutEngineEventArgs(this, layoutEngine));
	}

	public bool RemoveLayoutEngine(ILayoutEngine layoutEngine)
	{
		Logger.Debug("Removing layout engine {name} from workspace {workspace}", layoutEngine.Name, Name);

		if (_layoutEngines.Remove(layoutEngine))
		{
			LayoutEngineRemoved?.Invoke(this, new LayoutEngineEventArgs(this, layoutEngine));
			return true;
		}

		return false;
	}

	public bool RemoveLayoutEngine(string name)
	{
		Logger.Debug("Removing layout engine {name} from workspace {workspace}", name, Name);

		ILayoutEngine? layoutEngine = _layoutEngines.FirstOrDefault(x => x.Name == name);
		if (layoutEngine == null)
		{
			return false;
		}

		return _layoutEngines.Remove(layoutEngine);
	}

	public void AddWindow(IWindow window)
	{
		Logger.Debug("Adding window {window} to workspace {Name}", window, Name);

		if (_windows.Contains(window))
		{
			Logger.Debug("Window {window} already exists in workspace {Name}", window, Name);
			return;
		}

		foreach (ILayoutEngine layoutEngine in _layoutEngines)
		{
			layoutEngine.AddWindow(window);
		}
	}

	public bool RemoveWindow(IWindow window)
	{
		Logger.Debug("Removing window {window} from workspace {Name}", window, Name);

		if (!_windows.Contains(window))
		{
			Logger.Debug("Window {window} already does not exist in workspace {Name}", window, Name);
			return false;
		}

		bool success = true;
		foreach (ILayoutEngine layoutEngine in _layoutEngines)
		{
			if (!layoutEngine.RemoveWindow(window))
			{
				Logger.Debug("Window {window} could not be removed from layout engine {layoutEngine}", window, layoutEngine);
				success = false;
			}
		}
		return success;
	}

	public override string ToString() => Name;
}
