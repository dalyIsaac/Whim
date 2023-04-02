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
	public required string PreviousName { get; init; }

	/// <summary>
	/// The new name of the workspace.
	/// </summary>
	public required string CurrentName { get; init; }

	/// <summary>
	/// The workspace that was renamed.
	/// </summary>
	public required IWorkspace Workspace { get; init; }
}
