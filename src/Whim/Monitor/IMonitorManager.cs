using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// The manager for <see cref="IMonitor"/>s.
/// </summary>
public interface IMonitorManager : IEnumerable<IMonitor>, IDisposable
{
	/// <summary>
	/// The number of <see cref="IMonitor"/>s contained by <see cref="IMonitorManager"/>.
	/// </summary>
	public int Length { get; }

	/// <summary>
	/// The currently focused <see cref="IMonitor"/>.
	/// </summary>
	public IMonitor FocusedMonitor { get; }

	/// <summary>
	/// Initialize the windows event hooks.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Returns the <see cref="IMonitor"/> at the given <i>x</i> and <i>y</i> coordinates.
	/// </summary>
	/// <param name="point">Point defined in terms of the real monitor coordinates.</param>
	/// <returns></returns>
	public IMonitor GetMonitorAtPoint(IPoint<int> point);

	/// <summary>
	/// Event raised when the monitors handled by Whim are changed.
	/// </summary>
	public event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	/// <summary>
	/// Gets the monitor before the given monitor.
	/// </summary>
	/// <returns></returns>
	public IMonitor GetPreviousMonitor(IMonitor monitor);

	/// <summary>
	/// Gets the monitor after the given monitor.
	/// </summary>
	/// <returns></returns>
	public IMonitor GetNextMonitor(IMonitor monitor);
}
