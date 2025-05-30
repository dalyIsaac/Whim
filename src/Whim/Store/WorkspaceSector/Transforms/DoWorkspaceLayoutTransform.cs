namespace Whim;

/// <summary>
/// Queues a layout (sets all the window positions) for the workspace specified by <see cref="WorkspaceId"/>.
/// This requires that the workspace has a matching monitor in the <see cref="IMonitorSector"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <example>
/// <code>
/// context.Store.Dispatch(new DoWorkspaceLayoutTransform(workspaceId));
/// </code>
/// </example>
public record DoWorkspaceLayoutTransform(WorkspaceId WorkspaceId) : WhimTransform
{
	internal override WhimResult<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		WorkspaceSector sector = rootSector.WorkspaceSector;
		sector.WorkspacesToLayout = sector.WorkspacesToLayout.Add(WorkspaceId);
		return Unit.Result;
	}
}
