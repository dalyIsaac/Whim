using System;
using DotNext;

namespace Whim;

/// <summary>
/// Whim's store.
/// WARNING: Currently non-functional - use managers instead.
/// </summary>
public interface IStore : IDisposable
{
	/// <summary>
	/// Whether the store is being disposed.
	/// </summary>
	bool IsDisposing { get; }

	/// <inheritdoc cref="IMonitorSectorEvents"/>
	IMonitorSectorEvents MonitorEvents { get; }

	/// <inheritdoc cref="IWindowSectorEvents"/>
	IWindowSectorEvents WindowEvents { get; }

	/// <inheritdoc cref="IMapSectorEvents"/>
	IMapSectorEvents MapEvents { get; }

	/// <inheritdoc cref="IWorkspaceSectorEvents"/>
	IWorkspaceSectorEvents WorkspaceEvents { get; }

	/// <summary>
	/// Initialize the event listeners.
	/// </summary>
	public void Initialize();

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
	/// The record implementing <see cref="Picker{TResult}"/> to fetch from Whim's state.
	/// </param>
	/// <returns></returns>
	public TResult Pick<TResult>(Picker<TResult> picker);

	/// <summary>
	/// Entry-point to pick from Whim's state.
	/// </summary>
	/// <typeparam name="TResult">
	/// The type of the resulting data from the store.
	/// </typeparam>
	/// <param name="picker">
	/// Pure picker to fetch from Whim's state.
	/// </param>
	/// <returns></returns>
	public TResult Pick<TResult>(PurePicker<TResult> picker);
}
