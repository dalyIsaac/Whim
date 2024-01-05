using System.Collections.Generic;

namespace Whim;

internal class ButlerPantry : IButlerPantry
{
	private readonly Dictionary<IWindow, IWorkspace> _windowWorkspaceMap = new();
	private readonly Dictionary<IMonitor, IWorkspace> _monitorWorkspaceMap = new();

	public IMonitor? GetMonitorForWindow(IWindow window)
	{
		Logger.Debug($"Getting monitor for window: {window}");
		return _windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace)
			? GetMonitorForWorkspace(workspace)
			: null;
	}

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace)
	{
		Logger.Debug($"Getting monitor for workspace {workspace}");

		// Linear search for the monitor that contains the workspace.
		foreach ((IMonitor m, IWorkspace w) in _monitorWorkspaceMap)
		{
			if (w.Equals(workspace))
			{
				Logger.Debug($"Found monitor {m} for workspace {workspace}");
				return m;
			}
		}

		Logger.Debug($"Could not find monitor for workspace {workspace}");
		return null;
	}

	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor)
	{
		Logger.Debug($"Getting workspace for monitor {monitor}");
		return _monitorWorkspaceMap.TryGetValue(monitor, out IWorkspace? workspace) ? workspace : null;
	}

	public IWorkspace? GetWorkspaceForWindow(IWindow window)
	{
		Logger.Debug($"Getting workspace for window {window}");
		return _windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace) ? workspace : null;
	}

	public bool RemoveMonitor(IMonitor monitor) => throw new System.NotImplementedException();

	public bool RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window}");
		return _windowWorkspaceMap.Remove(window);
	}

	public bool RemoveWorkspace(IWorkspace workspace) => throw new System.NotImplementedException();

	public void SetWindowWorkspace(IWindow window, IWorkspace workspace)
	{
		Logger.Debug($"Setting window {window} to workspace {workspace}");
		_windowWorkspaceMap[window] = workspace;
	}

	public void SetMonitorWorkspace(IMonitor monitor, IWorkspace workspace)
	{
		Logger.Debug($"Setting workspace {workspace} to monitor {monitor}");
		_monitorWorkspaceMap[monitor] = workspace;
	}
}
