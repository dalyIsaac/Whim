using Serilog.Core;

namespace Whim;

/// <summary>
/// Default configuration options for a logging sink.
/// </summary>
public class SinkConfig
{
	/// <summary>
	/// What the <see cref="Logger"/> looks at.
	/// </summary>
	internal LoggingLevelSwitch MinLogLevelSwitch { get; } = new LoggingLevelSwitch();

	/// <summary>
	/// The sink's minimum log level.
	/// </summary>
	public LogLevel MinLogLevel
	{
		get => LogLevelExtensions.ToSink(MinLogLevelSwitch.MinimumLevel);
		set => MinLogLevelSwitch.MinimumLevel = value.ToSerilog();
	}

	/// <summary>
	/// Initializes the sink config with the provided minimum log level.
	/// </summary>
	/// <param name="minLogLevel"></param>
	public SinkConfig(LogLevel minLogLevel)
	{
		MinLogLevel = minLogLevel;
	}
}
