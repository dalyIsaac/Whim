using DotNext;

namespace Whim;

/// <summary>
/// Triggers a layout (sets all the window positions) for the workspace specified by <see cref="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
public record DoWorkspaceLayoutTransform(WorkspaceId WorkspaceId) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		WorkspaceSector sector = rootSector.WorkspaceSector;
		sector.WorkspacesToLayout = sector.WorkspacesToLayout.Add(WorkspaceId);
		return Unit.Result;
	}
}
