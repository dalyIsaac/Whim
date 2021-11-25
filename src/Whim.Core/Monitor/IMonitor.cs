namespace Whim.Core.Monitor;

/// <summary>
/// Represents a single display device.
/// </summary>
public interface IMonitor : ILocation
{
	/// <summary>
	/// The name of the monitor.
	/// </summary>
	public string Name { get; }
}
