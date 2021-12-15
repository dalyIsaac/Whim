using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Whim.Core;

using Filter = Func<IWindow, bool>;
using Router = Func<IWindow, IWorkspace?>;

public class RouterManager : IRouterManager
{
	private readonly IConfigContext _configContext;
	private readonly List<Filter> _filters = new();
	private readonly List<Router> _routers = new();

	public RouterManager(IConfigContext configContext)
	{
		_configContext = configContext;
		AddDefaultFilters(this);
	}

	private void AddFilter(Filter filter)
	{
		Logger.Debug("Adding filter");
		_filters.Add(filter);
	}

	private void AddRouter(Router router)
	{
		Logger.Debug("Adding router");
		_routers.Add(router);
	}

	public void Add(Filter filter)
	{
		AddFilter(filter);
	}

	public void Add(Router router)
	{
		AddRouter(router);
	}

	public void ClearFilters(bool clearDefaults = false)
	{
		Logger.Debug("Clearing filters. Defaults being cleared: {0}", clearDefaults);
		_filters.Clear();
		if (!clearDefaults)
		{
			AddDefaultFilters(this);
		}
	}

	public void ClearRoutes()
	{
		Logger.Debug("Clearing routes");
		_routers.Clear();
	}

	public void IgnoreProcessName(string processName)
	{
		processName = processName.ToLower();
		Logger.Debug("Ignoring process name: {0}", processName);
		AddFilter(window => window.ProcessName.ToLower() != processName);
	}

	public void IgnoreTitle(string title)
	{
		title = title.ToLower();
		Logger.Debug("Ignoring title: {0}", title);
		AddFilter(window => window.Title.ToLower() != title);
	}

	public void IgnoreTitleMatch(string match)
	{
		Logger.Debug("Ignoring title match: {0}", match);
		Regex regex = new(match);
		AddFilter(window => !regex.IsMatch(window.Title));
	}

	public void IgnoreWindowClass(string windowClass)
	{
		windowClass = windowClass.ToLower();
		Logger.Debug("Ignoring window class: {0}", windowClass);
		AddFilter(window => window.Class.ToLower() != windowClass);
	}

	public void RouteProcessName(string processName, string workspaceName)
	{
		processName = processName.ToLower();
		Logger.Debug("Routing process name: {0} to workspace: {1}", processName, workspaceName);
		AddRouter(window =>
		{
			if (window.ProcessName.ToLower() == processName)
			{
				return _configContext.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
	}

	public void RouteProcessName(string processName, IWorkspace workspace)
	{
		processName = processName.ToLower();
		Logger.Debug("Routing process name: {0} to workspace: {1}", processName, workspace.Name);
		AddRouter(window =>
		{
			if (window.ProcessName.ToLower() == processName)
			{
				return workspace;
			}
			return null;
		});
	}

	public void RouteTitle(string title, string workspaceName)
	{
		title = title.ToLower();
		Logger.Debug("Routing title: {0} to workspace: {1}", title, workspaceName);
		AddRouter(window =>
		{
			if (window.Title.ToLower() == title)
			{
				return _configContext.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
	}

	public void RouteTitle(string title, IWorkspace workspace)
	{
		title = title.ToLower();
		Logger.Debug("Routing title: {0} to workspace: {1}", title, workspace.Name);
		AddRouter(window =>
		{
			if (window.Title.ToLower() == title)
			{
				return workspace;
			}
			return null;
		});
	}

	public void RouteTitleMatch(string match, string workspaceName)
	{
		Logger.Debug("Routing title match: {0} to workspace: {1}", match, workspaceName);
		Regex regex = new(match);
		AddRouter(window =>
		{
			if (regex.IsMatch(window.Title))
			{
				return _configContext.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
	}

	public void RouteTitleMatch(string match, IWorkspace workspace)
	{
		Logger.Debug("Routing title match: {0} to workspace: {1}", match, workspace.Name);
		Regex regex = new(match);
		AddRouter(window =>
		{
			if (regex.IsMatch(window.Title))
			{
				return workspace;
			}
			return null;
		});
	}

	public IWorkspace? RouteWindow(IWindow window)
	{
		Logger.Debug("Routing window {window}", window);
		foreach (Filter filter in _filters)
		{
			if (!filter(window))
			{
				Logger.Debug("Window {window} filtered out", window);
				return null;
			}
		}

		foreach (Router router in _routers)
		{
			IWorkspace? workspace = router(window);
			if (workspace != null)
			{
				Logger.Debug("Window {window} routed to workspace: {workspace}", window, workspace);
				return workspace;
			}
		}
		Logger.Debug("Window {window} not routed", window);
		return null;
	}

	public void RouteWindowClass(string windowClass, string workspaceName)
	{
		windowClass = windowClass.ToLower();
		Logger.Debug("Routing window class: {0} to workspace: {1}", windowClass, workspaceName);
		AddRouter(window =>
		{
			if (window.Class.ToLower() == windowClass)
			{
				return _configContext.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
	}

	public void RouteWindowClass(string windowClass, IWorkspace workspace)
	{
		windowClass = windowClass.ToLower();
		Logger.Debug("Routing window class: {0} to workspace: {1}", windowClass, workspace.Name);
		AddRouter(window =>
		{
			if (window.Class.ToLower() == windowClass)
			{
				return workspace;
			}
			return null;
		});
	}

	/// <summary>
	/// Returns Whim's default filters.
	/// </summary>
	/// <returns></returns>
	public static void AddDefaultFilters(IRouterManager router)
	{
		router.IgnoreWindowClass("TaskManagerWindow");
	}
}
