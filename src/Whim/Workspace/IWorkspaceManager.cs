using System;
using System.Collections.Generic;

namespace Whim;

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
	/// Creates the default layout engines to add to a workspace.
	/// </summary>
	Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; }

	/// <summary>
	/// Creates the workspaces from the provided names and <see cref="CreateLayoutEngines"/> function.
	/// Do not call this directly - Whim will call this when it is ready.
	/// </summary>
	void Initialize();

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
	/// <returns>
	/// <list type="bullet">
	/// <item>
	/// <description>If <see cref="Initialize"/> has not been called, returns <see langword="null"/>.</description>
	/// </item>
	/// <item>
	/// <description>If a workspace cannot be created, returns <see langword="null"/>.</description>
	/// </item>
	/// <item>
	/// <description>Otherwise, returns the created workspace.</description>
	/// </item>
	/// </list>
	/// </returns>
	IWorkspace? Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null);

	/// <summary>
	/// Whether the manager contains the given workspace.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns></returns>
	bool Contains(IWorkspace workspace);

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
	/// Adds a proxy layout engine to the workspace manager.
	/// A proxy layout engine is used by plugins to add layout functionality to
	/// all workspaces.
	/// This should be used by <see cref="IPlugin"/>s.
	/// </summary>
	/// <param name="proxyLayoutEngine">The proxy layout engine to add.</param>
	void AddProxyLayoutEngine(CreateProxyLayoutEngine proxyLayoutEngine);
}
