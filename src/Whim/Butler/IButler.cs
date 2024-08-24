namespace Whim;

/// <summary>
/// The butler is responsible for using the <see cref="IWorkspaceManager"/> and <see cref="IMonitorManager"/>
/// to handle events from the <see cref="IWindowManager"/> to update the assignment of <see cref="IWindow"/>s
/// to <see cref="IWorkspace"/>s, and <see cref="IWorkspace"/>s to <see cref="IMonitor"/>s.
/// </summary>
[Obsolete("Use transforms and pickers to interact with the store instead.")]
public interface IButler : IButlerChores, IDisposable
{
	/// <summary>
	/// The pantry is responsible for mapping <see cref="IWindow"/>s to <see cref="IWorkspace"/>s
	/// to <see cref="IMonitor"/>s.
	///
	/// Defaults to <see cref="ButlerPantry"/>.
	/// </summary>
	[Obsolete("Use transforms and pickers to interact with the store instead.")]
	IButlerPantry Pantry { get; }

	/// <summary>
	/// Initializes the butler with the store.
	/// </summary>
	void Initialize();

	/// <summary>
	/// Description of how an <see cref="IWindow"/> has been routed between workspaces.
	/// </summary>
	[Obsolete("Use the IStore.MapEvents.WindowRouted event instead.")]
	event EventHandler<RouteEventArgs>? WindowRouted;

	/// <summary>
	/// Event for when a monitor's workspace has changed.
	/// </summary>
	[Obsolete("Use the IStore.MapEvents.MonitorWorkspaceChanged event instead.")]
	event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;
}
