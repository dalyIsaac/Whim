using System;

namespace Whim;

/// <summary>
/// Event arguments for when the workspace shown on a monitor changes.
/// </summary>
public class MonitorWorkspaceChangedEventArgs : EventArgs
{
	/// <summary>
	/// The monitor that has a workspace added or removed.
	/// </summary>
	public IMonitor Monitor { get; }

	/// <summary>
	/// The previously shown workspace.
	/// </summary>
	public IWorkspace? OldWorkspace { get; }

	/// <summary>
	/// The new workspace shown on the monitor.
	/// </summary>
	public IWorkspace NewWorkspace { get; }

	/// <summary>
	/// Creates a new monitor workspace changed event args.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="oldWorkspace"></param>
	/// <param name="newWorkspace"></param>
	public MonitorWorkspaceChangedEventArgs(IMonitor monitor, IWorkspace? oldWorkspace, IWorkspace newWorkspace)
	{
		Monitor = monitor;
		OldWorkspace = oldWorkspace;
		NewWorkspace = newWorkspace;
	}
}
