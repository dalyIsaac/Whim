namespace Whim;

/// <summary>
/// The events raised by the <see cref="IMonitorSector"/>.
/// </summary>
public interface IMonitorSectorEvents
{
	/// <summary>
	/// Event raised when the monitors handled by Whim are changed.
	/// </summary>
	event EventHandler<MonitorsChangedEventArgs>? MonitorsChanged;
}
