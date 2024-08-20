namespace Whim;

/// <summary>
/// Contains the state of Whim's monitors, windows, and workspaces.
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
	/// <returns>
	/// The result of the transformation.
	/// </returns>
	/// <example>
	/// If you want to get the full result and handle it appropriately:
	/// <code>
	/// Result&lt;Guid:gt; firstWorkspace = context.Store.Dispatch(new AddWorkspaceTransform("First Workspace"));
	/// </code>
	///
	/// If you want to assume the result is successful and get the value (this will throw an exception
	/// if it fails):
	/// <code>
	/// Guid firstWorkspace = context.Store.Dispatch(new AddWorkspaceTransform("First Workspace")).Value;
	/// </code>
	/// </example>
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
	/// <returns>
	/// The result of the picker applied to Whim's state.
	/// </returns>
	/// <example>
	/// If you want to get the full result and handle it appropriately:
	/// <code>
	/// Result&lt;IMonitor&gt; monitor = context.Store.Pick(PickMonitorAtPoint(new Point&lt;int&gt;(100, 200)));
	/// </code>
	///
	/// If you want to assume the result is successful and get the value (this will throw an exception
	/// if it fails):
	/// <code>
	/// IMonitor monitor = context.Store.Pick(PickMonitorAtPoint(new Point&lt;int&gt;(100, 200))).Value;
	/// </code>
	/// </example>
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
	/// <returns>
	/// The result of the picker applied to Whim's state.
	/// </returns>
	/// <example>
	/// If you want to get the full result and handle it appropriately:
	/// <code>
	/// Result&lt;IWorkspace> workspace = context.Store.Pick(PickWorkspaceByWindow(window.Handle));
	/// </code>
	///
	/// If you want to assume the result is successful and get the value (this will throw an exception
	/// if it fails):
	/// <code>
	/// IWorkspace workspace = context.Store.Pick(PickWorkspaceByWindow(window.Handle)).Value;
	/// </code>
	/// </example>
	public TResult Pick<TResult>(PurePicker<TResult> picker);
}
