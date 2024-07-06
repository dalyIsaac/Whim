namespace Whim;

/// <summary>
/// The standard exception for Whim errors.
/// </summary>
public class WhimException : Exception
{
	/// <inheritdoc/>
	public WhimException() { }

	/// <inheritdoc/>
	public WhimException(string message)
		: base(message)
	{
		Logger.Debug(message);
	}

	/// <inheritdoc/>
	public WhimException(string message, Exception innerException)
		: base(message, innerException)
	{
		Logger.Debug(message);
	}

	/// <summary>
	/// Log the given <paramref name="message"/> with the given <paramref name="logLevel"/>.
	/// </summary>
	/// <param name="message">
	/// The message to log.
	/// </param>
	/// <param name="logLevel">
	/// The log level to log the message at.
	/// </param>
	public WhimException(string message, LogLevel logLevel)
	{
		switch (logLevel)
		{
			case LogLevel.Verbose:
				Logger.Verbose(message);
				break;
			case LogLevel.Debug:
				Logger.Debug(message);
				break;
			case LogLevel.Information:
				Logger.Information(message);
				break;
			case LogLevel.Warning:
				Logger.Warning(message);
				break;
			case LogLevel.Error:
			default:
				Logger.Error(message);
				break;
			case LogLevel.Fatal:
				Logger.Fatal(message);
				break;
		}
	}
}
