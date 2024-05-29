using System;

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
		Logger.Error(message);
	}

	/// <inheritdoc/>
	public WhimException(string message, Exception innerException)
		: base(message, innerException)
	{
		Logger.Error(message);
	}
}
