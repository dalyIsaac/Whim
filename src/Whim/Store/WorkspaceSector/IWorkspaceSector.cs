using System;
using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// The sector containing workspaces.
/// </summary>
public interface IWorkspaceSector
{
	/// <summary>
	/// The order of the workspaces in the sector.
	/// </summary>
	ImmutableArray<WorkspaceId> WorkspaceOrder { get; }

	/// <summary>
	/// All the workspaces currently tracked by Whim.
	/// </summary>
	ImmutableDictionary<WorkspaceId, ImmutableWorkspace> Workspaces { get; }

	Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; }

	ImmutableList<CreateProxyLayoutEngine> ProxyLayoutEngines { get; }

	/// <summary>
	/// The ID of the workspace which is currently active.
	/// </summary>
	WorkspaceId ActiveWorkspaceId { get; }
}