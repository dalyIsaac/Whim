using System.Collections.Immutable;

namespace Whim;

public class MonitorsSlice
{
	public ImmutableArray<IMonitor> Monitors { get; internal set; }
	public int ActiveMonitorIndex { get; internal set; } = -1;
	public int PrimaryMonitorIndex { get; internal set; } = -1;
	public int LastWhimActiveMonitorIndex { get; internal set; } = -1;
}
