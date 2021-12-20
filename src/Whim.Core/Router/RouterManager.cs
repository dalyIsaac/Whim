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
		Logger.Debug($"Clearing filters. Defaults being cleared: {clearDefaults}");
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

	public IRouterManager IgnoreProcessName(string processName)
	{
		processName = processName.ToLower();
		Logger.Debug($"Ignoring process name: {processName}");
		AddFilter(window => window.ProcessName.ToLower() != processName);
		return this;
	}

	public IRouterManager IgnoreTitle(string title)
	{
		title = title.ToLower();
		Logger.Debug($"Ignoring title: {title}");
		AddFilter(window => window.Title.ToLower() != title);
		return this;
	}

	public IRouterManager IgnoreTitleMatch(string match)
	{
		Logger.Debug($"Ignoring title match: {match}");
		Regex regex = new(match);
		AddFilter(window => !regex.IsMatch(window.Title));
		return this;
	}

	public IRouterManager IgnoreWindowClass(string windowClass)
	{
		windowClass = windowClass.ToLower();
		Logger.Debug($"Ignoring window class: {windowClass}");
		AddFilter(window => window.Class.ToLower() != windowClass);
		return this;
	}

	public IRouterManager RouteProcessName(string processName, string workspaceName)
	{
		processName = processName.ToLower();
		Logger.Debug($"Routing process name: {processName} to workspace {workspaceName}");
		AddRouter(window =>
		{
			if (window.ProcessName.ToLower() == processName)
			{
				return _configContext.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
		return this;
	}

	public IRouterManager RouteProcessName(string processName, IWorkspace workspace)
	{
		processName = processName.ToLower();
		Logger.Debug($"Routing process name: {processName} to workspace {workspace.Name}");
		AddRouter(window =>
		{
			if (window.ProcessName.ToLower() == processName)
			{
				return workspace;
			}
			return null;
		});
		return this;
	}

	public IRouterManager RouteTitle(string title, string workspaceName)
	{
		title = title.ToLower();
		Logger.Debug($"Routing title: {title} to workspace {workspaceName}");
		AddRouter(window =>
		{
			if (window.Title.ToLower() == title)
			{
				return _configContext.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
		return this;
	}

	public IRouterManager RouteTitle(string title, IWorkspace workspace)
	{
		title = title.ToLower();
		Logger.Debug($"Routing title: {title} to workspace {workspace.Name}");
		AddRouter(window =>
		{
			if (window.Title.ToLower() == title)
			{
				return workspace;
			}
			return null;
		});
		return this;
	}

	public IRouterManager RouteTitleMatch(string match, string workspaceName)
	{
		Logger.Debug($"Routing title match: {match} to workspace {workspaceName}");
		Regex regex = new(match);
		AddRouter(window =>
		{
			if (regex.IsMatch(window.Title))
			{
				return _configContext.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
		return this;
	}

	public IRouterManager RouteTitleMatch(string match, IWorkspace workspace)
	{
		Logger.Debug($"Routing title match: {match} to workspace {workspace.Name}");
		Regex regex = new(match);
		AddRouter(window =>
		{
			if (regex.IsMatch(window.Title))
			{
				return workspace;
			}
			return null;
		});
		return this;
	}

	public IWorkspace? RouteWindow(IWindow window)
	{
		Logger.Debug($"Routing window {window}");
		if (!FilterWindow(window))
		{
			return null;
		}

		foreach (Router router in _routers)
		{
			IWorkspace? workspace = router(window);
			if (workspace != null)
			{
				Logger.Debug($"Window {window} routed to workspace: {workspace}");
				return workspace;
			}
		}
		Logger.Debug($"Window {window} not routed");
		return null;
	}

	public bool FilterWindow(IWindow window)
	{
		Logger.Debug($"Filtering window \"{window}\"");
		foreach (Filter filter in _filters)
		{
			if (!filter(window))
			{
				Logger.Debug($"Window {window} filtered out");
				return true;
			}
		}
		Logger.Debug($"Window {window} not filtered");
		return false;
	}

	public IRouterManager RouteWindowClass(string windowClass, string workspaceName)
	{
		windowClass = windowClass.ToLower();
		Logger.Debug($"Routing window class: {windowClass} to workspace {workspaceName}");
		AddRouter(window =>
		{
			if (window.Class.ToLower() == windowClass)
			{
				return _configContext.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
		return this;
	}

	public IRouterManager RouteWindowClass(string windowClass, IWorkspace workspace)
	{
		windowClass = windowClass.ToLower();
		Logger.Debug($"Routing window class: {windowClass} to workspace {workspace.Name}");
		AddRouter(window =>
		{
			if (window.Class.ToLower() == windowClass)
			{
				return workspace;
			}
			return null;
		});
		return this;
	}

	/// <summary>
	/// Returns Whim's default filters.
	/// </summary>
	/// <returns></returns>
	public static void AddDefaultFilters(IRouterManager router)
	{
		router.IgnoreWindowClass("TaskManagerWindow")
			  .IgnoreProcessName("ShellExperienceHost")
			  .IgnoreProcessName("SearchHost") // Windows 11 search
			  .IgnoreWindowClass("Shell_TrayWnd") // Windows 11 start
			  .IgnoreProcessName("ScreenClippingHost") // Windows 10 screen clipping
              .IgnoreWindowClass("MSCTFIME UI") // Windows 10 IME
			  .IgnoreWindowClass("Xaml_WindowedPopupClass");
	}
}
