using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Whim;

public class RouterManager : IRouterManager
{
	private readonly IConfigContext _configContext;
	private readonly List<Router> _routers = new();

	public RouterManager(IConfigContext configContext)
	{
		_configContext = configContext;
	}

	private void AddRouter(Router router)
	{
		_routers.Add(router);
	}


	public void Add(Router router)
	{
		AddRouter(router);
	}

	public void Clear()
	{
		Logger.Debug("Clearing routes");
		_routers.Clear();
	}

	public IRouterManager AddProcessNameRoute(string processName, string workspaceName)
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

	public IRouterManager AddProcessNameRoute(string processName, IWorkspace workspace)
	{
		processName = processName.ToLower();
		Logger.Debug($"Routing process name: {processName} to workspace {workspace}");
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

	public IRouterManager AddTitleRoute(string title, string workspaceName)
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

	public IRouterManager AddTitleRoute(string title, IWorkspace workspace)
	{
		title = title.ToLower();
		Logger.Debug($"Routing title: {title} to workspace {workspace}");
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

	public IRouterManager AddTitleMatchRoute(string match, string workspaceName)
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

	public IRouterManager AddTitleMatchRoute(string match, IWorkspace workspace)
	{
		Logger.Debug($"Routing title match: {match} to workspace {workspace}");
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

	public IRouterManager AddWindowClassRoute(string windowClass, string workspaceName)
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

	public IRouterManager AddWindowClassRoute(string windowClass, IWorkspace workspace)
	{
		windowClass = windowClass.ToLower();
		Logger.Debug($"Routing window class: {windowClass} to workspace {workspace}");
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
}
