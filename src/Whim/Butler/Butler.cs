namespace Whim;

[Obsolete("This class is obsolete and will be removed in a future version.")]
internal partial class Butler : IButler
{
	private readonly IContext _context;
	private bool _disposedValue;

	public IButlerPantry Pantry { get; }

	public event EventHandler<RouteEventArgs>? WindowRouted;

	public event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;

	public Butler(IContext context)
	{
		_context = context;

		Pantry = new ButlerPantry(_context);
	}

	public void Initialize()
	{
		_context.Store.MapEvents.WindowRouted += ForwardWindowRouted;
		_context.Store.MapEvents.MonitorWorkspaceChanged += ForwardMonitorWorkspaceChanged;
	}

	private void ForwardWindowRouted(object? sender, RouteEventArgs e) => WindowRouted?.Invoke(this, e);

	private void ForwardMonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs e) =>
		MonitorWorkspaceChanged?.Invoke(this, e);

	#region Chores
	public void Activate(IWorkspace workspace, IMonitor? monitor = null) =>
		_context.Store.WhimDispatch(new ActivateWorkspaceTransform(workspace.Id, monitor?.Handle ?? default));

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.WhimDispatch(
			new ActivateAdjacentWorkspaceTransform(monitor?.Handle ?? default, reverse, skipActive)
		);

	public void LayoutAllActiveWorkspaces() => _context.Store.WhimDispatch(new LayoutAllActiveWorkspacesTransform());

	public void FocusMonitorDesktop(IMonitor monitor) =>
		_context.Store.WhimDispatch(new FocusMonitorDesktopTransform(monitor?.Handle ?? default));

	public bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null) =>
		_context
			.Store.WhimDispatch(new MoveWindowEdgesInDirectionTransform(edges, pixelsDeltas, window?.Handle ?? default))
			.IsSuccessful;

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.Dispatch(
			new MoveWindowToAdjacentWorkspaceTransform(window?.Handle ?? default, reverse, skipActive)
		);

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null) =>
		_context.Store.Dispatch(
			new MoveWindowToMonitorTransform(monitor?.Handle ?? default, window?.Handle ?? default)
		);

	public void MoveWindowToPreviousMonitor(IWindow? window = null) =>
		_context.Store.WhimDispatch(new MoveWindowToAdjacentMonitorTransform(window?.Handle ?? default, Reverse: true));

	public void MoveWindowToNextMonitor(IWindow? window = null) =>
		_context.Store.WhimDispatch(
			new MoveWindowToAdjacentMonitorTransform(window?.Handle ?? default, Reverse: false)
		);

	public void MoveWindowToPoint(IWindow window, IPoint<int> point) =>
		_context.Store.Dispatch(new MoveWindowToPointTransform(window?.Handle ?? default, point));

	public void MoveWindowToWorkspace(IWorkspace targetWorkspace, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToWorkspaceTransform(targetWorkspace.Id, window?.Handle ?? default));

	public void MergeWorkspaceWindows(IWorkspace source, IWorkspace target) =>
		_context.Store.WhimDispatch(new MergeWorkspaceWindowsTransform(source.Id, target.Id));

	public void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false) =>
		_context.Store.Dispatch(new SwapWorkspaceWithAdjacentMonitorTransform(workspace?.Id ?? default, reverse));

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)

				_context.Store.MapEvents.WindowRouted -= WindowRouted;
				_context.Store.MapEvents.MonitorWorkspaceChanged -= MonitorWorkspaceChanged;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
