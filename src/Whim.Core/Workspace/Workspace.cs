using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim.Core;

public class Workspace : IWorkspace
{
	private readonly IConfigContext _configContext;
	public string Name { get; set; }

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

		IEnumerable<IWindowLocation> locations = ActiveLayoutEngine.DoLayout(new Area(width: monitor.Width,
																				height: monitor.Height));
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

	public Workspace(IConfigContext configContext, string name)
	{
		_configContext = configContext;
		Name = name;
	}

	public void NextLayoutEngine()
	{
		Logger.Debug("Switching to next layout engine for workspace {Name}", Name);

		_activeLayoutEngineIndex = (_activeLayoutEngineIndex + 1) % _layoutEngines.Count;
		DoLayout();
	}

	public void PreviousLayoutEngine()
	{
		Logger.Debug("Switching to previous layout engine for workspace {Name}", Name);

		_activeLayoutEngineIndex = (_activeLayoutEngineIndex - 1) % _layoutEngines.Count;
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

		_activeLayoutEngineIndex = _layoutEngines.IndexOf(layoutEngine);
		DoLayout();
		return true;
	}

	public void AddLayoutEngine(ILayoutEngine layoutEngine)
	{
		Logger.Debug("Adding layout engine {name} to workspace {workspace}", layoutEngine.Name, Name);

		_layoutEngines.Add(layoutEngine);
	}

	public bool RemoveLayoutEngine(ILayoutEngine layoutEngine)
	{
		Logger.Debug("Removing layout engine {name} from workspace {workspace}", layoutEngine.Name, Name);

		return _layoutEngines.Remove(layoutEngine);
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
}
