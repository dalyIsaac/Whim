using System;
using DotNext;

namespace Whim;

/// <summary>
/// Get the workspace with the provided <paramref name="Id"/>.
/// </summary>
/// <param name="Id"></param>
public record GetWorkspaceByIdPicker(Guid Id) : Picker<Result<ImmutableWorkspace>>
{
	internal override Result<ImmutableWorkspace> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		IRootSector rootSelector
	)
	{
		IWorkspaceSector sector = rootSelector.Workspaces;
		ImmutableWorkspace? workspace = sector.Workspaces.Find(w => w.Id == Id);

		return workspace is null
			? Result.FromException<ImmutableWorkspace>(new WhimException($"Workspace with id {Id} not found"))
			: Result.FromValue(workspace);
	}
}
