using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// The manager for <see cref="IWorkspace"/>s. This is responsible for routing
/// windows between workspaces.
/// </summary>
public interface IWorkspaceManager : IEnumerable<IWorkspace>, IDisposable
{
	/// <summary>
	/// The active workspace.
	/// </summary>
	IWorkspace ActiveWorkspace { get; }

	/// <summary>
	/// Creates the default layout engines to add to a workspace.
	/// </summary>
	Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; }

	/// <summary>
	/// Initialize the event listeners.
	/// </summary>
	void Initialize();

	/// <summary>
	/// Description of how an <see cref="IWindow"/> has been routed between workspaces.
	/// </summary>
	event EventHandler<RouteEventArgs>? WindowRouted;

	/// <summary>
	/// Event for when a workspace is added.
	/// </summary>
	event EventHandler<WorkspaceEventArgs>? WorkspaceAdded;

	/// <summary>
	/// Event for when a workspace is removed.
	/// </summary>
	event EventHandler<WorkspaceEventArgs>? WorkspaceRemoved;

	/// <summary>
	/// Event for when <see cref="IWorkspace.DoLayout"/> has started.
	/// </summary>
	event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutStarted;

	/// <summary>
	/// Event for when <see cref="IWorkspace.DoLayout"/> has completed.
	/// </summary>
	event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutCompleted;

	/// <summary>
	/// Event for when a monitor's workspace has changed.
	/// </summary>
	event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;

	/// <summary>
	/// Event for when a workspace's active layout engine has changed.
	/// </summary>
	event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	/// <summary>
	/// Event for when a workspace is renamed.
	/// </summary>
	event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	/// <summary>
	/// Triggers all active workspaces to update their layout.
	/// Active workspaces are those that are visible on a monitor.
	/// </summary>
	void LayoutAllActiveWorkspaces();

	/// <summary>
	/// Add a new workspace.
	/// </summary>
	/// <param name="name">
	/// The name of the workspace. Defaults to <see langword="null"/>, which will generate the name
	/// <c>Workspace {n}</c>.
	/// </param>
	/// <param name="layoutEngines">
	/// The layout engines to add to the workspace. Defaults to <see langword="null"/>, which will
	/// use the <see cref="CreateLayoutEngines"/> function.
	/// </param>
	void Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? layoutEngines = null);

	/// <summary>
	/// Tries to remove the given workspace.
	/// </summary>
	/// <param name="workspace">The workspace to remove.</param>
	bool Remove(IWorkspace workspace);

	/// <summary>
	/// Try remove a workspace, by the workspace name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and remove.</param>
	/// <returns><c>true</c> when the workspace has been removed.</returns>
	bool Remove(string workspaceName);

	/// <summary>
	/// Tries to get a workspace by the given name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and get.</param>
	IWorkspace? TryGet(string workspaceName);

	/// <summary>
	/// Tries to get a workspace by the given name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and get.</param>
	IWorkspace? this[string workspaceName] { get; }

	/// <summary>
	/// Activates the given workspace in the active monitor, or the given monitor (if provided).
	/// </summary>
	/// <param name="workspace">The workspace to activate.</param>
	/// <param name="monitor">
	/// The monitor to activate the workspace in. If <see langword="null"/>, this will default to
	/// the active monitor.
	/// </param>
	void Activate(IWorkspace workspace, IMonitor? monitor = null);

	/// <summary>
	/// Activates the previous workspace in the given monitor, or the focused monitor (if provided).
	/// </summary>
	/// <param name="monitor">
	/// The monitor to activate the workspace in. If <see langword="null"/>, this will default to
	/// the focused monitor.
	/// </param>
	[Obsolete("Use ActivateAdjacent instead, with `reverse: true`")]
	void ActivatePrevious(IMonitor? monitor = null);

	/// <summary>
	/// Activates the next workspace in the given monitor, or the focused monitor (if provided).
	/// </summary>
	/// <param name="monitor">
	/// The monitor to activate the workspace in. If <see langword="null"/>, this will default to
	/// the focused monitor.
	/// </param>

	[Obsolete("Use ActivateAdjacent instead, with `reverse: false`")]
	void ActivateNext(IMonitor? monitor = null);

	/// <summary>
	/// Activates the next (or previous) workspace in the given monitor.
	/// </summary>
	/// <param name="monitor">
	/// The monitor to activate the next workspace in. Defaults to <see cref="IMonitorManager.ActiveMonitor"/>.
	/// </param>
	/// <param name="reverse">
	/// When <see langword="true"/>, gets the previous monitor, otherwise gets the next monitor. Defaults to <see langword="false" />.
	/// </param>
	/// <param name="skipActive">
	/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
	/// </param>
	void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the next (or previous) workspace.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to the focused/active window.
	/// </param>
	/// <param name="reverse">
	/// When <see langword="true"/>, moves to the previous workspace, otherwise moves to the next workspace. Defaults to <see langword="false" />.
	/// </param>
	/// <param name="skipActive">
	/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
	/// </param>
	void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false);

	/// <summary>
	/// Retrieves the monitor for the active workspace.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns><see langword="null"/> if the workspace is not active.</returns>
	IMonitor? GetMonitorForWorkspace(IWorkspace workspace);

	/// <summary>
	/// Retrieves the active workspace for the given monitor.
	/// </summary>
	/// <param name="monitor"></param>
	/// <returns><see langword="null"/> if the monitor is not active.</returns>
	IWorkspace? GetWorkspaceForMonitor(IMonitor monitor);

	/// <summary>
	/// Retrieves the workspace for the given window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns><see langword="null"/> if the window is not in a workspace.</returns>
	IWorkspace? GetWorkspaceForWindow(IWindow window);

	/// <summary>
	/// Retrieves the monitor for the given window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns><see langword="null"/> if the window is not in a workspace.</returns>
	IMonitor? GetMonitorForWindow(IWindow window);

	/// <summary>
	/// Adds a proxy layout engine to the workspace manager.
	/// A proxy layout engine is used by plugins to add layout functionality to
	/// all workspaces.
	/// This should be used by <see cref="IPlugin"/>s.
	/// </summary>
	/// <param name="proxyLayoutEngine">The proxy layout engine to add.</param>
	void AddProxyLayoutEngine(CreateProxyLayoutEngine proxyLayoutEngine);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="workspace"/>.
	/// </summary>
	/// <param name="workspace">The workspace to move the window to.</param>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="monitor"/>.
	/// </summary>
	/// <param name="monitor">The monitor to move the window to.</param>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the previous monitor.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	void MoveWindowToPreviousMonitor(IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the next monitor.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	void MoveWindowToNextMonitor(IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="point"/>.
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">
	/// The point to move the window to. The point is in the coordinate space of the monitors,
	/// not the unit square.
	/// </param>
	void MoveWindowToPoint(IWindow window, IPoint<int> point);

	/// <summary>
	/// Moves the given <paramref name="window"/> by the given <paramref name="pixelsDeltas"/>.
	/// </summary>
	/// <param name="edges">The edges to change.</param>
	/// <param name="pixelsDeltas">
	/// The deltas (in pixels) to change the given <paramref name="edges"/> by. When a value is
	/// positive, then the edge will move in the direction of the <paramref name="edges"/>.
	/// The <paramref name="pixelsDeltas"/> are in the coordinate space of the monitors, not the
	/// unit square.
	/// </param>
	/// <param name="window"></param>
	/// <returns>Whether the window's edges were moved.</returns>
	bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null);
}
