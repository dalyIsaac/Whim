using Serilog.Core;

namespace Whim.Core;

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
	public FileSinkConfig FileSink { get; }

	/// <summary>
	/// The configuration for the debug sink (what Visual Studio connects to).
	/// </summary>
	public SinkConfig? DebugSink { get; }

	/// <summary>
	/// Initializes the default <see cref="LoggerConfig"/>.<br/>
	///
	/// The default log file name is <c>whim.log</c>.
	/// </summary>
	public LoggerConfig() : this("whim.log") { }

	/// <summary>
	/// Initializes the <see cref="LoggerConfig"/> with a custom log file name.
	/// </summary>
	/// <param name="logFileName"></param>
	/// <param name="baseMinLogLevel">Sets <see cref="LoggerConfig.BaseMinLogLevel"/></param>
	public LoggerConfig(string logFileName, LogLevel baseMinLogLevel = LogLevel.Debug)
	{
		FileSink = new FileSinkConfig(logFileName, LogLevel.Debug);
		DebugSink = new SinkConfig(LogLevel.Debug);
		BaseMinLogLevel = baseMinLogLevel;
	}

	/// <summary>
	/// Initializes the <see cref="LoggerConfig"/> with custom file sink and optional debug sink
	/// configs.
	/// </summary>
	/// <param name="fileSinkConfig"><see cref="FileSinkConfig"/></param>
	/// <param name="debugSink"><see cref="DebugSink"/></param>
	public LoggerConfig(FileSinkConfig fileSinkConfig, SinkConfig? debugSink = null)
	{
		FileSink = fileSinkConfig;
		DebugSink = debugSink;
	}
}
