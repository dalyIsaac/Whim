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
	public ImmutableList<IWorkspace> Workspaces { get; }

	/// <summary>
	/// The index of the workspace which is currently active, in <see cref="Workspaces"/>.
	/// </summary>
	public int ActiveWorkspaceIndex { get; }
}
