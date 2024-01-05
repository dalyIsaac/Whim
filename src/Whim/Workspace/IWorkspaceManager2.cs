using System;
using System.Collections.Generic;

namespace Whim;

// TODO: Order
/// <summary>
/// Container responsible for mapping <see cref="IWorkspace"/>s to <see cref="IMonitor"/>s and
/// <see cref="IWindow"/>s.
///
/// It is responsible for the creation and destruction of <see cref="IWorkspace"/>s.
/// </summary>
public interface IWorkspaceManager2 : IEnumerable<IWorkspace>
{
	IWorkspace ActiveWorkspace { get; }

	/// <summary>
	/// Event for when a workspace is added.
	/// </summary>
	event EventHandler<WorkspaceEventArgs>? WorkspaceAdded;

	/// <summary>
	/// Event for when a workspace is removed.
	/// </summary>
	event EventHandler<WorkspaceEventArgs>? WorkspaceRemoved;

	/// <summary>
	/// Event for when a workspace is renamed.
	/// </summary>
	event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	/// <summary>
	/// Event for when <see cref="IWorkspace.DoLayout"/> has started.
	/// </summary>
	event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutStarted;

	/// <summary>
	/// Event for when <see cref="IWorkspace.DoLayout"/> has completed.
	/// </summary>
	event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutCompleted;

	/// <summary>
	/// Event for when a workspace's active layout engine has changed.
	/// </summary>
	event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	void Initialize();

	IWorkspace? this[string workspaceName] { get; }

	IWorkspace? TryGet(string workspaceName);

	IWorkspace? Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null);

	bool Remove(IWorkspace workspace);

	bool Remove(string workspaceName);

	bool Contains(IWorkspace workspace);

	bool RemoveWorkspace(IWorkspace workspace);

	bool RemoveWindow(IWindow window);

	bool RemoveMonitor(IMonitor monitor);

	IWorkspace? GetWorkspaceForMonitor(IMonitor monitor);

	IMonitor? GetMonitorForWorkspace(IWorkspace workspace);

	void SetWorkspaceMonitor(IWorkspace workspace, IMonitor monitor);

	void SetWindowWorkspace(IWindow window, IWorkspace workspace);

	IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse, bool skipActive);

	IWorkspace? GetWorkspaceForWindow(IWindow window);
}
