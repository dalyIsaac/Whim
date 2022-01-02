using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// The manager for <see cref="IMonitor"/>s.
/// </summary>
public interface IMonitorManager : IEnumerable<IMonitor>, ICommandable, IDisposable
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
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public IMonitor GetMonitorAtPoint(int x, int y);

	public event EventHandler<MonitorEventArgs>? MonitorsChanged;
}
