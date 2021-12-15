using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whim.Core;

using Filter = Func<IWindow, bool>;
using Router = Func<IWindow, IWorkspace?>;

public interface IRouterManager
{
	/// <summary>
	///
	/// </summary>
	/// <param name="window"></param>
	/// <returns>
	/// <see langword="null"/> when the window should be ignored, otherwise the
	/// <see cref="IWorkspace"/> to route the window to.
	/// </returns>
	public IWorkspace? RouteWindow(IWindow window);

	/// <summary>
	/// Clears all the filters.
	/// </summary>
	/// <param name="clearDefaults">Indicates whether the default filters should be retained.</param>
	public void ClearFilters(bool clearDefaults = false);

	/// <summary>
	/// Clear all the routes.
	/// </summary>
	public void ClearRoutes();

	/// <summary>
	/// Add a filter.
	/// </summary>
	/// <param name="filter"></param>
	public void Add(Filter filter);

	/// <summary>
	/// Add a router.
	/// </summary>
	/// <param name="router"></param>
	public void Add(Router router);

	#region Filters helper methods
	/// <summary>
	/// Ignores the window class. Case insensitive.
	/// </summary>
	/// <param name="windowClass"></param>
	public void IgnoreWindowClass(string windowClass);

	/// <summary>
	/// Ignores the process name. Case insensitive.
	/// </summary>
	/// <param name="processName"></param>
	public void IgnoreProcessName(string processName);

	/// <summary>
	/// Ignores the title. Case insensitive.
	/// </summary>
	/// <param name="title"></param>
	public void IgnoreTitle(string title);

	/// <summary>
	/// Filter the title according to the regex pattern.
	/// </summary>
	/// <param name="match"></param>
	public void IgnoreTitleMatch(string match);
	#endregion

	#region Routers helper methods
	public void RouteWindowClass(string windowClass, string workspaceName);
	public void RouteWindowClass(string windowClass, IWorkspace workspace);

	public void RouteProcessName(string processName, string workspaceName);
	public void RouteProcessName(string processName, IWorkspace workspace);

	public void RouteTitle(string title, string workspaceName);
	public void RouteTitle(string title, IWorkspace workspace);
	public void RouteTitleMatch(string match, string workspaceName);
	public void RouteTitleMatch(string match, IWorkspace workspace);
	#endregion
}
