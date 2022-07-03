namespace Whim;

/// <summary>
/// The file sink's rolling interval for the file logs.
/// </summary>
public enum FileSinkConfigRollingInterval
{
	/// <summary>
	/// Never rolls over the file log.
	/// </summary>
	Infinite,

	/// <summary>
	/// Rolls over the file log each month.
	/// </summary>
	Month,

	/// <summary>
	/// Rolls over the file log each day.
	/// </summary>
	Day
}
