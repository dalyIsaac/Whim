using System.Linq;

namespace Whim;

public partial record Workspace : IInternalWorkspace
{
	private readonly IContext _context;

	/// <summary>
	/// The latest instance of <see cref="IWorkspace"/> for the given <see cref="WorkspaceId"/>.
	/// This is used for compatibility with the old <see cref="Workspace"/> implementation.
	/// </summary>
	private Workspace LatestWorkspace => (Workspace)_context.Store.Pick(PickWorkspaceById(Id)).Value;

	/// <inheritdoc/>
	public string Name
	{
		get => LatestWorkspace.BackingName;
		set => _context.Store.Dispatch(new SetWorkspaceNameTransform(Id, value));
	}

	private bool _disposedValue;

	/// <inheritdoc/>
	public IWindow? LastFocusedWindow => _context.Store.Pick(PickLastFocusedWindow(Id)).ValueOrDefault;

	/// <inheritdoc/>
	public ILayoutEngine ActiveLayoutEngine => _context.Store.Pick(PickActiveLayoutEngine(Id)).Value!;

	/// <inheritdoc/>
	public IEnumerable<IWindow> Windows => _context.Store.Pick(PickAllWindowsInWorkspace(Id)).Value!;

	internal Workspace(IContext context, WorkspaceId id)
	{
		_context = context;
		Id = id;
	}

	/// <inheritdoc/>
	public void WindowFocused(IWindow? window)
	{
		if (window != null && Windows.Contains(window))
		{
			_context.Store.Dispatch(new SetLastFocusedWindowTransform(Id, window.Handle));
		}
	}

	/// <inheritdoc/>
	public void MinimizeWindowStart(IWindow window) =>
		_context.Store.Dispatch(new MinimizeWindowStartTransform(Id, window.Handle));

	/// <inheritdoc/>
	public void MinimizeWindowEnd(IWindow window) =>
		_context.Store.Dispatch(new MinimizeWindowEndTransform(Id, window.Handle));

	/// <inheritdoc/>
	public void FocusLastFocusedWindow() => _context.Store.Dispatch(new FocusWindowTransform(Id));

	/// <inheritdoc/>
	public bool TrySetLayoutEngineFromIndex(int nextIdx) =>
		_context.Store.Dispatch(new ActivateLayoutEngineTransform(Id, (_, idx) => idx == nextIdx)).IsSuccessful;

	/// <inheritdoc/>
	public void CycleLayoutEngine(bool reverse = false) =>
		_context.Store.Dispatch(new CycleLayoutEngineTransform(Id, reverse));

	/// <inheritdoc/>
	public void ActivatePreviouslyActiveLayoutEngine() =>
		_context.Store.Dispatch(
			new ActivateLayoutEngineTransform(Id, (_, idx) => idx == LatestWorkspace.PreviousLayoutEngineIndex)
		);

	/// <inheritdoc/>
	public bool TrySetLayoutEngineFromName(string name) =>
		_context.Store.Dispatch(new ActivateLayoutEngineTransform(Id, (l, _) => l.Name == name)).IsSuccessful;

	/// <inheritdoc/>
	public bool AddWindow(IWindow window) =>
		_context.Store.Dispatch(new AddWindowToWorkspaceTransform(Id, window)).IsSuccessful;

	/// <inheritdoc/>
	public bool RemoveWindow(IWindow window) =>
		_context.Store.Dispatch(new RemoveWindowFromWorkspaceTransform(Id, window)).IsSuccessful;

	/// <inheritdoc/>
	public bool FocusWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false) =>
		_context
			.Store.Dispatch(new FocusWindowInDirectionTransform(Id, window?.Handle ?? default, direction))
			.TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public bool SwapWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false) =>
		_context
			.Store.Dispatch(new SwapWindowInDirectionTransform(Id, window?.Handle ?? default, direction))
			.TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public bool MoveWindowEdgesInDirection(
		Direction edges,
		IPoint<double> deltas,
		IWindow? window = null,
		bool deferLayout = false
	) =>
		_context
			.Store.Dispatch(
				new MoveWindowEdgesInDirectionWorkspaceTransform(Id, edges, deltas, window?.Handle ?? default)
			)
			.TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public bool MoveWindowToPoint(IWindow window, IPoint<double> point, bool deferLayout = false) =>
		_context
			.Store.Dispatch(new MoveWindowToPointInWorkspaceTransform(Id, window.Handle, point))
			.TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public override string ToString() => Name;

	/// <inheritdoc/>
	public void Deactivate() => _context.Store.Dispatch(new DeactivateWorkspaceTransform(Id));

	/// <inheritdoc/>
	public IWindowState? TryGetWindowState(IWindow window)
	{
		if (WindowPositions.TryGetValue(window.Handle, out WindowPosition? pos))
		{
			return new WindowState
			{
				Window = window,
				Rectangle = pos.LastWindowRectangle,
				WindowSize = pos.WindowSize,
			};
		}

		return null;
	}

	/// <inheritdoc/>
	public void DoLayout() => _context.Store.Dispatch(new DoWorkspaceLayoutTransform(Id));

	/// <inheritdoc/>
	public bool ContainsWindow(IWindow window) => LatestWorkspace.WindowPositions.ContainsKey(window.Handle);

	/// <inheritdoc/>
	public bool PerformCustomLayoutEngineAction(LayoutEngineCustomAction action) =>
		_context.Store.Dispatch(new LayoutEngineCustomActionTransform(Id, action)).TryGet(out bool isChanged)
		&& isChanged;

	/// <inheritdoc/>
	public bool PerformCustomLayoutEngineAction<T>(LayoutEngineCustomAction<T> action) =>
		_context
			.Store.Dispatch(new LayoutEngineCustomActionWithPayloadTransform<T>(Id, action))
			.TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public void Dispose()
	{
		if (_disposedValue)
		{
			return;
		}

		Logger.Debug($"Disposing workspace {BackingName}");

		bool isWorkspaceActive = _context.Store.Pick(PickMonitorByWorkspace(Id)).IsSuccessful;

		// If the workspace isn't active on the monitor, show all the windows in as minimized.
		if (!isWorkspaceActive)
		{
			foreach (IWindow window in Windows)
			{
				window.ShowMinimized();
			}
		}

		_disposedValue = true;
	}
}
