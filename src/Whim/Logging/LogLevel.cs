using Serilog.Events;

namespace Whim;

/// <summary>
/// The log levels available to <see cref="Logger"/>.
/// </summary>
public enum LogLevel
{
	/// <summary>
	/// The <b>lowest</b> and noisiest log level.
	/// </summary>
	Verbose,

	/// <summary>
	/// Describes internal system events not observable from the outside. Useful for debugging.
	/// </summary>
	Debug,

	/// <summary>
	/// Events which correspond to its responsibilities and functions.
	/// <b>Generally observable actions the system can perform.</b>
	/// </summary>
	Information,

	/// <summary>
	/// When the system is performing outside of its expected parameters, is degraded or endangered.
	/// </summary>
	Warning,

	/// <summary>
	/// When something is unavailable or expectations broken.
	/// </summary>
	Error,

	/// <summary>
	/// Something which demands immediate attention. <b>The highest log level.</b>
	/// </summary>
	Fatal,
}

internal static class LogLevelExtensions
{
	/// <summary>
	/// Gets the associated Serilog <see cref="LogEventLevel"/> for the <see cref="LogLevel"/>.
	/// </summary>
	/// <param name="level"></param>
	/// <returns></returns>
	internal static LogEventLevel ToSerilog(this LogLevel level) => level switch
	{
		LogLevel.Verbose => LogEventLevel.Verbose,
		LogLevel.Debug => LogEventLevel.Debug,
		LogLevel.Information => LogEventLevel.Information,
		LogLevel.Warning => LogEventLevel.Warning,
		LogLevel.Error => LogEventLevel.Error,
		_ => LogEventLevel.Fatal,
	};

	/// <summary>
	/// Gets the associated <see cref="LogLevel"/> for the Serilog <see cref="LogEventLevel"/>.
	/// </summary>
	/// <param name="level"></param>
	/// <returns></returns>
	internal static LogLevel ToSink(this LogEventLevel level) => level switch
	{
		LogEventLevel.Verbose => LogLevel.Verbose,
		LogEventLevel.Debug => LogLevel.Debug,
		LogEventLevel.Information => LogLevel.Information,
		LogEventLevel.Warning => LogLevel.Warning,
		LogEventLevel.Error => LogLevel.Error,
		_ => LogLevel.Fatal,
	};
}
