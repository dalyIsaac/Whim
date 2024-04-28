using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// The sector containing window-workspace and workspace-monitor mappings.
/// </summary>
public interface IMapSector
{
	/// <summary>
	/// The mappings of windows to workspaces.
	/// </summary>
	ImmutableDictionary<IWindow, IWorkspace> WindowWorkspaceMap { get; }

	/// <summary>
	/// The mappings of monitors to workspaces.
	/// </summary>
	ImmutableDictionary<IMonitor, IWorkspace> MonitorWorkspaceMap { get; }
}
