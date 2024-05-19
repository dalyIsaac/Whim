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
