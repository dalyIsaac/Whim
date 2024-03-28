internal interface ISubscriber
{
	/// <summary>
	/// Subscribe to events. This <b>must</b> be called on the STA thread.
	/// </summary>
	void Subscribe();
}
