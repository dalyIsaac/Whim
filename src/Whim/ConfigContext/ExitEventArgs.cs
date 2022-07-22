using System;

namespace Whim;

/// <summary>
/// Event arguments for the <see cref="IConfigContext.Exiting"/> event.
/// </summary>
public class ExitEventArgs : EventArgs
{
	/// <summary>
	/// The reason why Whim has been shut down.
	/// </summary>
	public ExitReason Reason { get; }

	/// <summary>
	/// A string describing the reason why Whim has been shut down.
	/// </summary>
	public string? Message { get; }

	/// <summary>
	/// Constructs a new ShutdownEventArgs.
	/// </summary>
	/// <param name="reason">The reason why Whim has been shut down.</param>
	/// <param name="message">A string describing the reason why Whim has been shut down.</param>
	public ExitEventArgs(ExitReason reason, string? message = null)
	{
		Reason = reason;
		Message = message;
	}
}
