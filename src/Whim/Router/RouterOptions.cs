namespace Whim;

/// <summary>
/// Describes how to route windows when they are added to Whim.
/// </summary>
public enum RouterOptions
{
	/// <summary>
	/// Routes windows to the workspace that the window was launched on by Windows.
	/// </summary>
	RouteToLaunchedWorkspace,

	/// <summary>
	/// Routes windows to the active workspace. This may lead to unexpected results, as the
	/// <see cref="IMonitorSector.ActiveMonitorHandle"/> and thus the active workspace
	/// will be updated by every window event sent by Windows - even those which Whim ignores.
	///
	/// <br/>
	///
	/// For example, launching an app from the taskbar on Windows 11 will cause <c>Shell_TrayWnd</c>
	/// to focus on the main monitor, overriding the <see cref="IMonitorSector.ActiveMonitorHandle"/>.
	/// As a result, the window will be routed to the workspace on the main monitor.
	/// </summary>
	RouteToActiveWorkspace,

	/// <summary>
	/// Routes windows to the workspace which last received an event sent by Windows which Whim
	/// did not ignore.
	/// </summary>
	RouteToLastTrackedActiveWorkspace,
}
