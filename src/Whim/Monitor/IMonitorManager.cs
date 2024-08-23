namespace Whim;

/// <summary>
/// The manager for <see cref="IMonitor"/>s.
/// </summary>
[Obsolete("Use transforms and pickers to interact with the store instead.")]
public interface IMonitorManager : IEnumerable<IMonitor>, IDisposable
{
	/// <summary>
	/// The number of <see cref="IMonitor"/>s contained by <see cref="IMonitorManager"/>.
	/// </summary>
	[Obsolete("Use the picker PickAllMonitors instead.")]
	int Length { get; }

	/// <summary>
	/// The currently active <see cref="IMonitor"/>.
	/// </summary>
	[Obsolete("Use the picker PickActiveMonitor instead.")]
	IMonitor ActiveMonitor { get; }

	/// <summary>
	/// The primary <see cref="IMonitor"/>.
	/// </summary>
	[Obsolete("Use the picker PickPrimaryMonitor instead.")]
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
	[Obsolete("Use the picker PickMonitorAtPoint instead.")]
	IMonitor GetMonitorAtPoint(IPoint<int> point);

	/// <summary>
	/// Event raised when the monitors handled by Whim are changed.
	/// </summary>
	[Obsolete("Use the IStore.IMonitorSectorEvents.MonitorsChanged event instead.")]
	event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;

	/// <summary>
	/// Gets the monitor before the given monitor.
	/// </summary>
	/// <returns></returns>
	[Obsolete("Use the picker PickAdjacentMonitor instead.")]
	IMonitor GetPreviousMonitor(IMonitor monitor);

	/// <summary>
	/// Gets the monitor after the given monitor.
	/// </summary>
	/// <returns></returns>
	[Obsolete("Use the picker PickAdjacentMonitor instead.")]
	IMonitor GetNextMonitor(IMonitor monitor);
}
