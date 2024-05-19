using System.Collections.Generic;

namespace Whim;

internal class ButlerPantry : IButlerPantry
{
	private readonly IContext _ctx;

	public ButlerPantry(IContext context)
	{
		_ctx = context;
	}

	public IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse = false, bool skipActive = false) =>
		_ctx.Store.Pick(Pickers.PickAdjacentWorkspace(workspace.Id, reverse, skipActive)).OrDefault();

	public IEnumerable<IWorkspace> GetAllActiveWorkspaces() => _ctx.Store.Pick(Pickers.PickAllActiveWorkspaces());

	public IMonitor? GetMonitorForWindow(IWindow window) =>
		_ctx.Store.Pick(Pickers.PickMonitorByWindow(window.Handle)).OrDefault();

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace) =>
		_ctx.Store.Pick(Pickers.PickMonitorByWorkspace(workspace.Id)).OrDefault();

	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor) =>
		_ctx.Store.Pick(Pickers.PickWorkspaceByMonitor(monitor.Handle)).OrDefault();

	public IWorkspace? GetWorkspaceForWindow(IWindow window) =>
		_ctx.Store.Pick(Pickers.PickWorkspaceByWindow(window.Handle)).OrDefault();

	public bool RemoveMonitor(IMonitor monitor) => _monitorWorkspaceMap.Remove(monitor);

	public bool RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window}");
		return _windowWorkspaceMap.Remove(window);
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
