using System.Collections.Immutable;

namespace Whim;

public class MapsSlice
{
	public ImmutableDictionary<IWindow, IWorkspace> WindowWorkspaceMap { get; internal set; } =
		ImmutableDictionary<IWindow, IWorkspace>.Empty;

	public ImmutableDictionary<IMonitor, IWorkspace> MonitorWorkspaceMap { get; internal set; } =
		ImmutableDictionary<IMonitor, IWorkspace>.Empty;
}
