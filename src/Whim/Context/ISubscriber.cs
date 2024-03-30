namespace Whim;

/// <summary>
/// A Whim object which subscribes to events. The <see cref="Subscribe"/> method <b>must</b> be
/// called on the STA thread.
/// </summary>
internal interface ISubscriber
{
	/// <summary>
	/// Subscribe to events. This <b>must</b> be called on the STA thread.
	/// </summary>
	void Subscribe();
}
