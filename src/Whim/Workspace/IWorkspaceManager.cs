using System;
using System.Collections.Generic;

namespace Whim;

public delegate ILayoutEngine ProxyLayoutEngine(ILayoutEngine engine);

/// <summary>
/// The manager for <see cref="IWorkspace"/>s. This is responsible for routing
/// windows between workspaces.
/// </summary>
public interface IWorkspaceManager : IEnumerable<IWorkspace>, ICommandable
{
	/// <summary>
	/// The active workspace.
	/// </summary>
	public IWorkspace ActiveWorkspace { get; }

	/// <summary>
	/// Initialize the event listeners.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Description of how an <see cref="IWindow"/> has been routed between workspaces.
	/// </summary>
	public event EventHandler<RouteEventArgs>? WindowRouted;

	/// <summary>
	/// Event for when a workspace is added.
	/// </summary>
	public event EventHandler<WorkspaceEventArgs>? WorkspaceAdded;

	/// <summary>
	/// Event for when a workspace is removed.
	/// </summary>
	public event EventHandler<WorkspaceEventArgs>? WorkspaceRemoved;

	/// <summary>
	/// Event for when a monitor's workspace has changed.
	/// </summary>
	public event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;

	/// <summary>
	/// Event for when a workspace's active layout engine has changed.
	/// </summary>
	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	/// <summary>
	/// Triggered when workspace is renamed.
	/// </summary>
	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	/// <summary>
	/// Used by <see cref="IWorkspace"/> to trigger <see cref="ActiveLayoutEngineChanged"/>.
	/// </summary>
	public void TriggerActiveLayoutEngineChanged(ActiveLayoutEngineChangedEventArgs args);

	/// <summary>
	/// Used by <see cref="IWorkspace"/> to trigger <see cref="WorkspaceRenamed"/>.
	/// </summary>
	public void TriggerWorkspaceRenamed(WorkspaceRenamedEventArgs args);

	/// <summary>
	/// Triggers all active workspaces to update their layout.
	/// Active workspaces are those that are visible on a monitor.
	/// </summary>
	public void LayoutAllActiveWorkspaces();

	/// <summary>
	/// The <see cref="IWorkspace"/> to add.
	/// </summary>
	/// <param name="workspaces"></param>
	public void Add(IWorkspace workspace);

	/// <summary>
	/// Tries to remove the given workspace.
	/// </summary>
	/// <param name="workspace">The workspace to remove.</param>
	public bool Remove(IWorkspace workspace);

	/// <summary>
	/// Try remove a workspace, by the workspace name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and remove.</param>
	/// <returns><c>true</c> when the workspace has been removed.</returns>
	public bool Remove(string workspaceName);

	/// <summary>
	/// Tries to get a workspace by the given name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and get.</param>
	public IWorkspace? TryGet(string workspaceName);

	/// <summary>
	/// Tries to get a workspace by the given name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and get.</param>
	public IWorkspace? this[string workspaceName] { get; }

	/// <summary>
	/// Activates the given workspace in the focused monitor, or the given monitor (if provided).
	/// </summary>
	/// <param name="workspace">The workspace to activate.</param>
	/// <param name="monitor">
	/// The monitor to activate the workspace in. If <see langword="null"/>, this will default to
	/// the focused monitor.
	/// </param>
	public void Activate(IWorkspace workspace, IMonitor? monitor = null);

	/// <summary>
	/// Retrieves the monitor for the active workspace.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns><see langword="null"/> if the workspace is not active.</returns>
	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace);

	/// <summary>
	/// Retrieves the active workspace for the given monitor.
	/// </summary>
	/// <param name="monitor"></param>
	/// <returns><see langword="null"/> if the monitor is not active.</returns>
	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor);

	/// <summary>
	/// Retrieves the monitor for the given window.
	/// </summary>
	public IMonitor? GetMonitorForWindow(IWindow window);

	/// <summary>
	/// Adds a proxy layout engine to the workspace manager.
	/// A proxy layout engine is used by plugins to add layout functionality to
	/// all workspaces.
	/// </summary>
	/// <param name="proxyLayoutEngine">The proxy layout engine to add.</param>
	public void AddProxyLayoutEngine(ProxyLayoutEngine proxyLayoutEngine);

	/// <summary>
	/// The proxy layout engines.
	/// </summary>
	public IEnumerable<ProxyLayoutEngine> ProxyLayoutEngines { get; }

	/// <summary>
	/// Moves the given <see paramref="window"/> to the given <see paramref="workspace"/>.
	/// </summary>
	/// <param name="workspace">The workspace to move the window to.</param>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	public void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null);

	/// <summary>
	/// Moves the given <see paramref="window"/> to the given <see paramref="monitor"/>.
	/// </summary>
	/// <param name="monitor">The monitor to move the window to.</param>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null);

	/// <summary>
	/// Moves the given <see paramref="window"/> to the previous monitor.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	public void MoveWindowToPreviousMonitor(IWindow? window = null);

	/// <summary>
	/// Moves the given <see paramref="window"/> to the next monitor.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	public void MoveWindowToNextMonitor(IWindow? window = null);

	#region Phantom Windows
	/// <summary>
	/// Register a phantom window for the given <paramref name="workspace"/>.
	/// </summary>
	/// <param name="window">The phantom window to register.</param>
	/// <param name="workspace">The workspace to register the window for.</param>
	public void RegisterPhantomWindow(IWorkspace workspace, IWindow window);

	/// <summary>
	/// Unregister the given phantom window.
	/// </summary>
	public void UnregisterPhantomWindow(IWindow window);
	#endregion
}
