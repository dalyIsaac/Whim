using System;

namespace Whim;

/// <summary>
/// Delegate which is called to route a <see cref="IWindow"/>.
/// </summary>
public delegate IWorkspace? Router(IWindow window);

/// <summary>
/// Describes how to route windows when they are added to Whim.
/// </summary>
public enum RouterOptions
{
	/// <summary>
	/// Routes windows to the workspace which is currently active on the monitor the window is on.
	/// </summary>
	RouteToLaunchedWorkspace,

	/// <summary>
	/// Routes windows to the active workspace. This may lead to unexpected results, as the
	/// <see cref="IMonitorManager.ActiveMonitor"/> and thus <see cref="IWorkspaceManager.ActiveWorkspace"/>
	/// will be updated by every window event sent by Windows - even those which Whim ignores.
	///
	/// <br/>
	///
	/// For example, launching an app from the taskbar on Windows 11 will cause <c>Shell_TrayWnd</c>
	/// to focus on the main monitor, overriding the <see cref="IMonitorManager.ActiveMonitor"/>.
	/// As a result, the window will be routed to the workspace on the main monitor.
	/// </summary>
	RouteToActiveWorkspace,

	/// <summary>
	/// Routes windows to the workspace which last received an event sent by Windows which Whim
	/// did not ignore.
	/// </summary>
	RouteToLastTrackedActiveWorkspace
}

/// <summary>
/// Manages routers for <see cref="IWindow"/>s.
/// </summary>
public interface IRouterManager
{
	/// <summary>
	/// When <see langword="true"/>, windows are routed to the active workspace.
	/// When <see langword="false"/>, windows are routed to the active workspace on the monitor they are on.
	/// Defaults to <see langword="false"/>.
	/// This is overridden by any other routers in this <see cref="IRouterManager"/>.
	/// </summary>
	[Obsolete("Use RouterOptions instead")]
	bool RouteToActiveWorkspace { get; set; }

	/// <summary>
	/// Describes how to route windows when they are added to Whim. <see cref="RouteWindow(IWindow)"/>
	/// takes precedence over this.
	/// </summary>
	RouterOptions RouterOptions { get; set; }

	/// <summary>
	/// Routes a window to a workspace.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>
	/// <see langword="null"/> when the window should be ignored, otherwise the
	/// <see cref="IWorkspace"/> to route the window to.
	/// </returns>
	IWorkspace? RouteWindow(IWindow window);

	/// <summary>
	/// Clear all the routes.
	/// </summary>
	void Clear();

	/// <summary>
	/// Add a router.
	/// </summary>
	/// <param name="router"></param>
	void Add(Router router);

	#region Routers helper methods
	/// <summary>
	/// Adds a router which moves windows matching <paramref name="windowClass"/> to the workspace
	/// with the name <paramref name="workspaceName"/>.
	/// </summary>
	/// <param name="windowClass"></param>
	/// <param name="workspaceName"></param>
	IRouterManager AddWindowClassRoute(string windowClass, string workspaceName);

	/// <summary>
	/// Adds a router which moves windows matching <paramref name="windowClass"/> to the
	/// <paramref name="workspace"/>.
	/// </summary>
	/// <param name="windowClass"></param>
	/// <param name="workspace"></param>
	IRouterManager AddWindowClassRoute(string windowClass, IWorkspace workspace);

	/// <summary>
	/// Adds a router which moves processes matching <see cref="IWindow.ProcessName"/> to the
	/// <paramref name="workspaceName"/>.
	/// </summary>
	/// <param name="processName"></param>
	/// <param name="workspaceName"></param>
	[Obsolete("Use AddProcessFileNameRoute instead")]
	IRouterManager AddProcessNameRoute(string processName, string workspaceName);

	/// <summary>
	/// Adds a router which moves processes matching <see cref="IWindow.ProcessName"/> to the
	/// <paramref name="workspace"/>.
	/// </summary>
	/// <param name="processName"></param>
	/// <param name="workspace"></param>
	[Obsolete("Use AddProcessFileNameRoute instead")]
	IRouterManager AddProcessNameRoute(string processName, IWorkspace workspace);

	/// <summary>
	/// Adds a router which moves windows matching <see cref="IWindow.ProcessFileName"/> to the
	/// <paramref name="workspaceName"/>.
	/// </summary>
	/// <param name="processFileName"></param>
	/// <param name="workspaceName"></param>
	IRouterManager AddProcessFileNameRoute(string processFileName, string workspaceName);

	/// <summary>
	/// Adds a router which moves windows matching <see cref="IWindow.ProcessFileName"/> to the
	/// <paramref name="workspace"/>.
	/// </summary>
	/// <param name="processFileName"></param>
	/// <param name="workspace"></param>
	IRouterManager AddProcessFileNameRoute(string processFileName, IWorkspace workspace);

	/// <summary>
	/// Adds a router which moves windows matching <paramref name="title"/> to the workspace
	/// with the name <paramref name="workspaceName"/>.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="workspaceName"></param>
	IRouterManager AddTitleRoute(string title, string workspaceName);

	/// <summary>
	/// Adds a router which moves windows matching <paramref name="title"/> to the
	/// <paramref name="workspace"/>.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="workspace"></param>
	IRouterManager AddTitleRoute(string title, IWorkspace workspace);

	/// <summary>
	/// Adds a router which moves windows matching regex <paramref name="match"/> string to the
	/// <paramref name="workspaceName"/>.
	/// </summary>
	/// <param name="match"></param>
	/// <param name="workspaceName"></param>
	IRouterManager AddTitleMatchRoute(string match, string workspaceName);

	/// <summary>
	/// Adds a router which moves windows matching regex <paramref name="match"/> string to the
	/// <paramref name="workspace"/>.
	/// </summary>
	/// <param name="match"></param>
	/// <param name="workspace"></param>
	IRouterManager AddTitleMatchRoute(string match, IWorkspace workspace);
	#endregion
}
