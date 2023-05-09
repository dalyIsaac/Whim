using Serilog.Core;

namespace Whim;

/// <summary>
/// The user-configurable settings for <see cref="Logger"/>.
/// </summary>
public class LoggerConfig
{
	/// <summary>
	/// What the <see cref="Logger"/> looks at.
	/// </summary>
	internal LoggingLevelSwitch BaseMinLogLevelSwitch { get; } = new LoggingLevelSwitch();

	/// <summary>
	/// The base minimum log level for the logger itself.<br/>
	/// <b><see cref="SinkConfig.MinLogLevel"/> cannot override this</b>.
	/// </summary>
	public LogLevel BaseMinLogLevel
	{
		get => LogLevelExtensions.ToSink(BaseMinLogLevelSwitch.MinimumLevel);
		set => BaseMinLogLevelSwitch.MinimumLevel = value.ToSerilog();
	}

	/// <summary>
	/// The configuration for the file sink.
	/// </summary>
	public FileSinkConfig? FileSink { get; init; } =
		new FileSinkConfig()
		{
			FileName = "whim.log",
			MinLogLevel = LogLevel.Error,
			RollingInterval = FileSinkConfigRollingInterval.Day
		};

	/// <summary>
	/// The configuration for the debug sink (what Visual Studio connects to).
	/// </summary>
	public SinkConfig? DebugSink { get; } = new SinkConfig() { MinLogLevel = LogLevel.Error };
}
