using System.IO;
using System.Runtime.CompilerServices;
using Serilog;

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
	private readonly ILogger _logger;

	/// <summary>
	/// Serilog <see cref="LoggerConfiguration"/> instance.
	/// </summary>
	private readonly LoggerConfiguration _loggerConfiguration;

	private Logger(LoggerConfig config)
	{
		FileSinkConfig? fileSink = config.FileSink;
		SinkConfig? debugSink = config.DebugSink;

		_loggerConfiguration = new LoggerConfiguration().MinimumLevel.ControlledBy(config.BaseMinLogLevelSwitch);

		if (fileSink != null)
		{
			string loggerFilePath = Path.Combine(FileHelper.GetUserWhimPath(), fileSink.FileName);
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

	/// <summary>
	/// Initialize the <see cref="Logger"/> with the <see cref="IConfigContext.WhimPath"/> and
	/// <see cref="LoggerConfig"/>.
	/// </summary>
	/// <param name="whimPath"><see cref="IConfigContext.WhimPath"/></param>
	/// <param name="config"><see cref="LoggerConfig"/></param>
	/// <returns>The <see cref="Logger"/> singleton instance.</returns>
	public static Logger Initialize(LoggerConfig config)
	{
		if (instance == null)
		{
			instance = new Logger(config);
		}
		return instance;
	}

	/// <summary>
	/// Initialize the <see cref="Logger"/> with the defaults.
	/// </summary>
	/// <returns>The <see cref="Logger"/> singleton instance.</returns>
	public static Logger Initialize() => Initialize(new LoggerConfig());

	public static void Verbose(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		if (instance == null)
		{
			throw new LoggerNotInitializedException();
		}

		instance._logger.Verbose(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		if (instance == null)
		{
			throw new LoggerNotInitializedException();
		}

		instance._logger.Debug(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Information(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		if (instance == null)
		{
			throw new LoggerNotInitializedException();
		}

		instance._logger.Information(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		if (instance == null)
		{
			throw new LoggerNotInitializedException();
		}

		instance._logger.Warning(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		if (instance == null)
		{
			throw new LoggerNotInitializedException();
		}

		instance._logger.Error(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}

	public static void Fatal(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		if (instance == null)
		{
			throw new LoggerNotInitializedException();
		}

		instance._logger.Fatal(message.AddCaller(memberName, sourceFilePath, sourceLineNumber));
	}
}
