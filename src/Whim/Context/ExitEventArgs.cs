namespace Whim;

/// <summary>
/// Event arguments for the <see cref="IContext.Exiting"/> event.
/// </summary>
public class ExitEventArgs : EventArgs
{
	/// <summary>
	/// The reason why Whim has been shut down.
	/// </summary>
	public required ExitReason Reason { get; init; }

	/// <summary>
	/// A string describing the reason why Whim has been shut down.
	/// </summary>
	public string? Message { get; init; }
}
