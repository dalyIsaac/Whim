using System;

namespace Whim;

/// <summary>
/// Delegate which is called to route a <see cref="IWindow"/>.
/// </summary>
public delegate IWorkspace? Router(IWindow window);

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
	bool RouteToActiveWorkspace { get; set; }

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
