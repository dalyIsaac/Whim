using Windows.Win32.Foundation;

namespace Whim;

internal static partial class StoreExceptions
{
	public static WhimException NoValidWindow() => new("No valid window found.");

	public static WhimException WindowNotFound(HWND handle) => new($"Could not find window with handle {handle}");

	public static WhimException MonitorNotFound(IMonitor monitor) => new($"Monitor {monitor} not found in store.");

	public static WhimException NoWorkspaceFoundForMonitor(IMonitor monitor) =>
		new($"No workspace found for monitor {monitor}.");

	public static WhimException NoWorkspaceFoundForWindow(IWindow window) =>
		new($"No workspace found for window {window}.");

	public static WhimException NoMonitorFoundForWorkspace(IWorkspace workspace) =>
		new($"No monitor found for workspace {workspace}.");

	public static WhimException NoMonitorFoundForWindow(IWindow window) =>
		new($"No monitor found for window {window}.");

	public static WhimException NoAdjacentWorkspaceFound(IWorkspace Workspace) =>
		new($"No adjacent workspace found for {Workspace}.");

	public static WhimException NoMonitorFoundAtPoint(IPoint<int> point) => new($"No monitor found at point {point}.");

	public static WhimException InvalidMonitorIndex(int index) => new($"Monitor index {index} is invalid.");

	public static WhimException WindowIsSplashScreen(HWND handle) =>
		new($"Window {handle} is a splash screen, ignoring.");

	public static WhimException WindowIsCloaked(HWND handle) => new($"Window {handle} is cloaked, ignoring.");

	public static WhimException WindowIsNotStandard(HWND handle) =>
		new($"Window {handle} is not a standard window, ignoring.");

	public static WhimException WindowHasNoVisibleOwner(HWND handle) =>
		new($"Window {handle} has a visible owner, ignoring.");

	public static WhimException IgnoredByFilter(HWND handle) => new("Window was ignored by filter");

	public static WhimException WorkspaceNotFound() => new($"Workspace not found in store.");
}
