using System;
using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// The sector containing window-workspace and workspace-monitor mappings.
/// </summary>
internal class MapSector : SectorBase, IMapSector
{
	public ImmutableDictionary<IWindow, IWorkspace> WindowWorkspaceMap { get; set; } =
		ImmutableDictionary<IWindow, IWorkspace>.Empty;

	public ImmutableDictionary<IMonitor, IWorkspace> MonitorWorkspaceMap { get; set; } =
		ImmutableDictionary<IMonitor, IWorkspace>.Empty;

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
