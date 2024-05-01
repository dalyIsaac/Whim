namespace Whim;

internal class ButlerChores : IButlerChores
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	public ButlerChores(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	public void Activate(IWorkspace workspace, IMonitor? monitor = null) =>
		_context.Store.Dispatch(new ActivateWorkspaceTransform(workspace, monitor));

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.Dispatch(new ActivateAdjacentTransform(monitor, reverse, skipActive));

	public void LayoutAllActiveWorkspaces() => _context.Store.Dispatch(new LayoutAllActiveWorkspacesTransform());

	public void FocusMonitorDesktop(IMonitor monitor) =>
		_context.Store.Dispatch(new FocusMonitorDesktopTransform(monitor));

	public bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowEdgesInDirectionTransform(edges, pixelsDeltas, window)).IsSuccessful;

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentWorkspaceTransform(window, reverse, skipActive));

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToMonitorTransform(monitor, window));

	public void MoveWindowToPreviousMonitor(IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform(window, Reverse: true));

	public void MoveWindowToNextMonitor(IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform(window, Reverse: false));

	public void MoveWindowToPoint(IWindow window, IPoint<int> point) =>
		_context.Store.Dispatch(new MoveWindowToPointTransform(window, point));

	public void MoveWindowToWorkspace(IWorkspace targetWorkspace, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToWorkspaceTransform(targetWorkspace, window));

	public void MergeWorkspaceWindows(IWorkspace source, IWorkspace target) =>
		_context.Store.Dispatch(new MergeWorkspaceWindowsTransform(source, target));

	public void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false) =>
		_context.Store.Dispatch(new SwapWorkspaceWithAdjacentMonitorTransform(workspace, reverse));
}
