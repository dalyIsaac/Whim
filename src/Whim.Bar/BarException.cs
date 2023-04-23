using System;

namespace Whim.Bar;

/// <summary>
/// Exception thrown by Whim.Bar.
/// </summary>
[Serializable]
public class BarException : System.Exception
{
	/// <summary>
	/// Constructs a new BarException.
	/// </summary>
	public BarException() { }

	/// <summary>
	/// Constructs a new BarException.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public BarException(string message)
		: base(message) { }

	/// <summary>
	/// Constructs a new BarException.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception.</param>
	public BarException(string message, System.Exception inner)
		: base(message, inner) { }

	/// <summary>
	/// Constructs a new BarException.
	/// </summary>
	/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
	/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
	protected BarException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context
	)
		: base(info, context) { }
}
