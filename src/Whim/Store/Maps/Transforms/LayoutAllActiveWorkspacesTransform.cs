using DotNext;

namespace Whim;

/// <summary>
/// Triggers all active workspaces to update their layout.
/// Active workspaces are those that are visible on a monitor.
/// </summary>
public record LayoutAllActiveWorkspacesTransform() : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		foreach (IWorkspace workspace in ctx.Store.Pick(MapPickers.GetAllActiveWorkspaces))
		{
			workspace.DoLayout();
		}

		return Empty.Result;
	}
}
