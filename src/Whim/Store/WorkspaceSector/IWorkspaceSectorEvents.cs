namespace Whim;

/// <summary>
/// The events raised by the <see cref="IWorkspaceSector"/>.
/// </summary>
public interface IWorkspaceSectorEvents
{
	/// <summary>
	/// Event raised when a workspace is added.
	/// </summary>
	event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;

	/// <summary>
	/// Event raised when a workspace is removed.
	/// </summary>
	event EventHandler<WorkspaceRemovedEventArgs>? WorkspaceRemoved;

	/// <summary>
	/// Event raised when a workspace is renamed.
	/// </summary>
	event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	/// <summary>
	/// Event raised when a workspace's active layout engine has changed.
	/// </summary>
	event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	/// <summary>
	/// Event raised when a workspace has started its layout.
	/// </summary>
	event EventHandler<WorkspaceLayoutStartedEventArgs>? WorkspaceLayoutStarted;

	/// <summary>
	/// Event raised when a workspace has completed its layout.
	/// </summary>
	event EventHandler<WorkspaceLayoutCompletedEventArgs>? WorkspaceLayoutCompleted;
}
