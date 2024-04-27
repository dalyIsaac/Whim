using System.Collections.Generic;
using DotNext;

namespace Whim;

/// <summary>
/// Get all the windows in the provided workspace.
/// </summary>
/// <param name="Workspace"></param>
public record GetAllWorkspaceWindowsPicker(ImmutableWorkspace Workspace) : Picker<Result<IEnumerable<IWindow>>>
{
	internal override Result<IEnumerable<IWindow>> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		IRootSector rootSelector
	)
	{
		if (!rootSelector.Workspaces.Workspaces.Contains(Workspace))
		{
			return Result.FromException<IEnumerable<IWindow>>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		return Result.FromValue<IEnumerable<IWindow>>(Workspace.Windows);
	}
}
