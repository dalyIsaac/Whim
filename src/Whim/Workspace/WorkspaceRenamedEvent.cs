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
	public string OldName { get; }

	/// <summary>
	/// The new name of the workspace.
	/// </summary>
	public string NewName { get; }

	/// <summary>
	/// The workspace that was renamed.
	/// </summary>
	public IWorkspace Workspace { get; }

	/// <summary>
	/// Creates a new instance of <see cref="WorkspaceRenamedEventArgs"/>.
	/// </summary>
	/// <param name="workspace">The workspace that was renamed.</param>
	/// <param name="oldName">The old name of the workspace.</param>
	/// <param name="newName">The new name of the workspace.</param>
	public WorkspaceRenamedEventArgs(IWorkspace workspace, string oldName, string newName)
	{
		Workspace = workspace;
		OldName = oldName;
		NewName = newName;
	}
}
