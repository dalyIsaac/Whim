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

	public void MergeWorkspaceWindows(IWorkspace source, IWorkspace target)
	{
		_context.Butler.Pantry.MergeWorkspaceWindows(source, target);

		foreach (IWindow window in source.Windows)
		{
			target.AddWindow(window);
		}
	}

	public void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false)
	{
		// Get the current monitor.
		workspace ??= _context.WorkspaceManager.ActiveWorkspace;
		IMonitor? currentMonitor = _context.Butler.Pantry.GetMonitorForWorkspace(workspace);

		if (currentMonitor == null)
		{
			Logger.Error($"Workspace {workspace} was not found in any monitor");
			return;
		}

		// Get the next monitor.
		IMonitor nextMonitor = reverse
			? _context.MonitorManager.GetPreviousMonitor(currentMonitor)
			: _context.MonitorManager.GetNextMonitor(currentMonitor);

		if (currentMonitor.Equals(nextMonitor))
		{
			Logger.Error($"Monitor {currentMonitor} is already the {(!reverse ? "next" : "previous")} monitor");
			return;
		}

		// Get workspace on next monitor.
		IWorkspace? nextWorkspace = _context.Butler.Pantry.GetWorkspaceForMonitor(nextMonitor);
		if (nextWorkspace == null)
		{
			Logger.Error($"Monitor {nextMonitor} was not found to correspond to any workspace");
			return;
		}

		Activate(nextWorkspace, currentMonitor);
	}
}
