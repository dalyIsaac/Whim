using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

public class MonitorsChangedEventArgs : EventArgs
{
	/// <summary>
	/// The monitors that were not removed or added.
	/// </summary>
	public IEnumerable<IMonitor> UnchangedMonitors { get; private set; }

	/// <summary>
	/// The monitors that were removed.
	/// </summary>
	public IEnumerable<IMonitor> RemovedMonitors { get; private set; }

	/// <summary>
	/// The monitors that were added.
	/// </summary>
	public IEnumerable<IMonitor> AddedMonitors { get; private set; }

	/// <summary>
	/// The previous monitors. This is derived from <see cref="UnchangedMonitors"/> and <see cref="RemovedMonitors"/>.
	/// </summary>
	public IEnumerable<IMonitor> PreviousMonitors { get => Concat(UnchangedMonitors, RemovedMonitors); }

	/// <summary>
	/// The new monitors. This is derived from <see cref="UnchangedMonitors"/> and <see cref="AddedMonitors"/>.
	/// </summary>
	public IEnumerable<IMonitor> CurrentMonitors { get => Concat(UnchangedMonitors, AddedMonitors); }

	public MonitorsChangedEventArgs(IEnumerable<IMonitor> unchangedMonitors, IEnumerable<IMonitor> removedMonitors, IEnumerable<IMonitor> addedMonitors)
	{
		UnchangedMonitors = unchangedMonitors;
		RemovedMonitors = removedMonitors;
		AddedMonitors = addedMonitors;
	}

	private static IEnumerable<IMonitor> Concat(IEnumerable<IMonitor> first, IEnumerable<IMonitor> second)
	{
		List<IMonitor> result = new(first.Count() + second.Count());
		result.AddRange(first);
		result.AddRange(second);
		return result;
	}
}
