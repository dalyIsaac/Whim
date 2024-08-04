using System.Text.RegularExpressions;

namespace Whim;

internal class RouterManager(IContext context) : IRouterManager
{
	private readonly IContext _context = context;
	private readonly List<Router> _routers = [];

	public RouterOptions RouterOptions { get; set; } = RouterOptions.RouteToLaunchedWorkspace;

	public void Add(Router router)
	{
		Logger.Debug($"Adding router {router}");
		_routers.Add(router);
	}

	public void Clear()
	{
		Logger.Debug("Clearing routes");
		_routers.Clear();
	}

	public IRouterManager AddProcessFileNameRoute(string processFileName, string workspaceName)
	{
		processFileName = processFileName.ToLower();
		Logger.Debug($"Routing process file name {processFileName} to workspace {workspaceName}");
		Add(window =>
		{
			if (window.ProcessFileName?.ToLower() == processFileName)
			{
				return _context.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
		return this;
	}

	public IRouterManager AddProcessFileNameRoute(string processFileName, IWorkspace workspace)
	{
		processFileName = processFileName.ToLower();
		Logger.Debug($"Routing process file name: {processFileName} to workspace {workspace}");
		Add(window =>
		{
			if (window.ProcessFileName?.ToLower() == processFileName)
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
		Add(window =>
		{
			if (window.Title.ToLower() == title)
			{
				return _context.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
		return this;
	}

	public IRouterManager AddTitleRoute(string title, IWorkspace workspace)
	{
		title = title.ToLower();
		Logger.Debug($"Routing title: {title} to workspace {workspace}");
		Add(window =>
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
		Add(window =>
		{
			if (regex.IsMatch(window.Title))
			{
				return _context.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
		return this;
	}

	public IRouterManager AddTitleMatchRoute(string match, IWorkspace workspace)
	{
		Logger.Debug($"Routing title match: {match} to workspace {workspace}");
		Regex regex = new(match);
		Add(window =>
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
		Add(window =>
		{
			if (window.WindowClass.ToLower() == windowClass)
			{
				return _context.WorkspaceManager.TryGet(workspaceName);
			}
			return null;
		});
		return this;
	}

	public IRouterManager AddWindowClassRoute(string windowClass, IWorkspace workspace)
	{
		windowClass = windowClass.ToLower();
		Logger.Debug($"Routing window class: {windowClass} to workspace {workspace}");
		Add(window =>
		{
			if (window.WindowClass.ToLower() == windowClass)
			{
				return workspace;
			}
			return null;
		});
		return this;
	}
}
