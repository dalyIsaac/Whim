using DotNext;

namespace Whim;

/// <summary>
/// Get the workspace with the provided <paramref name="Id"/>.
/// </summary>
/// <param name="Id"></param>
public record GetWorkspaceByIdPicker(uint Id) : Picker<Result<ImmutableWorkspace>>
{
	internal override Result<ImmutableWorkspace> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;
		ImmutableWorkspace? workspace = slice.Workspaces.Find(w => w.Id == Id);

		return workspace is null
			? Result.FromException<ImmutableWorkspace>(new WhimException($"Workspace with id {Id} not found"))
			: Result.FromValue(workspace);
	}
}
