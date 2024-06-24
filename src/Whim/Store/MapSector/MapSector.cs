using System;
using System.Collections.Immutable;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// The sector containing window-workspace and workspace-monitor mappings.
/// </summary>
internal class MapSector : SectorBase, IMapSector, IMapSectorEvents
{
	public ImmutableDictionary<HWND, WorkspaceId> WindowWorkspaceMap { get; set; } =
		ImmutableDictionary<HWND, WorkspaceId>.Empty;

	public ImmutableDictionary<HMONITOR, WorkspaceId> MonitorWorkspaceMap { get; set; } =
		ImmutableDictionary<HMONITOR, WorkspaceId>.Empty;

	public event EventHandler<RouteEventArgs>? WindowRouted;

	public event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;

	public override void Initialize() { }

	public override void DispatchEvents()
	{
		// Use index access to prevent the list from being modified during enumeration.
		for (int idx = 0; idx < _events.Count; idx++)
		{
			EventArgs eventArgs = _events[idx];
			switch (eventArgs)
			{
				case RouteEventArgs args:
					WindowRouted?.Invoke(this, args);
					break;

				case MonitorWorkspaceChangedEventArgs args:
					MonitorWorkspaceChanged?.Invoke(this, args);
					break;

				default:
					break;
			}
		}

		_events.Clear();
	}
}
