namespace Whim;

/// <summary>
/// Exception thrown when a window could not be initialized.
/// </summary>
[System.Serializable]
public class InitializeWindowException : System.Exception
{
	/// <summary>
	/// Constructs a new InitializeWindowException.
	/// </summary>
	public InitializeWindowException() { }

	/// <summary>
	/// Constructs a new InitializeWindowException.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public InitializeWindowException(string message) : base(message) { }

	/// <summary>
	/// Constructs a new InitializeWindowException.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception.</param>
	public InitializeWindowException(string message, System.Exception inner) : base(message, inner) { }

	/// <summary>
	/// Constructs a new InitializeWindowException.
	/// </summary>
	/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
	/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
	protected InitializeWindowException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
