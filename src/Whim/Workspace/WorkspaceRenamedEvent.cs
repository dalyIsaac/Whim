using System;

namespace Whim;

/// <summary>
/// Describes the old and new names of a workspace.
/// </summary>
public class WorkspaceRenamedEventArgs : EventArgs
{
	/// <summary>
	/// The old name of the workspace.
	/// </summary>
	public required string OldName { get; init; }

	/// <summary>
	/// The new name of the workspace.
	/// </summary>
	public required string NewName { get; init; }

	/// <summary>
	/// The workspace that was renamed.
	/// </summary>
	public required IWorkspace Workspace { get; init; }
}
