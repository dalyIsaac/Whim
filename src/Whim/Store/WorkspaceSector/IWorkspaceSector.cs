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
	/// The index of the workspace which is currently active, in <see cref="Workspaces"/>.
	/// </summary>
	int ActiveWorkspaceIndex { get; }
}
