namespace Whim;

/// <summary>
/// Triggers all active workspaces to update their layout.
/// Active workspaces are those that are visible on a monitor.
/// </summary>
public record LayoutAllActiveWorkspacesTransform() : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{		if (ctx.Store.IsDisposing)
		{
			return new(new WhimError("Whim is shutting down"));
		}

		foreach (IWorkspace workspace in ctx.Store.Pick(PickAllActiveWorkspaces()))
		{
			rootSector.WorkspaceSector.WorkspacesToLayout = rootSector.WorkspaceSector.WorkspacesToLayout.Add(
				workspace.Id
			);
		}

		return Unit.Result;
	}
}
