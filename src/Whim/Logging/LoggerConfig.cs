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
	public FileSinkConfig? FileSink { get; }

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
	/// Initializes the <see cref="LoggerConfig"/> with the given base minimum log level.
	/// The file and debug sink will both have the same minimum log level.
	/// </summary>
	/// <param name="baseMinLogLevel">The base minimum log level.</param>
	public LoggerConfig(LogLevel baseMinLogLevel)
	{
		FileSink = new FileSinkConfig("whim.log", baseMinLogLevel);
		DebugSink = new SinkConfig(baseMinLogLevel);
		BaseMinLogLevel = baseMinLogLevel;
	}

	/// <summary>
	/// Initializes the <see cref="LoggerConfig"/> with a custom log file name.
	/// </summary>
	/// <param name="logFileName"></param>
	/// <param name="baseMinLogLevel">Sets <see cref="BaseMinLogLevel"/></param>
	public LoggerConfig(string logFileName, LogLevel baseMinLogLevel = LogLevel.Error)
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
	public LoggerConfig(FileSinkConfig? fileSinkConfig = null, SinkConfig? debugSink = null)
	{
		FileSink = fileSinkConfig;
		DebugSink = debugSink;
	}
}
