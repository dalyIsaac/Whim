using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

/// <summary>
/// Represents the change in monitors of Windows and Whim.
/// </summary>
public class MonitorsChangedEventArgs : EventArgs
{
	/// <summary>
	/// The monitors that were not removed or added.
	/// </summary>
	public required IEnumerable<IMonitor> UnchangedMonitors { get; init; }

	/// <summary>
	/// The monitors that were removed.
	/// </summary>
	public required IEnumerable<IMonitor> RemovedMonitors { get; init; }

	/// <summary>
	/// The monitors that were added.
	/// </summary>
	public required IEnumerable<IMonitor> AddedMonitors { get; init; }

	/// <summary>
	/// The previous monitors. This is derived from <see cref="UnchangedMonitors"/> and <see cref="RemovedMonitors"/>.
	/// </summary>
	public IEnumerable<IMonitor> PreviousMonitors => Concat(UnchangedMonitors, RemovedMonitors);

	/// <summary>
	/// The new monitors. This is derived from <see cref="UnchangedMonitors"/> and <see cref="AddedMonitors"/>.
	/// </summary>
	public IEnumerable<IMonitor> CurrentMonitors => Concat(UnchangedMonitors, AddedMonitors);

	private static IEnumerable<IMonitor> Concat(IEnumerable<IMonitor> first, IEnumerable<IMonitor> second)
	{
		List<IMonitor> result = new(first.Count() + second.Count());
		result.AddRange(first);
		result.AddRange(second);
		return result;
	}
}
