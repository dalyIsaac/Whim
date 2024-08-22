namespace Whim;

/// <summary>
/// Container responsible for the creation and removal of <see cref="IWorkspace"/>s. Events for
/// workspaces are exposed here.
///
/// To activate a workspace, or change the mapping between workspaces and monitors, use the
/// <see cref="IButler"/>.
/// </summary>
[Obsolete("Use transforms and pickers to interact with the store instead.")]
public interface IWorkspaceManager : IEnumerable<IWorkspace>, IDisposable
{
	/// <summary>
	/// The active workspace.
	/// </summary>
	[Obsolete("Use the picker PickActiveWorkspace instead.")]
	IWorkspace ActiveWorkspace { get; }

	/// <summary>
	/// Creates the default layout engines to add to a workspace.
	/// </summary>
	[Obsolete("Use the picker PickCreateLeafLayoutEngines or the transform SetCreateLayoutEnginesTransform instead.")]
	Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; }

	/// <summary>
	/// Creates the workspaces from the provided names and <see cref="CreateLayoutEngines"/> function.
	/// Do not call this directly - Whim will call this when it is ready.
	/// </summary>
	void Initialize();

	/// <summary>
	/// Event for when a workspace is added.
	/// </summary>
	[Obsolete("Use the IStore.IWorkspaceSectorEvents.WorkspaceAdded event instead.")]
	event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;

	/// <summary>
	/// Event for when a workspace is removed.
	/// </summary>
	[Obsolete("Use the IStore.IWorkspaceSectorEvents.WorkspaceRemoved event instead.")]
	event EventHandler<WorkspaceRemovedEventArgs>? WorkspaceRemoved;

	/// <summary>
	/// Event for when <see cref="IWorkspace.DoLayout"/> has started.
	/// </summary>
	[Obsolete("Use the IStore.IWorkspaceSectorEvents.WorkspaceLayoutStarted event instead.")]
	event EventHandler<WorkspaceLayoutStartedEventArgs>? WorkspaceLayoutStarted;

	/// <summary>
	/// Event for when <see cref="IWorkspace.DoLayout"/> has completed.
	/// </summary>
	[Obsolete("Use the IStore.IWorkspaceSectorEvents.WorkspaceLayoutCompleted event instead.")]
	event EventHandler<WorkspaceLayoutCompletedEventArgs>? WorkspaceLayoutCompleted;

	/// <summary>
	/// Event for when a workspace's active layout engine has changed.
	/// </summary>
	[Obsolete("Use the IStore.IWorkspaceSectorEvents.ActiveLayoutEngineChanged event instead.")]
	event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	/// <summary>
	/// Event for when a workspace is renamed.
	/// </summary>
	[Obsolete("Use the IStore.IWorkspaceSectorEvents.WorkspaceRenamed event instead.")]
	event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

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
	/// <description>
	/// If <see cref="Initialize"/> has not been called, returns <see langword="null"/>. (i.e., will)
	/// return null while running in the config phase.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// If a workspace cannot be created due to <paramref name="createLayoutEngines"/> being
	/// <see langword="null"/> and <see cref="CreateLayoutEngines"/> returning no layout engines,
	/// returns <see langword="null"/>.
	/// </description>
	/// </item>
	/// <item>
	/// <description>Otherwise, returns the ID of the created workspace.</description>
	/// </item>
	/// </list>
	/// </returns>
	[Obsolete("Use the transform AddWorkspaceTransform instead.")]
	WorkspaceId? Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null);

	/// <summary>
	/// Whether the manager contains the given workspace.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns></returns>
	[Obsolete("Use the picker PickWorkspaces instead.")]
	bool Contains(IWorkspace workspace);

	/// <summary>
	/// Tries to remove the given workspace.
	/// </summary>
	/// <param name="workspace">The workspace to remove.</param>
	[Obsolete("Use the transform RemoveWorkspaceTransform instead.")]
	bool Remove(IWorkspace workspace);

	/// <summary>
	/// Try remove a workspace, by the workspace name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and remove.</param>
	/// <returns><c>true</c> when the workspace has been removed.</returns>
	[Obsolete("Use the transform RemoveWorkspaceByNameTransform instead.")]
	bool Remove(string workspaceName);

	/// <summary>
	/// Tries to get a workspace by the given name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and get.</param>
	[Obsolete("Use the picker PickWorkspaceByName instead.")]
	IWorkspace? TryGet(string workspaceName);

	/// <summary>
	/// Tries to get a workspace by the given name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and get.</param>
	[Obsolete("Use the picker PickWorkspaceByName instead.")]
	IWorkspace? this[string workspaceName] { get; }

	/// <summary>
	/// Adds a proxy layout engine to the workspace manager.
	/// A proxy layout engine is used by plugins to add layout functionality to
	/// all workspaces.
	/// This should be used by <see cref="IPlugin"/>s.
	/// </summary>
	/// <param name="proxyLayoutEngineCreator">The proxy layout engine to add.</param>
	[Obsolete("Use the transform AddProxyLayoutEngineTransform instead.")]
	void AddProxyLayoutEngine(ProxyLayoutEngineCreator proxyLayoutEngineCreator);
}
