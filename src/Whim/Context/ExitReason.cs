namespace Whim;

/// <summary>
/// The possible reasons why Whim has been shut down.
/// </summary>
public enum ExitReason
{
	/// <summary>
	/// Whim has been shut down normally.
	/// </summary>
	User,

	/// <summary>
	/// Whim has been shut down because of an error.
	/// </summary>
	Error,

	/// <summary>
	/// Whim has been shut down as it's currently restarting.
	/// </summary>
	Restart,

	/// <summary>
	/// Whim has been shut down as it's currently updating.
	/// </summary>
	Update,
}
