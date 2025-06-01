namespace Whim;

/// <summary>
/// Represents an error that occurred in Whim operations.
/// This is not an Exception and avoids the performance overhead of exceptions.
/// It provides a functional approach to error handling instead of exception-based error handling.
/// </summary>
public class WhimError
{
	/// <summary>
	/// The error message describing what went wrong.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// An optional exception that caused this error (for interop with existing exception-based code).
	/// </summary>
	public Exception? InnerException { get; }

	/// <summary>
	/// Creates a new WhimError with the specified message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public WhimError(string message)
		: this(message, LogLevel.Debug)
	{
		Message = message;
	}

	/// <summary>
	/// Creates a new WhimError with the specified message and inner exception.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="innerException">The inner exception that caused this error.</param>
	public WhimError(string message, Exception innerException)
		: this(message)
	{
		InnerException = innerException;
	}

	/// <summary>
	/// Creates a new WhimError with the specified message and log level.
	/// </summary>
	/// <param name="message">
	/// The error message.
	/// </param>
	/// <param name="logLevel">
	/// The log level to use when logging this error.
	/// </param>
	public WhimError(string message, LogLevel logLevel)
	{
		Message = message;

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

	/// <summary>
	/// Returns a string representation of this error.
	/// </summary>
	public override string ToString()
	{
		string result = Message;
		if (InnerException != null)
		{
			result += $" -> {InnerException.Message}";
		}
		return result;
	}
}
