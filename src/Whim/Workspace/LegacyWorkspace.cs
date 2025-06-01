using System.Linq;

namespace Whim;

public partial record Workspace : IInternalWorkspace
{
	private readonly IContext _ctx;

	/// <summary>
	/// The latest instance of <see cref="IWorkspace"/> for the given <see cref="WorkspaceId"/>.
	/// This is used for compatibility with the old <see cref="Workspace"/> implementation.
	/// </summary>
	private Workspace LatestWorkspace => (Workspace)_ctx.Store.Pick(PickWorkspaceById(Id)).Value!;

	/// <inheritdoc/>
	public string Name
	{
		get => LatestWorkspace.BackingName;
		set => _ctx.Store.Dispatch(new SetWorkspaceNameTransform(Id, value));
	}

	private bool _disposedValue;

	/// <inheritdoc/>
	public IWindow? LastFocusedWindow => _ctx.Store.Pick(PickLastFocusedWindow(Id)).ValueOrDefault;

	/// <inheritdoc/>
	public ILayoutEngine ActiveLayoutEngine => _ctx.Store.Pick(PickActiveLayoutEngine(Id)).Value!;

	/// <inheritdoc/>
	public IEnumerable<IWindow> Windows => _ctx.Store.Pick(PickWorkspaceWindows(Id)).Value!;

	internal Workspace(IContext context, WorkspaceId id)
	{
		_ctx = context;
		Id = id;
	}

	/// <inheritdoc/>
	public void WindowFocused(IWindow? window)
	{
		if (window != null && Windows.Contains(window))
		{
			_ctx.Store.Dispatch(new SetLastFocusedWindowTransform(Id, window.Handle));
		}
	}

	/// <inheritdoc/>
	public void MinimizeWindowStart(IWindow window) =>
		_ctx.Store.Dispatch(new MinimizeWindowStartTransform(Id, window.Handle));

	/// <inheritdoc/>
	public void MinimizeWindowEnd(IWindow window) =>
		_ctx.Store.Dispatch(new MinimizeWindowEndTransform(Id, window.Handle));

	/// <inheritdoc/>
	public void FocusLastFocusedWindow() => _ctx.Store.Dispatch(new FocusWorkspaceTransform(Id));

	/// <inheritdoc/>
	public bool TrySetLayoutEngineFromIndex(int nextIdx) =>
		_ctx.Store.Dispatch(new SetLayoutEngineFromIndexTransform(Id, nextIdx)).IsSuccessful;

	/// <inheritdoc/>
	public void CycleLayoutEngine(bool reverse = false) =>
		_ctx.Store.Dispatch(new CycleLayoutEngineTransform(Id, reverse));

	/// <inheritdoc/>
	public void ActivatePreviouslyActiveLayoutEngine() =>
		_ctx.Store.Dispatch(new ActivatePreviouslyActiveLayoutEngineTransform(Id));

	/// <inheritdoc/>
	public bool TrySetLayoutEngineFromName(string name) =>
		_ctx.Store.Dispatch(new SetLayoutEngineFromNameTransform(Id, name)).IsSuccessful;

	/// <inheritdoc/>
	public bool AddWindow(IWindow window) =>
		_ctx.Store.Dispatch(new AddWindowToWorkspaceTransform(Id, window)).IsSuccessful;

	/// <inheritdoc/>
	public bool RemoveWindow(IWindow window) =>
		_ctx.Store.Dispatch(new RemoveWindowFromWorkspaceTransform(Id, window)).IsSuccessful;

	/// <inheritdoc/>
	public bool FocusWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false) =>
		_ctx.Store.Dispatch(new FocusWindowInDirectionTransform(Id, direction, window?.Handle ?? default))
			.TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public bool SwapWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false) =>
		_ctx.Store.Dispatch(new SwapWindowInDirectionTransform(Id, direction, window?.Handle ?? default))
			.TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public bool MoveWindowEdgesInDirection(
		Direction edges,
		IPoint<double> deltas,
		IWindow? window = null,
		bool deferLayout = false
	) =>
		_ctx.Store.Dispatch(
				new MoveWindowEdgesInDirectionWorkspaceTransform(Id, edges, deltas, window?.Handle ?? default)
			)
			.TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public bool MoveWindowToPoint(IWindow window, IPoint<double> point, bool deferLayout = false) =>
		_ctx.Store.Dispatch(new MoveWindowToPointInWorkspaceTransform(Id, window.Handle, point))
			.TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public override string ToString() => Name;

	/// <inheritdoc/>
	public void Deactivate() => _ctx.Store.Dispatch(new DeactivateWorkspaceTransform(Id));

	/// <inheritdoc/>
	public IWindowState? TryGetWindowState(IWindow window)
	{
		if (_ctx.Store.Pick(PickWindowPosition(Id, window.Handle)).TryGet(out WindowPosition pos))
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
	public void DoLayout() => _ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(Id));

	/// <inheritdoc/>
	public bool ContainsWindow(IWindow window) => _ctx.Store.Pick(PickWorkspaceWindows(Id)).Value!.Contains(window);

	/// <inheritdoc/>
	public bool PerformCustomLayoutEngineAction(LayoutEngineCustomAction action) =>
		_ctx.Store.Dispatch(new LayoutEngineCustomActionTransform(Id, action)).TryGet(out bool isChanged) && isChanged;

	/// <inheritdoc/>
	public bool PerformCustomLayoutEngineAction<T>(LayoutEngineCustomAction<T> action) =>
		_ctx.Store.Dispatch(new LayoutEngineCustomActionWithPayloadTransform<T>(Id, action)).TryGet(out bool isChanged)
		&& isChanged;

	/// <inheritdoc/>
	public void Dispose()
	{
		if (_disposedValue)
		{
			return;
		}

		Logger.Debug($"Disposing workspace {BackingName}");

		bool isWorkspaceActive = _ctx.Store.Pick(PickMonitorByWorkspace(Id)).IsSuccessful;

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
