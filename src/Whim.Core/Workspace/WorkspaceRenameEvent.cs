using System;

namespace Whim.Core;

public class WorkspaceRenameEventArgs : EventArgs
{
	public string OldName { get; }
	public string NewName { get; }
	public IWorkspace Workspace { get; }

	public WorkspaceRenameEventArgs(IWorkspace workspace, string oldName, string newName)
	{
		Workspace = workspace;
		OldName = oldName;
		NewName = newName;
	}
}
