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
	ImmutableDictionary<HMONITOR, WorkspaceId> MonitorWorkspaceMap { get; }

	/// <summary>
	/// The mappings of workspaces to the indices of the monitors they are allowed to be on.
	/// </summary>
	ImmutableDictionary<WorkspaceId, ImmutableArray<int>> StickyWorkspaceMonitorIndexMap { get; }

	/// <summary>
	/// The last monitor which the workspace was on.
	/// </summary>
	ImmutableDictionary<WorkspaceId, HMONITOR> LastMonitorWorkspaceMap { get; }
}
