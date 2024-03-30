using System;
using System.Collections.Generic;
using System.Threading;

namespace Whim;

/// <summary>
/// Invokes event handlers more safely than normal event handlers. If the UI thread subscribes to an
/// event, then when the event is invoked the delegate is called on the UI thread.
/// Otherwise, the event is invocation continues on the current thread.
///
/// For Whim's core, only <c>public</c> events in <c>public</c> classes need to implement this.
/// </summary>
public class ThreadSafeEvent<T>
{
	private readonly IContext _context;
	private readonly List<(EventHandler<T> Handler, bool IsSTA)> _handlers = new();

	/// <summary>
	/// Creates an <see cref="ThreadSafeEvent{T}"/>, with easy access to the <see cref="IContext"/>.
	/// </summary>
	/// <param name="context"></param>
	public ThreadSafeEvent(IContext context)
	{
		_context = context;
	}

	/// <summary>
	/// Adds the given <paramref name="handler"/>.
	/// </summary>
	/// <param name="handler"></param>
	public void Add(EventHandler<T>? handler)
	{
		if (handler is null)
		{
			return;
		}

		ApartmentState state = Thread.CurrentThread.GetApartmentState();
		_handlers.Add((handler, state == ApartmentState.STA));
	}

	/// <summary>
	/// Removes the first instance of <paramref name="handler"/>.
	/// </summary>
	/// <param name="handler"></param>
	/// <returns>
	/// <c>true</c> if <paramref name="handler"/> was found, otherwise <c>false</c>.
	/// </returns>
	public bool Remove(EventHandler<T>? handler)
	{
		if (handler is null)
		{
			return false;
		}

		int idxToRemove = -1;
		for (int idx = 0; idx < _handlers.Count; idx++)
		{
			if (_handlers[idx].Handler == handler)
			{
				idxToRemove = idx;
				break;
			}
		}

		if (idxToRemove != -1)
		{
			_handlers.RemoveAt(idxToRemove);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Invoke the event. If the subscriber is from the STA thread, invoke on the UI thread. Otherwise,
	/// continue on the current thread.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="args"></param>
	public void Invoke(object sender, T args)
	{
		foreach ((EventHandler<T> handler, bool isSta) in _handlers)
		{
			if (isSta)
			{
				_context.NativeManager.InvokeOnUIThread(() => handler.Invoke(sender, args));
			}
			else
			{
				handler.Invoke(sender, args);
			}
		}
	}
}
