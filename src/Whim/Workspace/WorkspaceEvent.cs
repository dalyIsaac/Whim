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
	public required IWorkspace Workspace { get; init; }
}
