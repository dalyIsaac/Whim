namespace Whim;

/// <summary>
/// The sector containing workspaces.
/// </summary>
public interface IWorkspaceSector
{
	/// <summary>
	/// The order of the workspaces in the sector.
	/// </summary>
	ImmutableArray<WorkspaceId> WorkspaceOrder { get; }

	/// <summary>
	/// All the workspaces currently tracked by Whim.
	/// </summary>
	ImmutableDictionary<WorkspaceId, Workspace> Workspaces { get; }

	/// <summary>
	/// The IDs of the workspaces that should be laid out in this dispatch.
	/// </summary>
	ImmutableHashSet<WorkspaceId> WorkspacesToLayout { get; }

	/// <summary>
	/// The window handle to focus after all workspaces have been laid out.
	/// </summary>
	HWND WindowHandleToFocus { get; }

	/// <summary>
	/// Creates the default layout engines to add to a workspace.
	/// </summary>
	Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; }

	/// <summary>
	/// A proxy layout ie used by plugins to add layout functionality to all workspaces.
	/// </summary>
	ImmutableList<ProxyLayoutEngineCreator> ProxyLayoutEngineCreators { get; }
}
