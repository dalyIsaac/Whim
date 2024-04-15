using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Operation describing how to update the state of the <see cref="Store"/>.
/// The implementing record should be populated with the payload.
/// <see cref="Execute"/> will specify how to update the store.
/// </summary>
public abstract record Transform()
{
	/// <summary>
	/// How to update the store.
	/// </summary>
	/// <param name="ctx">Whim's context.</param>
	/// <param name="internalCtx">Internal-only parts of Whim's API.</param>
	internal abstract void Execute(IContext ctx, IInternalContext internalCtx);
}

/// <summary>
/// Description of how to retrieve data from the <see cref="Store"/>.
/// The implementing record should be populated with the payload.
/// </summary>
/// <typeparam name="TResult">The type of the resulting data from the store.</typeparam>
public abstract record Picker<TResult>()
{
	/// <summary>
	/// How to fetch the data from the store.
	/// </summary>
	/// <param name="ctx">Whim's context.</param>
	/// <param name="internalCtx">Internal-only parts of Whim's API.</param>
	/// <returns></returns>
	internal abstract TResult Execute(IContext ctx, IInternalContext internalCtx);
}

/// <summary>
/// A slice of data in Whim's <see cref="Store"/>.
/// </summary>
public abstract class ISlice
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
	/// Dispatch the events for the slice.
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

/// <summary>
/// Whim's store.
/// </summary>
public interface IStore
{
	/// <inheritdoc cref="MonitorSlice"/>
	public MonitorSlice MonitorSlice { get; }

	/// <inheritdoc cref="WorkspaceSlice" />
	public WorkspaceSlice WorkspaceSlice { get; }

	/// <inheritdoc cref="MapSlice" />
	public MapSlice MapSlice { get; }

	/// <inheritdoc cref="WindowSlice" />
	public WindowSlice WindowSlice { get; }

	/// <summary>
	/// Dispatch updates to transform Whim's state.
	/// </summary>
	/// <param name="transform">
	/// The record implementing <see cref="Dispatch"/> to update Whim's state.
	/// </param>
	public void Dispatch(Transform transform);

	/// <summary>
	/// Entry-point to pick from Whim's state.
	/// </summary>
	/// <typeparam name="TResult">
	/// The type of the resulting data from the store.
	/// </typeparam>
	/// <param name="picker">
	/// The record implementing <see cref="Pick"/> to fetch from Whim's state.
	/// </param>
	/// <returns></returns>
	public TResult Pick<TResult>(Picker<TResult> picker);
}

/// <inheritdoc />
public class Store : IStore
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;

	/// <inheritdoc />
	public MonitorSlice MonitorSlice { get; }

	/// <inheritdoc />
	public WorkspaceSlice WorkspaceSlice { get; }

	/// <inheritdoc />
	public MapSlice MapSlice { get; }

	/// <inheritdoc />
	public WindowSlice WindowSlice { get; }

	internal Store(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;

		MonitorSlice = new MonitorSlice(ctx, internalCtx);
		WorkspaceSlice = new WorkspaceSlice();
		MapSlice = new MapSlice();
		WindowSlice = new WindowSlice();
	}

	/// <inheritdoc />
	public void Dispatch(Transform transform)
	{
		// TODO: reader-writer lock.
		transform.Execute(_ctx, _internalCtx);
		DispatchEvents();
	}

	private void DispatchEvents()
	{
		Logger.Debug("Dispatching events");
		MonitorSlice.DispatchEvents();
		WorkspaceSlice.DispatchEvents();
		MapSlice.DispatchEvents();
		WindowSlice.DispatchEvents();
	}

	/// <inheritdoc cref="WindowSlice" />
	public TResult Pick<TResult>(Picker<TResult> selector)
	{
		// TODO: reader-writer lock.
		// TODO: don't do a read lock if a transform is currently in progress.
		return selector.Execute(_ctx, _internalCtx);
	}
}
