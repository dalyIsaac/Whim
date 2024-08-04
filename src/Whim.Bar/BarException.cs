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
}
