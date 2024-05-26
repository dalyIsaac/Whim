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

// TODO: Make WorkspaceEventArgs the base when Workspace is no longer mutable
public abstract class ImmutableWorkspaceEventArgs : EventArgs
{
	public required ImmutableWorkspace Workspace { get; init; }
}

public class WorkspaceAddedEventArgs : ImmutableWorkspaceEventArgs { }

public class WorkspaceRemovedEventArgs : ImmutableWorkspaceEventArgs { }

public class WorkspaceRenamedEventArgs : ImmutableWorkspaceEventArgs
{
	/// <summary>
	/// The old name of the workspace.
	/// </summary>
	public required string PreviousName { get; init; }
}

public class WorkspaceLayoutStartedEventArgs : ImmutableWorkspaceEventArgs { }

public class WorkspaceLayoutCompletedEventArgs : ImmutableWorkspaceEventArgs { }
