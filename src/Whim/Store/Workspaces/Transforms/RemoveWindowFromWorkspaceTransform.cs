using DotNext;

namespace Whim;

/// <summary>
/// Remove the <paramref name="Window"/> from the provided <paramref name="Workspace"/>
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Window"></param>
public record RemoveWindowFromWorkspaceTransform(ImmutableWorkspace Workspace, IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSector sector = mutableRootSector.Workspaces;

		int workspaceIdx = sector.Workspaces.IndexOf(Workspace);
		if (workspaceIdx == -1)
		{
			return Result.FromException<Empty>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		ImmutableWorkspace workspace = Workspace;

		if (!workspace.Windows.Contains(Window))
		{
			return Result.FromException<Empty>(new WhimException("Window not in workspace"));
		}

		// Remove the window from the workspace.
		workspace = workspace with
		{
			Windows = workspace.Windows.Remove(Window)
		};

		workspace = ResetLastFocusedWindow(workspace);
		workspace = RemoveWindowFromLayoutEngines(workspace);

		// Save the workspace.
		sector.Workspaces = sector.Workspaces.SetItem(workspaceIdx, workspace);

		return Empty.Result;
	}

	/// <summary>
	/// Reset the last focused window if the removed window was the last focused window.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns></returns>
	private ImmutableWorkspace ResetLastFocusedWindow(ImmutableWorkspace workspace)
	{
		if (!Window.Equals(workspace.LastFocusedWindow))
		{
			return workspace;
		}

		// Find the next window to focus.
		foreach (IWindow nextWindow in workspace.Windows)
		{
			if (nextWindow.Equals(Window))
			{
				continue;
			}

			if (!nextWindow.IsMinimized)
			{
				workspace = workspace with { LastFocusedWindow = nextWindow };
				break;
			}
		}

		// If there are no other windows, set the last focused window to null.
		if (workspace.LastFocusedWindow.Equals(Window))
		{
			workspace = workspace with { LastFocusedWindow = null };
		}

		return workspace;
	}

	private ImmutableWorkspace RemoveWindowFromLayoutEngines(ImmutableWorkspace workspace)
	{
		for (int idx = 0; idx < workspace.LayoutEngines.Count; idx++)
		{
			workspace = workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(idx, workspace.LayoutEngines[idx].RemoveWindow(Window))
			};
		}

		return workspace;
	}
}
