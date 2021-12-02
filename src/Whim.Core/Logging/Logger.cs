using System;
using System.IO;
using Serilog;

namespace Whim.Core.Logging;

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
		FileSinkConfig fileSink = config.FileSink;
		SinkConfig? debugSink = config.DebugSink;

		string loggerFilePath = Path.Combine(FileHelper.GetUserWhimPath(), fileSink.FileName);

		_loggerConfiguration = new LoggerConfiguration()
			.MinimumLevel.ControlledBy(config.BaseMinLogLevelSwitch)
			.WriteTo.File(
				loggerFilePath,
				levelSwitch: fileSink.MinLogLevelSwitch
			);

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

	public static void Verbose(string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Verbose(message, args);
	}

	public static void Verbose(Exception exception, string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Verbose(exception, message, args);
	}


	public static void Debug(string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Debug(message, args);
	}

	public static void Debug(Exception exception, string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Debug(exception, message, args);
	}


	public static void Information(string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Information(message, args);
	}

	public static void Information(Exception exception, string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Information(exception, message, args);
	}


	public static void Warning(string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Warning(message, args);
	}

	public static void Warning(Exception exception, string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Warning(exception, message, args);
	}


	public static void Error(string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Error(message, args);
	}

	public static void Error(Exception exception, string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Error(exception, message, args);
	}


	public static void Fatal(string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Fatal(message, args);
	}

	public static void Fatal(Exception exception, string message, params object[] args)
	{
		if (instance == null)
		{
			throw new LoggerUninitializedException();
		}

		instance._logger.Fatal(exception, message, args);
	}
}
