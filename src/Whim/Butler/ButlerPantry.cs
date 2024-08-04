namespace Whim;

internal class ButlerPantry : IButlerPantry
{
	private readonly IContext _ctx;

	public ButlerPantry(IContext context)
	{
		_ctx = context;
	}

	public IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse = false, bool skipActive = false) =>
		_ctx.Store.Pick(PickAdjacentWorkspace(workspace.Id, reverse, skipActive)).ValueOrDefault;

	public IEnumerable<IWorkspace> GetAllActiveWorkspaces() => _ctx.Store.Pick(PickAllActiveWorkspaces());

	public IMonitor? GetMonitorForWindow(IWindow window) =>
		_ctx.Store.Pick(PickMonitorByWindow(window.Handle)).ValueOrDefault;

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace) =>
		_ctx.Store.Pick(PickMonitorByWorkspace(workspace.Id)).ValueOrDefault;

	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor) =>
		_ctx.Store.Pick(PickWorkspaceByMonitor(monitor.Handle)).ValueOrDefault;

	public IWorkspace? GetWorkspaceForWindow(IWindow window) =>
		_ctx.Store.Pick(PickWorkspaceByWindow(window.Handle)).ValueOrDefault;
}
