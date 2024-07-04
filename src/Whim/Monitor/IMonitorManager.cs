namespace Whim;

/// <summary>
/// The manager for <see cref="IMonitor"/>s.
/// </summary>
public interface IMonitorManager : IEnumerable<IMonitor>, IDisposable
{
	/// <summary>
	/// The number of <see cref="IMonitor"/>s contained by <see cref="IMonitorManager"/>.
	/// </summary>
	int Length { get; }

	/// <summary>
	/// The currently active <see cref="IMonitor"/>.
	/// </summary>
	IMonitor ActiveMonitor { get; }

	/// <summary>
	/// The primary <see cref="IMonitor"/>.
	/// </summary>
	IMonitor PrimaryMonitor { get; }

	/// <summary>
	/// Initialize the windows event hooks.
	/// </summary>
	void Initialize();

	/// <summary>
	/// Returns the <see cref="IMonitor"/> at the given <i>x</i> and <i>y</i> coordinates.
	/// </summary>
	/// <param name="point">Point defined in terms of the real monitor coordinates.</param>
	/// <returns></returns>
	IMonitor GetMonitorAtPoint(IPoint<int> point);

	/// <summary>
	/// Event raised when the monitors handled by Whim are changed.
	/// </summary>
	event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	/// <summary>
	/// Gets the monitor before the given monitor.
	/// </summary>
	/// <returns></returns>
	IMonitor GetPreviousMonitor(IMonitor monitor);

	/// <summary>
	/// Gets the monitor after the given monitor.
	/// </summary>
	/// <returns></returns>
	IMonitor GetNextMonitor(IMonitor monitor);
}
