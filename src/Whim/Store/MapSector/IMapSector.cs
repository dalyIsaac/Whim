using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// The sector containing window-workspace and workspace-monitor mappings.
/// </summary>
public interface IMapSector
{
	/// <summary>
	/// The mappings of windows to workspaces.
	/// </summary>
	ImmutableDictionary<HWND, WorkspaceId> WindowWorkspaceMap { get; }

	/// <summary>
	/// The mappings of monitors to workspaces.
	/// </summary>
	ImmutableDictionary<IMonitor, WorkspaceId> MonitorWorkspaceMap { get; }
}
