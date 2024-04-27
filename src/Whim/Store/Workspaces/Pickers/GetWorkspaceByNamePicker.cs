using DotNext;

namespace Whim;

/// <summary>
/// Get the workspace with the provided <paramref name="Name"/>.
/// </summary>
/// <param name="Name"></param>
public record GetWorkspaceByNamePicker(string Name) : Picker<Result<ImmutableWorkspace>>
{
	internal override Result<ImmutableWorkspace> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		IRootSector rootSelector
	)
	{
		IWorkspaceSector sector = rootSelector.Workspaces;
		ImmutableWorkspace? workspace = sector.Workspaces.Find(w => w.Name == Name);

		return workspace is null
			? Result.FromException<ImmutableWorkspace>(new WhimException($"Workspace with name {Name} not found"))
			: Result.FromValue(workspace);
	}
}
