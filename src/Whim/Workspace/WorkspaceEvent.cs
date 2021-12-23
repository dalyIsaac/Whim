using System;

namespace Whim;

/// <summary>
/// Event arguments for when an argument is added or removed from the workspace
/// manager.
/// </summary>
public class WorkspaceEventArgs : EventArgs
{
	public IWorkspace Workspace { get; }

	public WorkspaceEventArgs(IWorkspace workspace)
	{
		Workspace = workspace;
	}
}
