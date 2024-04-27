using System;
using System.Collections.Immutable;

namespace Whim;

internal class WorkspaceSector : SectorBase, IWorkspaceSector
{
	public ImmutableList<IWorkspace> Workspaces { get; set; } = ImmutableList<IWorkspace>.Empty;

	public int ActiveWorkspaceIndex { get; set; } = -1;

	// TODO: Add to StoreTests
	public override void Initialize() { }

	public override void DispatchEvents()
	{
		foreach (EventArgs eventArgs in _events)
		{
			switch (eventArgs)
			{
				// TODO
				default:
					break;
			}
		}

		_events.Clear();
	}
}
