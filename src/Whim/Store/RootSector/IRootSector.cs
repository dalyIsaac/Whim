namespace Whim;

/// <summary>
/// The root sector of the state. This is read-only.
/// </summary>
public interface IRootSector
{
	/// <inheritdoc cref="IMonitorSector"/>
	public IMonitorSector MonitorSector { get; }
}
