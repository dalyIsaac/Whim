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

public class WorkspaceAddedEventArgs : WorkspaceEventArgs { }

public class WorkspaceRemovedEventArgs : WorkspaceEventArgs { }

public class WorkspaceRenamedEventArgs : WorkspaceEventArgs
{
	/// <summary>
	/// The old name of the workspace.
	/// </summary>
	public required string PreviousName { get; init; }
}

public class WorkspaceLayoutStartedEventArgs : WorkspaceEventArgs { }

public class WorkspaceLayoutCompletedEventArgs : WorkspaceEventArgs { }
