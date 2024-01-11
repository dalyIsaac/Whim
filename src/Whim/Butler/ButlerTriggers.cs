using System;

namespace Whim;

internal class ButlerTriggers
{
	public required Action<RouteEventArgs> WindowRouted { get; init; }

	public required Action<MonitorWorkspaceChangedEventArgs> MonitorWorkspaceChanged { get; init; }
}
