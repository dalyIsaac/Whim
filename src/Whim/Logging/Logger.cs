using Serilog;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Whim;

/// <summary>
/// Logger used throughout Whim. It is accessed according to the singleton pattern.
/// </summary>
public class Logger : IDisposable
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

	private bool _disposedValue;

	/// <summary>
	/// The config for the logger.
	/// NOTE: Changes to this will only take effect if set prior to <see cref="Initialize"/>.
	/// </summary>
	public LoggerConfig Config { get; set; }

	/// <summary>
	/// Initialize the <see cref="Logger"/> with the <see cref="LoggerConfig"/>.
	/// </summary>
	/// <param name="config"></param>
	public Logger(LoggerConfig? config = null)
	{
		Config = config ?? new LoggerConfig();
	}

	/// <summary>
	/// Initializes the <see cref="Logger"/> with the file and debug sink.
	/// </summary>
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

	/// <summary>
	/// Adds caller information to the message.
	/// </summary>
	/// <param name="message">The message to add caller information to.</param>
	/// <param name="memberName">The name of the calling member.</param>
	/// <param name="sourceFilePath">The path to the source file.</param>
	/// <param name="sourceLineNumber">The line number in the source file.</param>
	/// <returns>The message with caller information added.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string AddCaller(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
		string fileLocation = $"{fileName}:{sourceLineNumber}";

		string methodName = $"[{memberName}]";

		return $"{fileLocation,-30} {methodName,-30} {message}";
	}

	/// <summary>
	/// Write a log event with the <see cref="LogLevel.Verbose"/> level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="memberName">The caller's member name.</param>
	/// <param name="sourceFilePath">The caller's source file path.</param>
	/// <param name="sourceLineNumber">The caller's source line number.</param>
	public static void Verbose(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Verbose(AddCaller(message, memberName, sourceFilePath, sourceLineNumber));
	}

	/// <summary>
	/// Write a log event with the <see cref="LogLevel.Debug"/> level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="memberName">The caller's member name.</param>
	/// <param name="sourceFilePath">The caller's source file path.</param>
	/// <param name="sourceLineNumber">The caller's source line number.</param>
	public static void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Debug(AddCaller(message, memberName, sourceFilePath, sourceLineNumber));
	}

	/// <summary>
	/// Write a log event with the <see cref="LogLevel.Information"/> level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="memberName">The caller's member name.</param>
	/// <param name="sourceFilePath">The caller's source file path.</param>
	/// <param name="sourceLineNumber">The caller's source line number.</param>
	public static void Information(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Information(AddCaller(message, memberName, sourceFilePath, sourceLineNumber));
	}

	/// <summary>
	/// Write a log event with the <see cref="LogLevel.Warning"/> level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="memberName">The caller's member name.</param>
	/// <param name="sourceFilePath">The caller's source file path.</param>
	/// <param name="sourceLineNumber">The caller's source line number.</param>
	public static void Warning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Warning(AddCaller(message, memberName, sourceFilePath, sourceLineNumber));
	}

	/// <summary>
	/// Write a log event with the <see cref="LogLevel.Error"/> level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="memberName">The caller's member name.</param>
	/// <param name="sourceFilePath">The caller's source file path.</param>
	/// <param name="sourceLineNumber">The caller's source line number.</param>
	public static void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Error(AddCaller(message, memberName, sourceFilePath, sourceLineNumber));
	}

	/// <summary>
	/// Write a log event with the <see cref="LogLevel.Fatal"/> level.
	/// </summary>
	/// <param name="message">The message to log.</param>
	/// <param name="memberName">The caller's member name.</param>
	/// <param name="sourceFilePath">The caller's source file path.</param>
	/// <param name="sourceLineNumber">The caller's source line number.</param>
	public static void Fatal(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		instance?._logger?.Fatal(AddCaller(message, memberName, sourceFilePath, sourceLineNumber));
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				if (_logger is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
