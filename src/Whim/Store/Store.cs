using System;
using DotNext;

namespace Whim;

/// <summary>
/// An empty type for <see cref="Result{T}"/>s which don't return anything.
/// </summary>
public record Empty()
{
	/// <summary>
	/// Default placeholder for empty result.
	/// </summary>
	public static Result<Empty> Result { get; } = DotNext.Result.FromValue(new Empty());
}

/// <summary>
/// Base class for the transform.
/// </summary>
public abstract record TransformBase();

/// <summary>
/// Operation describing how to update the state of the <see cref="Store"/>.
/// The implementing record should be populated with the payload.
/// <see cref="Execute"/> will specify how to update the store.
/// </summary>
public abstract record Transform() : TransformBase()
{
	/// <summary>
	/// How to update the store.
	/// </summary>
	/// <param name="ctx">Whim's context.</param>
	/// <param name="internalCtx">Internal-only parts of Whim's API.</param>
	internal abstract Result<Empty> Execute(IContext ctx, IInternalContext internalCtx);
}

/// <summary>
/// Operation describing how to update the state of the <see cref="Store"/>.
/// The implementing record should be populated with the payload.
/// <see cref="Execute"/> will specify how to update the store.
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract record Transform<TResult>()
{
	/// <summary>
	/// How to update the store.
	/// </summary>
	/// <param name="ctx">Whim's context.</param>
	/// <param name="internalCtx">Internal-only parts of Whim's API.</param>
	/// <returns>A wrapped result.</returns>
	internal abstract Result<TResult> Execute(IContext ctx, IInternalContext internalCtx);
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
/// Whim's store.
/// </summary>
public interface IStore : IDisposable
{
	/// <inheritdoc cref="MonitorSector"/>
	public MonitorSector Monitors { get; }

	/// <inheritdoc cref="WorkspaceSector" />
	public WorkspaceSector Workspaces { get; }

	/// <inheritdoc cref="MapSector" />
	public MapSector Maps { get; }

	/// <inheritdoc cref="MapSector" />
	public WindowSector Windows { get; }

	/// <summary>
	/// Initialize the event listeners.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Dispatch updates to transform Whim's state.
	/// </summary>
	/// <param name="transform">
	/// The record implementing <see cref="Dispatch"/> to update Whim's state.
	/// </param>
	public void Dispatch(Transform transform);

	/// <summary>
	/// Dispatch updates to transform Whim's state.
	/// </summary>
	/// <typeparam name="TResult">
	/// The result from the transform, if it's successful.
	/// </typeparam>
	/// <param name="transform">
	/// The record implementing <see cref="Dispatch"/> to update Whim's state.
	/// </param>
	/// <returns></returns>
	public Result<TResult> Dispatch<TResult>(Transform<TResult> transform);

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
	private bool _disposedValue;

	/// <inheritdoc />
	public MonitorSector Monitors { get; }

	/// <inheritdoc />
	public WorkspaceSector Workspaces { get; }

	/// <inheritdoc />
	public MapSector Maps { get; }

	/// <inheritdoc />
	public WindowSector Windows { get; }

	internal Store(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;

		Monitors = new MonitorSector(ctx, internalCtx);
		Workspaces = new WorkspaceSector();
		Maps = new MapSector();
		Windows = new WindowSector();
	}

	/// <inheritdoc />
	public void Initialize()
	{
		Monitors.Initialize();
		Workspaces.Initialize();
		Maps.Initialize();
		Windows.Initialize();
	}

	/// <inheritdoc />
	public void Dispatch(Transform transform)
	{
		// TODO: reader-writer lock.
		transform.Execute(_ctx, _internalCtx);
		DispatchEvents();
	}

	/// <inheritdoc />
	public Result<TResult> Dispatch<TResult>(Transform<TResult> transform)
	{
		Result<TResult> result = transform.Execute(_ctx, _internalCtx);
		DispatchEvents();
		return result;
	}

	private void DispatchEvents()
	{
		Logger.Debug("Dispatching events");
		Monitors.DispatchEvents();
		Workspaces.DispatchEvents();
		Maps.DispatchEvents();
		Windows.DispatchEvents();
	}

	/// <inheritdoc cref="Windows" />
	public TResult Pick<TResult>(Picker<TResult> picker)
	{
		// TODO: reader-writer lock.
		// don't do a read lock if a transform is currently in progress.
		return picker.Execute(_ctx, _internalCtx);
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				Monitors.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
