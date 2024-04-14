using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Operation describing how to update the <see cref="RootSlice"/> for the <see cref="Store"/>.
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
	/// <param name="root">The root slice.</param>
	internal abstract void Execute(IContext ctx, IInternalContext internalCtx, RootSlice root);
}

/// <summary>
/// Description of how to retrieve data from the <see cref="Store"/>.
/// The implementing record should be populated with the payload.
/// </summary>
/// <typeparam name="TResult">The type of the resulting data from the store.</typeparam>
public abstract record Selector<TResult>()
{
	/// <summary>
	/// How to fetch the data from the store.
	/// </summary>
	/// <param name="ctx">Whim's context.</param>
	/// <param name="internalCtx">Internal-only parts of Whim's API.</param>
	/// <param name="root">The root slice.</param>
	/// <returns></returns>
	internal abstract TResult Execute(IContext ctx, IInternalContext internalCtx, RootSlice root);
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
/// The root slice for the <see cref="Store"/>.
/// </summary>
internal class RootSlice
{
	public MonitorSlice MonitorSlice { get; }
	public WorkspaceSlice WorkspaceSlice { get; }
	public MapSlice MapSlice { get; }
	public WindowSlice WindowSlice { get; }

	public RootSlice(IContext ctx, IInternalContext internalCtx)
	{
		MonitorSlice = new MonitorSlice(ctx, internalCtx);
		WorkspaceSlice = new WorkspaceSlice();
		MapSlice = new MapSlice();
		WindowSlice = new WindowSlice();
	}

	public void DispatchEvents()
	{
		Logger.Debug("Dispatching events");
		MonitorSlice.DispatchEvents();
		WorkspaceSlice.DispatchEvents();
		MapSlice.DispatchEvents();
		WindowSlice.DispatchEvents();
	}
}

/// <summary>
/// Whim's store.
/// </summary>
public class Store
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;

	private readonly RootSlice _rootSlice;

	internal Store(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;
		_rootSlice = new(ctx, internalCtx);
	}

	/// <summary>
	/// Dispatch updates to transform Whim's state.
	/// </summary>
	/// <param name="transform">
	/// The record implementing <see cref="Dispatch"/> to update Whim's state.
	/// </param>
	public void Dispatch(Transform transform)
	{
		// TODO: reader-writer lock.
		transform.Execute(_ctx, _internalCtx, _rootSlice);
	}

	/// <summary>
	/// Entry-point to select from Whim's state.
	/// </summary>
	/// <typeparam name="TResult">
	/// The type of the resulting data from the store.
	/// </typeparam>
	/// <param name="selector">
	/// The record implementing <see cref="Select"/> to fetch from Whim's state.
	/// </param>
	/// <returns></returns>
	public TResult Select<TResult>(Selector<TResult> selector)
	{
		// TODO: reader-writer lock.
		// TODO: don't do a read lock if a transform is currently in progress.
		return selector.Execute(_ctx, _internalCtx, _rootSlice);
	}
}
