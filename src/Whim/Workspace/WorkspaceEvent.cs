using System;

namespace Whim;

/// <summary>
/// Event arguments for when an argument is added or removed from the workspace
/// manager.
/// </summary>
public class WorkspaceEventArgs : EventArgs
{
	/// <summary>
	/// The workspace that was added or removed.
	/// </summary>
	public IWorkspace Workspace { get; }

	/// <summary>
	/// Creates a new workspace event args.
	/// </summary>
	public WorkspaceEventArgs(IWorkspace workspace)
	{
		Workspace = workspace;
	}
}
