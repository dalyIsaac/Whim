using System.Collections.Immutable;
using DotNext;

namespace Whim;

/// <summary>
/// Get all workspaces.
/// </summary>
public record GetAllWorkspacesPicker() : Picker<ImmutableList<ImmutableWorkspace>>
{
	internal override ImmutableList<ImmutableWorkspace> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;
		return slice.Workspaces;
	}
}
