using System;

namespace Whim;

/// <summary>
/// Event arguments for the <see cref="IConfigContext.Quitting"/> event.
/// </summary>
public class QuitEventArgs : EventArgs
{
	/// <summary>
	/// The reason why Whim has been shut down.
	/// </summary>
	public QuitReason Reason { get; }

	/// <summary>
	/// A string describing the reason why Whim has been shut down.
	/// </summary>
	public string? Message { get; }

	/// <summary>
	/// Constructs a new ShutdownEventArgs.
	/// </summary>
	/// <param name="reason">The reason why Whim has been shut down.</param>
	/// <param name="message">A string describing the reason why Whim has been shut down.</param>
	public QuitEventArgs(QuitReason reason, string? message = null)
	{
		Reason = reason;
		Message = message;
	}
}
