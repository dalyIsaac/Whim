using System;

namespace Whim;

// TODO: Order

/// <summary>
/// The butler is responsible for using the <see cref="IWorkspaceManager"/> and <see cref="IMonitorManager"/>
/// to handle events from the <see cref="IWindowManager"/> to update the assignment of <see cref="IWindow"/>s
/// to <see cref="IWorkspace"/>s, and <see cref="IWorkspace"/>s to <see cref="IMonitor"/>s.
/// </summary>
public interface IButler : IDisposable, IButlerChores
{
	/// <summary>
	/// Description of how an <see cref="IWindow"/> has been routed between workspaces.
	/// </summary>
	event EventHandler<RouteEventArgs>? WindowRouted;

	/// <summary>
	/// Event for when a monitor's workspace has changed.
	/// </summary>
	event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;

	void PreInitialize();

	void Initialize();

	#region ButlerPantry methods
	IWorkspace? GetWorkspaceForMonitor(IMonitor monitor);

	IWorkspace? GetWorkspaceForWindow(IWindow window);

	IMonitor? GetMonitorForWorkspace(IWorkspace workspace);

	IMonitor? GetMonitorForWindow(IWindow window);
	#endregion
}
