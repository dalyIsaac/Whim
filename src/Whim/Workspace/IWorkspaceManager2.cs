using System;

namespace Whim;

// TODO: Order
public interface IWorkspaceManager2 : IDisposable
{
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

	IWorkspace ActiveWorkspace { get; }

	IWorkspaceContainer WorkspaceContainer { get; }

	void Initialize();

	void Activate(IWorkspace workspace, IMonitor? monitor = null);

	void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false);

	void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false);

	void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false);

	void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null);

	void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null);

	void MoveWindowToPreviousMonitor(IWindow? window = null);

	void MoveWindowToNextMonitor(IWindow? window = null);

	void MoveWindowToPoint(IWindow window, IPoint<int> point);

	bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null);
}
