namespace Whim.Core.Logging;

/// <summary>
/// The file sink's rolling interval for the file logs.
/// </summary>
public enum FileSinkConfigRollingInterval
{
	/// <summary>
	/// Never rolls over the file log.
	/// </summary>
	Infinite,
	Month,
	Day
}
