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

	public IEnumerable<IWindow> Windows => throw new NotImplementedException();

	public Commander Commander { get; } = new();

	public void DoLayout()
	{
		IMonitor focusedMonitor = _configContext.MonitorManager.FocusedMonitor;

		using IWindowDeferPosHandle handle = WindowDeferPosHandle.Initialize(Windows.Count());

		IEnumerable<IWindowLocation> locations = ActiveLayoutEngine.DoLayout(new Area(width: focusedMonitor.Width,
																				height: focusedMonitor.Height));
		foreach (IWindowLocation loc in locations)
		{
			// Adjust the window location to the monitor's coordinates
			loc.Location = new Location(x: loc.Location.X + focusedMonitor.X,
										y: loc.Location.Y + focusedMonitor.Y,
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
		_activeLayoutEngineIndex = (_activeLayoutEngineIndex + 1) % _layoutEngines.Count;
		DoLayout();
	}

	public void PreviousLayoutEngine()
	{
		_activeLayoutEngineIndex = (_activeLayoutEngineIndex - 1) % _layoutEngines.Count;
		DoLayout();
	}

	public bool TrySetLayoutEngine(string name)
	{
		ILayoutEngine? layoutEngine = _layoutEngines.FirstOrDefault(x => x.Name == name);
		if (layoutEngine == null)
		{
			return false;
		}

		_activeLayoutEngineIndex = _layoutEngines.IndexOf(layoutEngine);
		DoLayout();
		return true;
	}

	public void AddWindow(IWindow window)
	{
		foreach (ILayoutEngine layoutEngine in _layoutEngines)
		{
			layoutEngine.AddWindow(window);
		}
	}

	public void RemoveWindow(IWindow window)
	{
		foreach (ILayoutEngine layoutEngine in _layoutEngines)
		{
			layoutEngine.RemoveWindow(window);
		}
	}
}
