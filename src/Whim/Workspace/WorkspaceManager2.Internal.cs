namespace Whim;

internal partial class WorkspaceManager2 : IWorkspaceManager2, IInternalWorkspaceManager
{
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

		// TODO
		// WindowRouted?.Invoke(this, RouteEventArgs.WindowAdded(window, workspace));
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

		WorkspaceContainer.Remove(window);

		workspace.RemoveWindow(window);
		// TODO
		//WindowRouted?.Invoke(this, RouteEventArgs.WindowRemoved(window, workspace));
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
}
