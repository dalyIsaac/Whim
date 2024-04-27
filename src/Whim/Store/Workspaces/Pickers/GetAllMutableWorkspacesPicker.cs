using System.Collections.Immutable;

namespace Whim;

internal record GetAllMutableWorkspacesPicker() : Picker<ImmutableList<IWorkspace>>
{
	internal override ImmutableList<IWorkspace> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		IRootSector rootSelector
	)
	=> rootSelector.Workspaces.MutableWorkspaces;
}
