using Serilog;

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
	Day,
}

internal static class FileSinkConfigRollingIntervalExtensions
{
	/// <summary>
	/// Gets the associated Serilog <see cref="RollingInterval"/> for the <see cref="FileSinkConfigRollingInterval"/>.
	/// </summary>
	/// <param name="interval"></param>
	/// <returns></returns>
	internal static RollingInterval ToSerilog(this FileSinkConfigRollingInterval interval) =>
		interval switch
		{
			FileSinkConfigRollingInterval.Infinite => RollingInterval.Infinite,
			FileSinkConfigRollingInterval.Month => RollingInterval.Month,
			_ => RollingInterval.Day,
		};
}
