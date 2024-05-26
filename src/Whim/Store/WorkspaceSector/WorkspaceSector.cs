using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim;

internal class WorkspaceSector : SectorBase, IWorkspaceSector, IWorkspaceSectorEvents
{
	private readonly IContext _ctx;

	private readonly IInternalContext _internalCtx;

	/// <summary>
	/// The IDs of the workspaces that should be laid out.
	/// </summary>
	public HashSet<Guid> WorkspacesToLayout { get; set; } = new();

	public ImmutableArray<WorkspaceId> WorkspaceOrder { get; set; } = ImmutableArray<WorkspaceId>.Empty;

	public ImmutableDictionary<WorkspaceId, ImmutableWorkspace> Workspaces { get; set; } =
		ImmutableDictionary<WorkspaceId, ImmutableWorkspace>.Empty;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; } =
		() => new CreateLeafLayoutEngine[] { (id) => new ColumnLayoutEngine(id) };

	public ImmutableList<CreateProxyLayoutEngine> ProxyLayoutEngines { get; set; } =
		ImmutableList<CreateProxyLayoutEngine>.Empty;

	public int ActiveWorkspaceIndex { get; set; }

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
			if (Workspaces.TryGetValue(id, out ImmutableWorkspace workspace))
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

	public void TriggerWorkspaceLayoutStarted(ImmutableWorkspace workspace) =>
		WorkspaceLayoutStarted?.Invoke(this, new WorkspaceLayoutStartedEventArgs { Workspace = workspace });

	public void TriggerWorkspaceLayoutCompleted(ImmutableWorkspace workspace) =>
		WorkspaceLayoutCompleted?.Invoke(this, new WorkspaceLayoutCompletedEventArgs { Workspace = workspace });
}
