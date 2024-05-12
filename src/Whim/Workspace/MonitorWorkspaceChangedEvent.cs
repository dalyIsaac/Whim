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
	public IWorkspace? PreviousWorkspace { get; init; }

	/// <summary>
	/// The new workspace shown on the monitor.
	/// </summary>
	public required IWorkspace CurrentWorkspace { get; init; }

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (obj is not MonitorWorkspaceChangedEventArgs other)
		{
			return false;
		}

		return other.Monitor.Handle == Monitor.Handle
			&& other.PreviousWorkspace == PreviousWorkspace
			&& other.CurrentWorkspace == CurrentWorkspace;
	}

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Monitor, PreviousWorkspace, CurrentWorkspace);
}
