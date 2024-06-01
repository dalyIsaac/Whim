using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// A workspace's name and layout engines.
/// </summary>
/// <param name="Name"></param>
/// <param name="LayoutEngines"></param>
internal record WorkspaceToCreate(string? Name, IEnumerable<CreateLeafLayoutEngine>? LayoutEngines);

internal class WorkspaceSector : SectorBase, IWorkspaceSector, IWorkspaceSectorEvents
{
	private readonly IContext _ctx;

	private readonly IInternalContext _internalCtx;

	public bool HasInitialized { get; set; }

	public ImmutableList<WorkspaceToCreate> WorkspacesToCreate { get; set; } = ImmutableList<WorkspaceToCreate>.Empty;

	/// <summary>
	/// The IDs of the workspaces that should be laid out.
	/// </summary>
	public ImmutableHashSet<WorkspaceId> WorkspacesToLayout { get; set; } = ImmutableHashSet<WorkspaceId>.Empty;

	public ImmutableArray<WorkspaceId> WorkspaceOrder { get; set; } = ImmutableArray<WorkspaceId>.Empty;

	public ImmutableDictionary<WorkspaceId, Workspace> Workspaces { get; set; } =
		ImmutableDictionary<WorkspaceId, Workspace>.Empty;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; } =
		() => new CreateLeafLayoutEngine[] { (id) => new ColumnLayoutEngine(id) };

	public ImmutableList<ProxyLayoutEngineCreator> ProxyLayoutEngineCreators { get; set; } =
		ImmutableList<ProxyLayoutEngineCreator>.Empty;

	// TODO
	public WorkspaceId ActiveWorkspaceId { get; set; }

	public event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;

	public event EventHandler<WorkspaceRemovedEventArgs>? WorkspaceRemoved;

	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	public event EventHandler<WorkspaceLayoutStartedEventArgs>? WorkspaceLayoutStarted;

	public event EventHandler<WorkspaceLayoutCompletedEventArgs>? WorkspaceLayoutCompleted;

	public WorkspaceSector(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;
	}

	// TODO: Add to StoreTests
	public override void Initialize() { }

	public override void DispatchEvents()
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

	private void DoLayout(Workspace workspace)
	{
		Logger.Debug($"Layout {workspace}");

		// Get the monitor for this workspace
		if (!_ctx.Store.Pick(Pickers.PickMonitorByWorkspace(workspace.Id)).TryGet(out IMonitor monitor))
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

		// triggers.WorkspaceLayoutCompleted(new WorkspaceEventArgs() { Workspace = workspace });
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

		foreach (IWindowState loc in workspace.ActiveLayoutEngine.DoLayout(monitor.WorkingArea, monitor))
		{
			HWND hwnd = loc.Window.Handle;
			IRectangle<int> rect = loc.Rectangle;

			windowPositions = windowPositions.SetItem(hwnd, new WindowPosition(loc.WindowSize, rect));
			handle.DeferWindowPos(new SetWindowPosState(hwnd, loc.WindowSize, rect));

			Logger.Debug($"Window {loc.Window} has rectangle {loc.Rectangle}");
		}

		Workspaces = Workspaces.SetItem(workspace.Id, workspace with { WindowPositions = windowPositions });
	}
}
