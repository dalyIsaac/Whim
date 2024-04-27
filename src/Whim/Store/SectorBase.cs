using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// A sector of data in Whim's <see cref="Store"/>.
/// </summary>
public abstract class SectorBase
{
	/// <summary>
	/// Queue of events to dispatch.
	/// </summary>
	protected readonly List<EventArgs> _events = new();

	/// <summary>
	/// Initialize the event listeners.
	/// </summary>
	internal abstract void Initialize();

	/// <summary>
	/// Dispatch the events for the sector.
	/// </summary>
	internal abstract void DispatchEvents();

	/// <summary>
	/// Add the given <paramref name="eventArgs"/> to the queue of events.
	/// </summary>
	/// <param name="eventArgs"></param>
	internal void QueueEvent(EventArgs eventArgs)
	{
		_events.Add(eventArgs);
	}
}
