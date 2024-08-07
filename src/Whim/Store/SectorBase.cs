namespace Whim;

/// <summary>
/// A sector of data in Whim's <see cref="IStore"/>.
/// </summary>
internal abstract class SectorBase
{
	/// <summary>
	/// Queue of events to dispatch.
	/// </summary>
	protected readonly List<EventArgs> _events = [];

	/// <summary>
	/// Whether there are events queued.
	/// </summary>
	public virtual bool HasQueuedEvents => _events.Count > 0;

	/// <summary>
	/// Initialize the event listeners.
	/// </summary>
	public abstract void Initialize();

	/// <summary>
	/// Dispatch the events for the sector.
	/// </summary>
	public abstract void DispatchEvents();

	/// <summary>
	/// Add the given <paramref name="eventArgs"/> to the queue of events.
	/// </summary>
	/// <param name="eventArgs"></param>
	public void QueueEvent(EventArgs eventArgs)
	{
		_events.Add(eventArgs);
	}
}
