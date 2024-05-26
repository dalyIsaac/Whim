using System;

namespace Whim;

public interface IWorkspaceSectorEvents
{
	event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;
	event EventHandler<WorkspaceRemovedEventArgs>? WorkspaceRemoved;
	event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;
	event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;
	event EventHandler<WorkspaceLayoutStartedEventArgs>? WorkspaceLayoutStarted;
	event EventHandler<WorkspaceLayoutCompletedEventArgs>? WorkspaceLayoutCompleted;
}
