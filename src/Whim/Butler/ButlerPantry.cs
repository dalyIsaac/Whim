using System.Collections.Generic;

namespace Whim;

internal class ButlerPantry : IButlerPantry
{
	private readonly IContext _ctx;

	public ButlerPantry(IContext context)
	{
		_ctx = context;
	}

	public IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse = false, bool skipActive = false) =>
		_ctx.Store.Pick(Pickers.PickAdjacentWorkspace(workspace.Id, reverse, skipActive)).OrDefault();

	public IEnumerable<IWorkspace> GetAllActiveWorkspaces() => _ctx.Store.Pick(Pickers.PickAllActiveWorkspaces());

	public IMonitor? GetMonitorForWindow(IWindow window) =>
		_ctx.Store.Pick(Pickers.PickMonitorByWindow(window.Handle)).OrDefault();

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace) =>
		_ctx.Store.Pick(Pickers.PickMonitorByWorkspace(workspace.Id)).OrDefault();

	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor) =>
		_ctx.Store.Pick(Pickers.PickWorkspaceByMonitor(monitor.Handle)).OrDefault();

	public IWorkspace? GetWorkspaceForWindow(IWindow window) =>
		_ctx.Store.Pick(Pickers.PickWorkspaceByWindow(window.Handle)).OrDefault();
}
