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
public interface IWorkspaceManager : IEnumerable<IWorkspace>
{
	/// <summary>
	/// The active workspace.
	/// </summary>
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

	/// <summary>
	/// Creates the workspaces from the provided names and <see cref="CreateLayoutEngines"/> function.
	/// </summary>
	void Initialize();

	/// <summary>
	/// Tries to get a workspace by the given name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and get.</param>
	IWorkspace? this[string workspaceName] { get; }

	/// <summary>
	/// Add a new workspace.
	/// </summary>
	/// <param name="name">
	/// The name of the workspace. Defaults to <see langword="null"/>, which will generate the name
	/// <c>Workspace {n}</c>.
	/// </param>
	/// <param name="createLayoutEngines">
	/// The layout engines to add to the workspace. Defaults to <see langword="null"/>, which will
	/// use the <see cref="CreateLayoutEngines"/> function.
	/// </param>
	void Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null);

	/// <summary>
	/// Whether the manager contains the given workspace.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns></returns>
	bool Contains(IWorkspace workspace);

	/// <summary>
	/// Creates the default layout engines to add to a workspace.
	/// </summary>
	Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; }

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

	// TODO: Maybe use a triggers object?
	// NOTE: Event handlers called by Workspace.
	void OnWorkspaceAdded(WorkspaceEventArgs args);

	void OnWorkspaceRemoved(WorkspaceEventArgs args);

	void OnWorkspaceRenamed(WorkspaceRenamedEventArgs args);

	void OnWorkspaceLayoutStarted(WorkspaceEventArgs args);

	void OnWorkspaceLayoutCompleted(WorkspaceEventArgs args);

	void OnActiveLayoutEngineChanged(ActiveLayoutEngineChangedEventArgs args);

	/// <summary>
	/// Tries to get a workspace by the given name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and get.</param>
	IWorkspace? TryGet(string workspaceName);
}
