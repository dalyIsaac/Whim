namespace Whim;

/// <summary>
/// Get the active layout engine in the provided workspace.
/// </summary>
/// <param name="WorkspacePredicate"></param>
public record GetActiveLayoutEnginePicker(Pred<ImmutableWorkspace> WorkspacePredicate) : Picker<ILayoutEngine>
{
	internal override ILayoutEngine Execute(IContext ctx, IInternalContext internalCtx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;

		int workspaceIdx = slice.Workspaces.GetMatchingIndex(WorkspacePredicate);
		if (workspaceIdx == -1)
		{
			throw WorkspaceUtils.WorkspaceDoesNotExist();
		}

		ImmutableWorkspace workspace = slice.Workspaces[workspaceIdx];
		int layoutEngineIdx = workspace.ActiveLayoutEngineIndex;

		return workspace.LayoutEngines[layoutEngineIdx];
	}
}
