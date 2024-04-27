using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// Get all workspaces.
/// </summary>
public record GetAllWorkspacesPicker() : Picker<ImmutableList<ImmutableWorkspace>>
{
	internal override ImmutableList<ImmutableWorkspace> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		IRootSector rootSelector
	)
	=> rootSelector.Workspaces.Workspaces;
}
