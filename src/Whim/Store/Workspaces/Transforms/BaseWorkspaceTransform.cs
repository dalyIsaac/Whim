using System;
using DotNext;

namespace Whim;

/// <summary>
/// Base transform for a window operation in a given workspace.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
/// <param name="DefaultToLastFocusedWindow">
/// If <paramref name="Window"/> is <c>null</c>, try to use the last focused window.
/// </param>
/// <param name="SkipDoLayout">
/// If <c>true</c>, do not perform a workspace layout.
/// </param>
public abstract record BaseWorkspaceWindowTransform(
	Guid WorkspaceId,
	IWindow? Window,
	bool DefaultToLastFocusedWindow,
	bool SkipDoLayout = false
) : Transform<bool>
{
	/// <summary>
	/// The operation to execute.
	/// </summary>
	/// <param name="workspace">
	/// The workspace.
	/// </param>
	/// <param name="window">
	/// A window in the workspace.
	/// </param>
	/// <returns>
	/// The updated workspace.
	/// </returns>
	protected abstract ImmutableWorkspace Operation(ImmutableWorkspace workspace, IWindow window);

	internal override Result<bool> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		WorkspaceSector sector = rootSector.Workspaces;

		int workspaceIdx = -1;
		for (int idx = 0; idx < sector.Workspaces.Count; idx++)
		{
			if (sector.Workspaces[idx].Id == WorkspaceId)
			{
				workspaceIdx = idx;
				break;
			}
		}

		if (workspaceIdx == -1)
		{
			return Result.FromException<bool>(StoreExceptions.WorkspaceNotFound());
		}

		ImmutableWorkspace workspace = sector.Workspaces[workspaceIdx];

		Result<IWindow> result = WorkspaceUtils.GetValidWorkspaceWindow(workspace, Window, DefaultToLastFocusedWindow);
		if (!result.TryGet(out IWindow validWindow))
		{
			return Result.FromException<bool>(result.Error!);
		}

		ImmutableWorkspace newWorkspace = Operation(workspace, validWindow);

		if (newWorkspace == workspace)
		{
			return Result.FromValue(false);
		}

		sector.Workspaces = sector.Workspaces.SetItem(workspaceIdx, newWorkspace);

		if (SkipDoLayout == false)
		{
			sector.WorkspacesToLayout.Add(workspace.Id);
		}

		return Result.FromValue(true);
	}
}
