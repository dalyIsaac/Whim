namespace Whim;

internal static class StoreExceptions
{
	public static Exception MonitorNotFound(HMONITOR handle) =>
		new WhimException($"Monitor with handle {handle} not found.");

	public static Exception WindowNotFound(HWND handle) => new WhimException($"Window with handle {handle} not found.");

	public static Exception WorkspaceNotFound(WorkspaceId workspaceId) =>
		new WhimException($"Workspace {workspaceId} not found.");

	public static Exception NoMonitorFoundAtPoint(IPoint<int> point) =>
		new WhimException($"No monitor found at point {point}.");

	public static Exception NoMonitorFoundForWorkspace(WorkspaceId workspaceId) =>
		new WhimException($"No monitor found for workspace {workspaceId}.");

	public static Exception NoMonitorFoundForWindow(HWND windowHandle) =>
		new WhimException($"No monitor found for window {windowHandle}.");

	public static Exception NoWorkspaceFoundForMonitor(HMONITOR monitorHandle) =>
		new WhimException($"No workspace found for monitor {monitorHandle}.");

	public static Exception NoWorkspaceFoundForWindow(HWND windowHandle) =>
		new WhimException($"No workspace found for window {windowHandle}.");

	public static Exception NoValidWindow() => new WhimException("No valid window found.");

	public static Exception InvalidMonitorIndex(int monitorIndex) =>
		new WhimException($"No monitor found at index {monitorIndex}.");
}
