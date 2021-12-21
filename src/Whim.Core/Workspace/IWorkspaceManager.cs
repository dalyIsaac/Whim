using System;
using System.Collections.Generic;

namespace Whim.Core;

/// <summary>
/// The manager for <see cref="IWorkspace"/>s. This is responsible for routing
/// windows between workspaces.
/// </summary>
public interface IWorkspaceManager : IEnumerable<IWorkspace>, ICommandable
{
	/// <summary>
	/// The active workspace.
	/// </summary>
	public IWorkspace? ActiveWorkspace { get; }

	/// <summary>
	/// Initialize the event listeners.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Description of how an <see cref="IWindow"/> has been routed between workspaces.
	/// </summary>
	public event EventHandler<RouteEventArgs>? WorkspaceRouted;

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
	public event EventHandler<WorkspaceMonitorChangedEventArgs>? WorkspaceMonitorChanged;

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
}
