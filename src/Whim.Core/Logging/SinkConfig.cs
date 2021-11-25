using Serilog.Events;

namespace Whim.Core.Logging;

/// <summary>
/// Default configuration options for a logging sink.
/// </summary>
public class SinkConfig
{
	/// <summary>
	/// The sink's minimum log level.
	/// </summary>
	public SinkLogLevel MinLogLevel { get; }

	/// <summary>
	/// Initializes the sink config, with the default log level of
	/// <see cref="SinkLogLevel.Warning"/>.
	/// </summary>
	public SinkConfig() : this(SinkLogLevel.Warning) { }

	/// <summary>
	/// Initializes the sink config with the provided minimum log level.
	/// </summary>
	/// <param name="minLogLevel"></param>
	public SinkConfig(SinkLogLevel minLogLevel)
	{
		MinLogLevel = minLogLevel;
	}

	/// <summary>
	/// Gets the associated Serilog <see cref="LogEventLevel"/> for the sink config's
	/// <see cref="MinLogLevel"/>.
	/// </summary>
	/// <returns></returns>
	internal LogEventLevel GetLogLevel()
	{
		return MinLogLevel switch
		{
			SinkLogLevel.Verbose => LogEventLevel.Verbose,
			SinkLogLevel.Debug => LogEventLevel.Debug,
			SinkLogLevel.Information => LogEventLevel.Information,
			SinkLogLevel.Warning => LogEventLevel.Warning,
			SinkLogLevel.Error => LogEventLevel.Error,
			_ => LogEventLevel.Fatal,
		};
	}
}
