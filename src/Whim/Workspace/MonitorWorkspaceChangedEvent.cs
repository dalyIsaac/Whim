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
	public required IMonitor Monitor { get; init; }

	/// <summary>
	/// The previously shown workspace.
	/// </summary>
	public IWorkspace? OldWorkspace { get; init; }

	/// <summary>
	/// The new workspace shown on the monitor.
	/// </summary>
	public required IWorkspace NewWorkspace { get; init; }
}
