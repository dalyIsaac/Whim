using Serilog;
using System.IO;
using System.Runtime.CompilerServices;

namespace Whim;

/// <summary>
/// Logger used throughout Whim. It is accessed according to the singleton pattern.
/// </summary>
public class Logger
{
	/// <summary>
	/// Logger instance.
	/// </summary>
	private static Logger? instance;

	/// <summary>
	/// Serilog <see cref="ILogger"/> instance.
	/// </summary>
	private ILogger? _logger;

	/// <summary>
	/// Serilog <see cref="LoggerConfiguration"/> instance.
	/// </summary>
	private LoggerConfiguration? _loggerConfiguration;

	/// <summary>
	/// The config for the logger.
	/// NOTE: Changes to this will only take effect if set prior to <see cref="Initialize"/>.
	/// </summary>
	public LoggerConfig Config { get; set; }

	/// <summary>
	/// Initialize the <see cref="Logger"/> with the <see cref="IConfigContext.WhimPath"/> and
	/// <see cref="LoggerConfig"/>.
	/// </summary>
	/// <param name="whimPath"><see cref="IConfigContext.WhimPath"/></param>
	/// <param name="config"><see cref="LoggerConfig"/></param>
	public Logger(LoggerConfig? config = null)
	{
		Config = config ?? new LoggerConfig();
	}

	public void Initialize()
	{
		Logger.instance = this;
		FileSinkConfig? fileSink = Config.FileSink;
		SinkConfig? debugSink = Config.DebugSink;

		_loggerConfiguration = new LoggerConfiguration().MinimumLevel.ControlledBy(Config.BaseMinLogLevelSwitch);

		if (fileSink != null)
		{
			string loggerFilePath = Path.Combine(FileHelper.GetWhimDir(), fileSink.FileName);
			_loggerConfiguration.WriteTo.File(
								loggerFilePath,
								levelSwitch: fileSink.MinLogLevelSwitch
							);
		}

		if (debugSink != null)
		{
			_loggerConfiguration.WriteTo.Debug(levelSwitch: debugSink.MinLogLevelSwitch);
		}

		_logger = _loggerConfiguration.CreateLogger();
		_logger.Debug("Created logger!");
	}

	public static void Verbose(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Verbose(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Debug(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Information(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Information(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Warning(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Error(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Fatal(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Fatal(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}
}
