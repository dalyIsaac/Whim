using DotNext;

namespace Whim;

public static partial class Pickers
{
	/// <summary>
	/// Get a workspace by its <see cref="WorkspaceId"/>.
	/// </summary>
	/// <param name="workspaceId"></param>
	/// <returns></returns>
	public static Picker<Result<IWorkspace>> PickWorkspaceById(WorkspaceId workspaceId) =>
		new WorkspaceByIdPicker(workspaceId);

	/// <summary>
	/// Get all workspaces.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<ImmutableList<ImmutableWorkspace>> GetAllWorkspaces() =>
		static (IRootSector rootSector) => rootSector.Workspaces.Workspaces;

	/// <summary>
	/// Get the workspace with the provided <paramref name="name"/>.
	/// </summary>
	/// <param name="name"></param>
	public static PurePicker<Result<ImmutableWorkspace>> GetWorkspaceByName(string name) =>
		(IRootSector rootSector) =>
		{
			IWorkspaceSector sector = rootSector.Workspaces;
			ImmutableWorkspace? workspace = sector.Workspaces.Find(w => w.Name == name);

			return workspace is null
				? Result.FromException<ImmutableWorkspace>(new WhimException($"Workspace with name {name} not found"))
				: Result.FromValue(workspace);
		};

	private static Result<TResult> BaseWorkspacePicker<TResult>(
		Guid workspaceId,
		IRootSector rootSector,
		Func<ImmutableWorkspace, TResult> operation
	)
	{
		IWorkspaceSector sector = rootSector.Workspaces;

		int workspaceIdx = -1;
		for (int idx = 0; idx < sector.Workspaces.Count; idx++)
		{
			if (sector.Workspaces[idx].Id == workspaceId)
			{
				workspaceIdx = idx;
				break;
			}
		}

		if (workspaceIdx == -1)
		{
			return Result.FromException<TResult>(StoreExceptions.WorkspaceNotFound());
		}

		return Result.FromValue(operation(sector.Workspaces[workspaceIdx]));
	}

	/// <summary>
	/// Get the active workspace.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<ImmutableWorkspace> GetActiveWorkspace() =>
		(IRootSector rootSector) => rootSector.Workspaces.Workspaces[rootSector.Workspaces.ActiveWorkspaceIndex];

	/// <summary>
	/// Get the id of the active workspace.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<Guid> GetActiveWorkspaceId() =>
		(IRootSector rootSector) => rootSector.Workspaces.Workspaces[rootSector.Workspaces.ActiveWorkspaceIndex].Id;

	/// <summary>
	/// Get the active layout engine in the provided workspace.
	/// </summary>
	/// <param name="workspaceId"></param>
	public static PurePicker<Result<ILayoutEngine>> GetActiveLayoutEngine(Guid workspaceId) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(
				workspaceId,
				rootSector,
				workspace => workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex]
			);

	/// <summary>
	/// Get all the windows in the provided workspace.
	/// </summary>
	/// <param name="workspaceId"></param>
	public static PurePicker<Result<IEnumerable<IWindow>>> GetAllWorkspaceWindows(Guid workspaceId) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(workspaceId, rootSector, workspace => (IEnumerable<IWindow>)workspace.Windows);

	/// <summary>
	/// Get the last focused window in the provided workspace.
	/// </summary>
	/// <param name="workspaceId">The workspace to get the last focused window for.</param>
	public static PurePicker<Result<IWindow?>> GetLastFocusedWindow(Guid workspaceId) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(workspaceId, rootSector, workspace => workspace.LastFocusedWindow);

	/// <summary>
	/// Get the workspace with the provided <paramref name="workspaceId"/>.
	/// </summary>
	/// <param name="workspaceId"></param>
	public static PurePicker<Result<ImmutableWorkspace>> GetWorkspaceById(Guid workspaceId) =>
		(IRootSector rootSector) => BaseWorkspacePicker(workspaceId, rootSector, workspace => workspace);
}

internal record WorkspaceByIdPicker(WorkspaceId WorkspaceId) : Picker<Result<IWorkspace>>
{
	internal override Result<IWorkspace> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		foreach (IWorkspace workspace in ctx.WorkspaceManager)
		{
			if (workspace.Id.Equals(WorkspaceId))
			{
				return Result.FromValue(workspace);
			}
		}

		return Result.FromException<IWorkspace>(StoreExceptions.WorkspaceNotFound(WorkspaceId));
	}
}
