using System;

namespace Whim;

public class WorkspaceRenamedEventArgs : EventArgs
{
	public string OldName { get; }
	public string NewName { get; }
	public IWorkspace Workspace { get; }

	public WorkspaceRenamedEventArgs(IWorkspace workspace, string oldName, string newName)
	{
		Workspace = workspace;
		OldName = oldName;
		NewName = newName;
	}
}
