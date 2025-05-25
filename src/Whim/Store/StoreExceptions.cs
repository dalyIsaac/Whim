namespace Whim;

internal static class StoreExceptions
{	public static WhimError MonitorNotFound(HMONITOR handle) =>
		new($"Monitor with handle {handle} not found.");

	public static WhimError WindowNotFound(HWND handle) => new($"Window with handle {handle} not found.");	public static WhimError WindowNotFoundInWorkspace(HWND handle, WorkspaceId workspaceId) =>
		new($"Window with handle {handle} not found in workspace {workspaceId}.");	public static WhimError WorkspaceNotFound(WorkspaceId workspaceId) =>
		new($"Workspace {workspaceId} not found.");	public static WhimError NoMonitorFoundAtPoint(IPoint<int> point) =>
		new($"No monitor found at point {point}.");
	public static WhimError NoMonitorFoundForWorkspace(WorkspaceId workspaceId) =>
		new($"No monitor found for workspace {workspaceId}.");
	public static WhimError NoMonitorFoundForWindow(HWND windowHandle) =>
		new($"No monitor found for window {windowHandle}.");
	public static WhimError NoWorkspaceFoundForMonitor(HMONITOR monitorHandle) =>
		new($"No workspace found for monitor {monitorHandle}.");
	public static WhimError NoWorkspaceFoundForWindow(HWND windowHandle) =>
		new($"No workspace found for window {windowHandle}.");
	public static WhimError NoValidMonitorForWorkspace(WorkspaceId workspaceId) =>
		new($"No valid monitor found for workspace {workspaceId}.");

	public static WhimError NoValidWindow() => new("No valid window found.");
	public static WhimError InvalidMonitorIndex(int monitorIndex) =>
		new($"No monitor found at index {monitorIndex}.");
}
