using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// The slice containing monitors.
/// </summary>
public class MonitorSlice
{
	/// <summary>
	/// All the monitors currently tracked by Whim.
	/// </summary>
	public ImmutableArray<IMonitor> Monitors { get; internal set; }

	/// <summary>
	/// The index of the monitor which is currently active, in <see cref="Monitors"/>.
	/// </summary>
	public int ActiveMonitorIndex { get; internal set; } = -1;

	/// <summary>
	/// The index of the primary monitor, in <see cref="Monitors"/>.
	/// </summary>
	public int PrimaryMonitorIndex { get; internal set; } = -1;

	/// <summary>
	/// The index of the last monitor which received an event sent by Windows which Whim did not ignore.
	/// </summary>
	public int LastWhimActiveMonitorIndex { get; internal set; } = -1;
}
