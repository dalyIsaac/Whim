using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

/// <summary>
/// Represents the change in monitors of Windows and Whim. This may include monitors were added
/// or removed, or monitors that were unchanged but had some properties changed, like their
/// position, resolution, work area, or scaling factor.
/// </summary>
public class MonitorsChangedEventArgs : EventArgs
{
	private IEnumerable<IMonitor>? _currentMonitors;

	/// <summary>
	/// The monitors that were not removed or added. These monitors may have had some properties
	/// changed, like their position, resolution, work area, or scaling factor.
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
	public IEnumerable<IMonitor> CurrentMonitors
	{
		get
		{
			_currentMonitors ??= Concat(UnchangedMonitors, AddedMonitors);
			return _currentMonitors;
		}
	}

	private static IEnumerable<IMonitor> Concat(IEnumerable<IMonitor> first, IEnumerable<IMonitor> second)
	{
		List<IMonitor> result = new(first.Count() + second.Count());
		result.AddRange(first);
		result.AddRange(second);
		return result;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"Unchanged: {UnchangedMonitors.Count()}, Removed: {RemovedMonitors.Count()}, Added: {AddedMonitors.Count()}";
	}
}
