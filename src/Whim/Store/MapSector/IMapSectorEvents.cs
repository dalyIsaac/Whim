using System;

namespace Whim;

/// <summary>
/// Events relating to the mapping of <see cref="IWindow"/>s to <see cref="IWorkspace"/>s
/// and <see cref="IWorkspace"/>s to <see cref="IMonitor"/>s.
/// </summary>
public interface IMapSectorEvents
{
	/// <summary>
	/// Description of how an <see cref="IWindow"/> has been routed between workspaces.
	/// </summary>
	event EventHandler<RouteEventArgs>? WindowRouted;

	/// <summary>
	/// Event for when a monitor's workspace has changed.
	/// </summary>
	event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;
}
