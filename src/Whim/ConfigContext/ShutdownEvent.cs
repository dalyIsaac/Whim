using System;

namespace Whim;

/// <summary>
/// Event arguments for the Shutdown event.
/// </summary>
public class ShutdownEventArgs : EventArgs
{
	/// <summary>
	/// The reason why Whim has been shut down.
	/// </summary>
	public ShutdownReason Reason { get; }

	/// <summary>
	/// A string describing the reason why Whim has been shut down.
	/// </summary>
	public string? Message { get; }

	/// <summary>
	/// Constructs a new ShutdownEventArgs.
	/// </summary>
	/// <param name="reason">The reason why Whim has been shut down.</param>
	/// <param name="message">A string describing the reason why Whim has been shut down.</param>
	public ShutdownEventArgs(ShutdownReason reason, string? message = null)
	{
		Reason = reason;
		Message = message;
	}
}
