using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// The sector containing monitors.
/// </summary>
public interface IMonitorSector
{
	/// <summary>
	/// All the monitors currently tracked by Whim.
	/// </summary>
	public ImmutableArray<IMonitor> Monitors { get; }

	/// <summary>
	/// The index of the monitor which is currently active, in <see cref="Monitors"/>.
	/// </summary>
	internal int ActiveMonitorIndex { get; }

	/// <summary>
	/// The index of the primary monitor, in <see cref="Monitors"/>.
	/// </summary>
	internal int PrimaryMonitorIndex { get; }

	/// <summary>
	/// The index of the last monitor which received an event sent by Windows which Whim did not ignore.
	/// </summary>
	internal int LastWhimActiveMonitorIndex { get; }
}
