using DotNext;

namespace Whim;

internal static class WorkspaceUtils
{
	public static void SetActiveLayoutEngine(WorkspaceSector sector, int workspaceIdx, int layoutEngineIdx)
	{
		ImmutableWorkspace workspace = sector.Workspaces[workspaceIdx];

		int previousLayoutEngineIdx = workspace.ActiveLayoutEngineIndex;
		sector.Workspaces = sector.Workspaces.SetItem(
			workspaceIdx,
			workspace with
			{
				ActiveLayoutEngineIndex = layoutEngineIdx
			}
		);

		sector.WorkspacesToLayout.Add(workspace.Id);
		sector.QueueEvent(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = workspace,
				PreviousLayoutEngine = workspace.LayoutEngines[previousLayoutEngineIdx],
				CurrentLayoutEngine = workspace.LayoutEngines[layoutEngineIdx]
			}
		);
	}

	/// <summary>
	/// Returns the window to process. If the window is null, the last focused window is used.
	/// If the given window is not null, it is checked that it is in the workspace.
	/// </summary>
	/// <param name="workspace"></param>
	/// <param name="window"></param>
	/// <param name="defaultToLastFocusedWindow"></param>
	/// <returns></returns>
	public static Result<IWindow> GetValidWorkspaceWindow(
		ImmutableWorkspace workspace,
		IWindow? window,
		bool defaultToLastFocusedWindow
	)
	{
		if (window == null)
		{
			if (!defaultToLastFocusedWindow)
			{
				return Result.FromException<IWindow>(new WhimException("No window provided"));
			}

			window = workspace.LastFocusedWindow;
		}

		if (window == null)
		{
			return Result.FromException<IWindow>(new WhimException("No windows in workspace"));
		}

		if (!workspace.Windows.Contains(window))
		{
			return Result.FromException<IWindow>(new WhimException("Window not in workspace"));
		}

		return Result.FromValue(window);
	}

	public static WhimException WorkspaceDoesNotExist() =>
		new("Provided workspace did not exist in collection, could not remove");
}
