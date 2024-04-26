using System;

namespace Whim;

public interface IWorkspaceSectorEvents
{
	event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;
	event EventHandler<WorkspaceRemovedEventArgs>? WorkspaceRemoved;

	event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;
}
