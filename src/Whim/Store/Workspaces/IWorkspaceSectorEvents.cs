using System;

namespace Whim;

public interface IWorkspaceSectorEvents
{
	event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;
}
