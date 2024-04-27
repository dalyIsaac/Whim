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
	public ImmutableDictionary<IWindow, IWorkspace> WindowWorkspaceMap { get; }

	/// <summary>
	/// The mappings of monitors to workspaces.
	/// </summary>
	public ImmutableDictionary<IMonitor, IWorkspace> MonitorWorkspaceMap { get; }
}
