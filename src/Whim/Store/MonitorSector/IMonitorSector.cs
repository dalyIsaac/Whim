namespace Whim;

/// <summary>
/// The sector containing monitors.
/// </summary>
public interface IMonitorSector
{
	/// <summary>
	/// All the monitors currently tracked by Whim.
	/// </summary>
	ImmutableArray<IMonitor> Monitors { get; }

	/// <summary>
	/// The handle of the monitor which is currently active, in <see cref="Monitors"/>.
	/// </summary>
	HMONITOR ActiveMonitorHandle { get; }

	/// <summary>
	/// The handle of the primary monitor, in <see cref="Monitors"/>.
	/// </summary>
	HMONITOR PrimaryMonitorHandle { get; }

	/// <summary>
	/// The handle of the last monitor which received an event sent by Windows which Whim did not ignore.
	/// </summary>
	HMONITOR LastWhimActiveMonitorHandle { get; }
}
