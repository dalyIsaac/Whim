namespace Whim;

internal partial class WorkspaceManager2 : IWorkspaceManager2, IInternalWorkspaceManager
{
	// TODO: Rename methods to start with WindowManager_
	public void WindowAdded(IWindow window)
	{
		Logger.Debug($"Adding window {window}");

		IWorkspace? workspace = null;
		if (_context.RouterManager.RouterOptions == RouterOptions.RouteToActiveWorkspace)
		{
			workspace = ActiveWorkspace;
		}
		else if (_context.RouterManager.RouterOptions == RouterOptions.RouteToLastTrackedActiveWorkspace)
		{
			workspace = _internalContext.MonitorManager.LastWhimActiveMonitor is IMonitor lastWhimActiveMonitor
				? WorkspaceContainer.GetWorkspaceForMonitor(lastWhimActiveMonitor)
				: ActiveWorkspace;
		}

		// RouteWindow takes precedence over RouterOptions.
		if (_context.RouterManager.RouteWindow(window) is IWorkspace routedWorkspace)
		{
			workspace = routedWorkspace;
		}

		// Check the workspace exists. If it doesn't, clear the workspace.
		if (workspace != null && !WorkspaceContainer.Contains(workspace))
		{
			Logger.Error($"Workspace {workspace} was not found");
			workspace = null;
		}

		// If no workspace has been selected, route the window to the monitor it's on.
		if (workspace == null)
		{
			IMonitor? monitor = _context.MonitorManager.GetMonitorAtPoint(window.Rectangle.Center);
			if (monitor is not null)
			{
				workspace = WorkspaceContainer.GetWorkspaceForMonitor(monitor);
			}
		}

		// If that fails too, route the window to the active workspace.
		workspace ??= ActiveWorkspace;

		WorkspaceContainer.SetWindowWorkspace(window, workspace);

		if (window.IsMinimized)
		{
			workspace.MinimizeWindowStart(window);
		}
		else
		{
			workspace.AddWindow(window);
		}

		WindowRouted?.Invoke(this, RouteEventArgs.WindowAdded(window, workspace));
		Logger.Debug($"Window {window} added to workspace {workspace.Name}");
	}

	public void WindowRemoved(IWindow window)
	{
		Logger.Debug($"Window removed: {window}");

		if (WorkspaceContainer.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		WorkspaceContainer.RemoveWindow(window);

		workspace.RemoveWindow(window);
		WindowRouted?.Invoke(this, RouteEventArgs.WindowRemoved(window, workspace));
	}

	public void WindowFocused(IWindow? window)
	{
		Logger.Debug($"Window focused: {window}");

		foreach (IWorkspace workspace in WorkspaceContainer)
		{
			((IInternalWorkspace)workspace).WindowFocused(window);
		}

		if (window is null)
		{
			return;
		}

		if (WorkspaceContainer.GetWorkspaceForWindow(window) is not IWorkspace workspaceForWindow)
		{
			Logger.Debug($"Window {window} was not found in any workspace");
			return;
		}

		if (WorkspaceContainer.GetMonitorForWorkspace(workspaceForWindow) is null)
		{
			Logger.Debug($"Window {window} is not in an active workspace");
			Activate(workspaceForWindow);
			return;
		}
	}

	public void WindowMinimizeStart(IWindow window)
	{
		Logger.Debug($"Window minimize start: {window}");

		if (WorkspaceContainer.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.MinimizeWindowStart(window);
	}

	public void WindowMinimizeEnd(IWindow window)
	{
		Logger.Debug($"Window minimize end: {window}");

		if (WorkspaceContainer.GetWorkspaceForWindow(window) is not IWorkspace workspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.MinimizeWindowEnd(window);
	}

	public void MonitorManager_MonitorsChanged(object? sender, MonitorsChangedEventArgs e)
	{
		Logger.Debug($"MonitorManager_MonitorsChanged: {e}");

		// If a monitor was removed, remove the workspace from the map.
		foreach (IMonitor monitor in e.RemovedMonitors)
		{
			IWorkspace? workspace = WorkspaceContainer.GetWorkspaceForMonitor(monitor);
			WorkspaceContainer.RemoveMonitor(monitor);

			if (workspace is null)
			{
				Logger.Error($"Could not find workspace for monitor {monitor}");
				continue;
			}

			workspace.Deactivate();
		}

		// If a monitor was added, set it to an inactive workspace.
		foreach (IMonitor monitor in e.AddedMonitors)
		{
			// Try find a workspace which doesn't have a monitor.
			IWorkspace? workspace = WorkspaceContainer.Find(w => GetMonitorForWorkspace(w) == null);

			// If there's no workspace, create one.
			if (workspace is null)
			{
				if (WorkspaceContainer.Add() is IWorkspace newWorkspace)
				{
					workspace = newWorkspace;
					WorkspaceAdded?.Invoke(this, new WorkspaceEventArgs() { Workspace = workspace });
				}
				else
				{
					continue;
				}
			}

			// Add the workspace to the map.
			Activate(workspace, monitor);
		}

		// For each workspace which is active in a monitor, do a layout.
		// This will handle cases when the monitor's properties have changed.
		LayoutAllActiveWorkspaces();
	}
}
