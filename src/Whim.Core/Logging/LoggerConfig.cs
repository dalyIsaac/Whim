namespace Whim.Core.Logging;

/// <summary>
/// The user-configurable settings for <see cref="Logger"/>.
/// </summary>
public class LoggerConfig
{
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
	public LoggerConfig(string logFileName)
	{
		FileSink = new FileSinkConfig(logFileName, SinkLogLevel.Verbose);
		DebugSink = new SinkConfig(SinkLogLevel.Verbose);
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
