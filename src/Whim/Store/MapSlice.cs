using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// The slice containing window-workspace and workspace-monitor mappings.
/// </summary>
public class MapSlice
{
	/// <summary>
	/// The mappings of windows to workspaces.
	/// </summary>
	public ImmutableDictionary<IWindow, IWorkspace> WindowWorkspaceMap { get; internal set; } =
		ImmutableDictionary<IWindow, IWorkspace>.Empty;

	/// <summary>
	/// The mappings of monitors to workspaces.
	/// </summary>
	public ImmutableDictionary<IMonitor, IWorkspace> MonitorWorkspaceMap { get; internal set; } =
		ImmutableDictionary<IMonitor, IWorkspace>.Empty;
}
