using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim;

internal class WorkspaceSector : SectorBase, IWorkspaceSector, IWorkspaceSectorEvents
{
	public HashSet<Guid> WorkspacesToLayout { get; set; } = new();

	public ImmutableList<ImmutableWorkspace> Workspaces { get; set; } = ImmutableList<ImmutableWorkspace>.Empty;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; } =
		() => new CreateLeafLayoutEngine[] { (id) => new ColumnLayoutEngine(id) };

	public ImmutableList<CreateProxyLayoutEngine> ProxyLayoutEngines { get; set; } =
		ImmutableList<CreateProxyLayoutEngine>.Empty;

	public int ActiveWorkspaceIndex { get; set; }

	public event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;

	public event EventHandler<WorkspaceRemovedEventArgs>? WorkspaceRemoved;

	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	// TODO: Add to StoreTests
	public override void Initialize() { }

	public override void DispatchEvents()
	{
		foreach (Guid workspaceId in WorkspacesToLayout)
		{
			// TODO: DoLayout
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
}
