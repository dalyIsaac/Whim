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

/// <summary>
/// Event arguments for when a workspace has been added.
/// </summary>
public class WorkspaceAddedEventArgs : WorkspaceEventArgs { }

/// <summary>
/// Event arguments for when a workspace has been removed.
/// </summary>
public class WorkspaceRemovedEventArgs : WorkspaceEventArgs { }

/// <summary>
/// Event arguments for when a workspace has been renamed.
/// </summary>
public class WorkspaceRenamedEventArgs : WorkspaceEventArgs
{
	/// <summary>
	/// The old name of the workspace.
	/// </summary>
	public required string PreviousName { get; init; }
}

/// <summary>
/// Event arguments for when a workspace has started performing a layout.
/// </summary>
public class WorkspaceLayoutStartedEventArgs : WorkspaceEventArgs { }

/// <summary>
/// Event arguments for when a workspace has completed performing a layout.
/// </summary>
public class WorkspaceLayoutCompletedEventArgs : WorkspaceEventArgs { }
