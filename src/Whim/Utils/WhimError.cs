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
	/// An optional inner error that caused this error.
	/// </summary>
	public WhimError? InnerError { get; }

	/// <summary>
	/// An optional exception that caused this error (for interop with existing exception-based code).
	/// </summary>
	public Exception? InnerException { get; }

	/// <summary>
	/// Creates a new WhimError with the specified message.
	/// </summary>
	/// <param name="message">The error message.</param>
	public WhimError(string message)
	{
		Message = message;
	}

	/// <summary>
	/// Creates a new WhimError with the specified message and inner error.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="innerError">The inner error that caused this error.</param>
	public WhimError(string message, WhimError innerError)
	{
		Message = message;
		InnerError = innerError;
	}

	/// <summary>
	/// Creates a new WhimError with the specified message and inner exception.
	/// </summary>
	/// <param name="message">The error message.</param>
	/// <param name="innerException">The inner exception that caused this error.</param>
	public WhimError(string message, Exception innerException)
	{
		Message = message;
		InnerException = innerException;
	}

	/// <summary>
	/// Returns a string representation of this error.
	/// </summary>
	public override string ToString()
	{
		string result = Message;
		if (InnerError != null)
		{
			result += $" -> {InnerError}";
		}
		if (InnerException != null)
		{
			result += $" -> {InnerException.Message}";
		}
		return result;
	}
}
