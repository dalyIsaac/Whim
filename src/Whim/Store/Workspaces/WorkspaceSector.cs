using System;
using System.Collections.Immutable;

namespace Whim;

internal class WorkspaceSector : SectorBase, IWorkspaceSector, IWorkspaceSectorEvents
{
	public ImmutableList<ImmutableWorkspace> Workspaces { get; set; } = ImmutableList<ImmutableWorkspace>.Empty;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; } =
		() => new CreateLeafLayoutEngine[] { (id) => new ColumnLayoutEngine(id) };

	public ImmutableList<CreateProxyLayoutEngine> ProxyLayoutEngines { get; set; } =
		ImmutableList<CreateProxyLayoutEngine>.Empty;

	public event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;

	// TODO: Add to StoreTests
	public override void Initialize() { }

	public override void DispatchEvents()
	{
		foreach (EventArgs eventArgs in _events)
		{
			switch (eventArgs)
			{
				case WorkspaceAddedEventArgs args:
					WorkspaceAdded?.Invoke(this, args);
					break;
				default:
					break;
			}
		}

		_events.Clear();
	}
}
