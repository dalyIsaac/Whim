using System;
using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// The sector containing workspaces.
/// </summary>
public interface IWorkspaceSector
{
	/// <summary>
	/// All the workspaces currently tracked by Whim.
	/// </summary>
	ImmutableList<ImmutableWorkspace> Workspaces { get; }

	Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; }

	ImmutableList<CreateProxyLayoutEngine> ProxyLayoutEngines { get; }

	/// <summary>
	/// The index of the workspace which is currently active, in <see cref="Workspaces"/>.
	/// </summary>
	int ActiveWorkspaceIndex { get; }
}
