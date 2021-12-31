using System;

namespace Whim;

public class MonitorWorkspaceChangedEventArgs : EventArgs
{
	public IMonitor Monitor { get; }
	public IWorkspace? OldWorkspace { get; }
	public IWorkspace NewWorkspace { get; }

	public MonitorWorkspaceChangedEventArgs(IMonitor monitor, IWorkspace? oldWorkspace, IWorkspace newWorkspace)
	{
		Monitor = monitor;
		OldWorkspace = oldWorkspace;
		NewWorkspace = newWorkspace;
	}
}
