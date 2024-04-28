using System.Collections.Generic;

namespace Whim;

internal class ButlerPantry : IButlerPantry
{
	private readonly IContext _context;

	public ButlerPantry(IContext context)
	{
		_context = context;
	}

	public IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse = false, bool skipActive = false) =>
		_context.Store.Pick(MapPickers.GetAdjacentWorkspace(workspace, reverse, skipActive)).OrDefault();

	public IEnumerable<IWorkspace> GetAllActiveWorkspaces() => _context.Store.Pick(MapPickers.GetAllActiveWorkspaces);

	public IMonitor? GetMonitorForWindow(IWindow window) =>
		_context.Store.Pick(MapPickers.GetMonitorForWindow(window)).OrDefault();

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace) =>
		_context.Store.Pick(MapPickers.GetMonitorForWorkspace(workspace)).OrDefault();

	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor) =>
		_context.Store.Pick(MapPickers.GetWorkspaceForMonitor(monitor)).OrDefault();

	public IWorkspace? GetWorkspaceForWindow(IWindow window) =>
		_context.Store.Pick(MapPickers.GetWorkspaceForWindow(window)).OrDefault();

	public bool RemoveMonitor(IMonitor monitor)
	{
		Logger.Debug($"Removing monitor {monitor}");
		return _monitorWorkspaceMap.Remove(monitor);
	}

	public void MergeWorkspaceWindows(IWorkspace workspaceToDelete, IWorkspace workspaceMergeTarget)
	{
		Logger.Debug($"Removing workspace {workspaceToDelete} and moving windows to {workspaceMergeTarget}");

		// Remove the workspace from the monitor map.
		IMonitor? monitor = GetMonitorForWorkspace(workspaceToDelete);
		if (monitor != null)
		{
			_monitorWorkspaceMap[monitor] = workspaceMergeTarget;
		}

		// Remap windows to the first workspace which isn't active.
		foreach (IWindow window in workspaceToDelete.Windows)
		{
			_windowWorkspaceMap[window] = workspaceMergeTarget;
		}
	}

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
