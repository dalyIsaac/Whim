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
	/// Routes a window to a workspace.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>
	/// <see langword="null"/> when the window should be ignored, otherwise the
	/// <see cref="IWorkspace"/> to route the window to.
	/// </returns>
	public IWorkspace? RouteWindow(IWindow window);

	/// <summary>
	/// Clear all the routes.
	/// </summary>
	public void Clear();

	/// <summary>
	/// Add a router.
	/// </summary>
	/// <param name="router"></param>
	public void Add(Router router);

	#region Routers helper methods
	/// <summary>
	/// Adds a router which moves windows matching <paramref name="windowClass"/> to the workspace
	/// with the name <paramref name="workspaceName"/>.
	/// </summary>
	/// <param name="windowClass"></param>
	/// <param name="workspaceName"></param>
	public IRouterManager AddWindowClassRoute(string windowClass, string workspaceName);

	/// <summary>
	/// Adds a router which moves windows matching <paramref name="windowClass"/> to the
	/// <paramref name="workspace"/>.
	/// </summary>
	/// <param name="windowClass"></param>
	/// <param name="workspace"></param>
	public IRouterManager AddWindowClassRoute(string windowClass, IWorkspace workspace);

	/// <summary>
	/// Adds a router which moves processes matching <paramref name="processName"/> to the
	/// <paramref name="workspaceName"/>.
	/// </summary>
	/// <param name="processName"></param>
	/// <param name="workspaceName"></param>
	public IRouterManager AddProcessNameRoute(string processName, string workspaceName);

	/// <summary>
	/// Adds a router which moves processes matching <paramref name="processName"/> to the
	/// <paramref name="workspace"/>.
	/// </summary>
	/// <param name="processName"></param>
	/// <param name="workspace"></param>
	public IRouterManager AddProcessNameRoute(string processName, IWorkspace workspace);

	/// <summary>
	/// Adds a router which moves windows matching <paramref name="title"/> to the workspace
	/// with the name <paramref name="workspaceName"/>.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="workspaceName"></param>
	public IRouterManager AddTitleRoute(string title, string workspaceName);

	/// <summary>
	/// Adds a router which moves windows matching <paramref name="title"/> to the
	/// <paramref name="workspace"/>.
	/// </summary>
	/// <param name="title"></param>
	/// <param name="workspace"></param>
	public IRouterManager AddTitleRoute(string title, IWorkspace workspace);

	/// <summary>
	/// Adds a router which moves windows matching regex <paramref name="match"/> string to the
	/// <paramref name="workspaceName"/>.
	/// </summary>
	/// <param name="match"></param>
	/// <param name="workspaceName"></param>
	public IRouterManager AddTitleMatchRoute(string match, string workspaceName);

	/// <summary>
	/// Adds a router which moves windows matching regex <paramref name="match"/> string to the
	/// <paramref name="workspace"/>.
	/// </summary>
	/// <param name="match"></param>
	/// <param name="workspace"></param>
	public IRouterManager AddTitleMatchRoute(string match, IWorkspace workspace);
	#endregion
}
