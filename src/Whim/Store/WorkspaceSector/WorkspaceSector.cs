namespace Whim;

/// <summary>
/// A workspace's name, layout engines, and monitor indices.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Name"></param>
/// <param name="CreateLeafLayoutEngines"></param>
/// <param name="MonitorIndices"></param>
internal record WorkspaceToCreate(
	WorkspaceId WorkspaceId,
	string? Name,
	IEnumerable<CreateLeafLayoutEngine>? CreateLeafLayoutEngines,
	IEnumerable<int>? MonitorIndices
);

internal class WorkspaceSector(IContext ctx, IInternalContext internalCtx)
	: SectorBase,
		IWorkspaceSector,
		IWorkspaceSectorEvents,
		IDisposable
{
	private readonly IContext _ctx = ctx;
	private readonly IInternalContext _internalCtx = internalCtx;

	public bool HasInitialized { get; set; }

	public ImmutableList<WorkspaceToCreate> WorkspacesToCreate { get; set; } = [];

	public ImmutableHashSet<WorkspaceId> WorkspacesToLayout { get; set; } = [];

	public HWND WindowHandleToFocus { get; set; }

	public ImmutableArray<WorkspaceId> WorkspaceOrder { get; set; } = [];

	public ImmutableDictionary<WorkspaceId, Workspace> Workspaces { get; set; } =
		ImmutableDictionary<WorkspaceId, Workspace>.Empty;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; } = () => [];

	public ImmutableList<ProxyLayoutEngineCreator> ProxyLayoutEngineCreators { get; set; } = [];

	public event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;

	public event EventHandler<WorkspaceRemovedEventArgs>? WorkspaceRemoved;

	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	public event EventHandler<WorkspaceLayoutStartedEventArgs>? WorkspaceLayoutStarted;

	public event EventHandler<WorkspaceLayoutCompletedEventArgs>? WorkspaceLayoutCompleted;

	public override void Initialize()
	{
		Logger.Information("Initializing WorkspaceSector");
		_ctx.Store.Dispatch(new InitializeWorkspacesTransform());
	}

	public override void DispatchEvents()
	{
		foreach (EventArgs eventArgs in _events)
		{
			switch (eventArgs)
			{
				case WorkspaceAddedEventArgs args:
					WorkspaceAdded?.Invoke(this, args);
					break;
				case WorkspaceRemovedEventArgs args:
					WorkspaceRemoved?.Invoke(this, args);
					break;
				case WorkspaceRenamedEventArgs args:
					WorkspaceRenamed?.Invoke(this, args);
					break;
				case ActiveLayoutEngineChangedEventArgs args:
					ActiveLayoutEngineChanged?.Invoke(this, args);
					break;
				default:
					break;
			}
		}

		_events.Clear();
	}

	public void DoLayout()
	{
		Logger.Debug("Doing layout");

		// Force the window to focus to not be minimized.
		if (WindowHandleToFocus != default)
		{
			if (_ctx.Store.Pick(PickWorkspaceByWindow(WindowHandleToFocus)).TryGet(out IWorkspace workspace))
			{
				WorkspacesToLayout = WorkspacesToLayout.Add(workspace.Id);

				// Force the window to not be minimized.
				_ctx.Store.Dispatch(new MinimizeWindowEndTransform(workspace.Id, WindowHandleToFocus));
			}
		}

		GarbageCollect();
		LayoutAllWorkspaces();
		FocusHandle();
	}

	private void GarbageCollect()
	{
		foreach (IWindow window in _ctx.Store.Pick(PickAllWindows()))
		{
			if (_internalCtx.CoreNativeManager.IsWindow(window.Handle))
			{
				continue;
			}

			Logger.Debug($"Window {window.Handle} is no longer a window.");
			_ctx.Store.Dispatch(new WindowRemovedTransform(window));
		}
	}

	private void LayoutAllWorkspaces()
	{
		foreach (WorkspaceId id in WorkspacesToLayout)
		{
			if (Workspaces.TryGetValue(id, out Workspace? workspace))
			{
				DoLayout(workspace);
			}
			else
			{
				Logger.Error($"Could not find workspace with id {id}");
			}
		}

		WorkspacesToLayout = WorkspacesToLayout.Clear();
	}

	private void FocusHandle()
	{
		if (WindowHandleToFocus == default)
		{
			return;
		}

		if (_ctx.Store.Pick(PickWindowByHandle(WindowHandleToFocus)).TryGet(out IWindow window))
		{
			window.Focus();
		}
		else
		{
			WindowHandleToFocus.Focus(_internalCtx);
		}

		_ctx.Store.Dispatch(new WindowFocusedTransform(window));
		WindowHandleToFocus = default;
	}

	private void DoLayout(Workspace workspace)
	{
		Logger.Debug($"Layout {workspace}");

		// Get the monitor for this workspace
		if (!_ctx.Store.Pick(PickMonitorByWorkspace(workspace.Id)).TryGet(out IMonitor monitor))
		{
			Logger.Debug($"No active monitors found for workspace {workspace}.");
			return;
		}

		Logger.Debug($"Starting layout for workspace {workspace}");

		_ctx.NativeManager.TryEnqueue(
			() => WorkspaceLayoutStarted?.Invoke(this, new WorkspaceLayoutStartedEventArgs { Workspace = workspace })
		);

		// Execute the layout task, and update the window states before the completed event.
		SetWindowPositions(workspace, monitor);

		_ctx.NativeManager.TryEnqueue(
			() =>
				WorkspaceLayoutCompleted?.Invoke(this, new WorkspaceLayoutCompletedEventArgs { Workspace = workspace })
		);
	}

	private void SetWindowPositions(Workspace workspace, IMonitor monitor)
	{
		Logger.Debug($"Setting window positions for workspace {workspace}");

		ImmutableDictionary<HWND, WindowPosition> windowPositions = workspace.WindowPositions;

		using DeferWindowPosHandle handle = _ctx.NativeManager.DeferWindowPos();

		foreach (IWindowState loc in workspace.GetActiveLayoutEngine().DoLayout(monitor.WorkingArea, monitor))
		{
			HWND hwnd = loc.Window.Handle;
			IRectangle<int> rect = loc.Rectangle;

			windowPositions = windowPositions.SetItem(hwnd, new WindowPosition(loc.WindowSize, rect));
			handle.DeferWindowPos(new DeferWindowPosState(hwnd, loc.WindowSize, rect));

			Logger.Debug($"Window {loc.Window} has rectangle {loc.Rectangle}");
		}

		Workspaces = Workspaces.SetItem(workspace.Id, workspace with { WindowPositions = windowPositions });
	}

	public void Dispose()
	{
		foreach (Workspace workspace in Workspaces.Values)
		{
			workspace.Dispose();
		}
	}
}
