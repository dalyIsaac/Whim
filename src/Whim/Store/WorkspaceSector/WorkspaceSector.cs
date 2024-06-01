using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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
				_internalCtx.DeferWorkspacePosManager.DoLayout(this, workspace);
			}
		}
		WorkspacesToLayout.Clear();

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

	// TODO: Are these necessary?
	public void TriggerWorkspaceLayoutStarted(Workspace workspace) =>
		WorkspaceLayoutStarted?.Invoke(this, new WorkspaceLayoutStartedEventArgs { Workspace = workspace });

	public void TriggerWorkspaceLayoutCompleted(Workspace workspace) =>
		WorkspaceLayoutCompleted?.Invoke(this, new WorkspaceLayoutCompletedEventArgs { Workspace = workspace });
}
