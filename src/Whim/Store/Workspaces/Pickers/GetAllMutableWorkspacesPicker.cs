using System.Collections.Immutable;

namespace Whim;

internal record GetAllMutableWorkspacesPicker() : Picker<ImmutableList<IWorkspace>>
{
	internal override ImmutableList<IWorkspace> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;
		return slice.MutableWorkspaces;
	}
}
