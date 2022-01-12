namespace Whim;

public delegate IWorkspace? Router(IWindow window);

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
	public IRouterManager AddWindowClassRoute(string windowClass, string workspaceName);
	public IRouterManager AddWindowClassRoute(string windowClass, IWorkspace workspace);

	public IRouterManager AddProcessNameRoute(string processName, string workspaceName);
	public IRouterManager AddProcessNameRoute(string processName, IWorkspace workspace);

	public IRouterManager AddTitleRoute(string title, string workspaceName);
	public IRouterManager AddTitleRoute(string title, IWorkspace workspace);
	public IRouterManager AddTitleMatchRoute(string match, string workspaceName);
	public IRouterManager AddTitleMatchRoute(string match, IWorkspace workspace);
	#endregion
}
