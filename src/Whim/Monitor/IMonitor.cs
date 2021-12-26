namespace Whim;

/// <summary>
/// Represents a single display device.
/// </summary>
public interface IMonitor : ILocation
{
	/// <summary>
	/// The name of the monitor.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// <see langword="true"/> if the monitor is the primary monitor.
	/// </summary>
	public bool IsPrimary { get; }
}
