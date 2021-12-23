using System;

namespace Whim;

public class WorkspaceMonitorChangedEventArgs : EventArgs
{
	public IMonitor Monitor { get; }
	public IWorkspace? OldWorkspace { get; }
	public IWorkspace NewWorkspace { get; }

	public WorkspaceMonitorChangedEventArgs(IMonitor monitor, IWorkspace? oldWorkspace, IWorkspace newWorkspace)
	{
		Monitor = monitor;
		OldWorkspace = oldWorkspace;
		NewWorkspace = newWorkspace;
	}
}
