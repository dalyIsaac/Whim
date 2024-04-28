using DotNext;

namespace Whim;

internal static class WorkspaceUtils
{
	public static ImmutableWorkspace SetActiveLayoutEngine(
		WorkspaceSector sector,
		ImmutableWorkspace workspace,
		int layoutEngineIdx
	)
	{
		int previousLayoutEngineIdx = workspace.ActiveLayoutEngineIndex;
		workspace = workspace with { ActiveLayoutEngineIndex = layoutEngineIdx };

		sector.QueueEvent(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = workspace,
				PreviousLayoutEngine = workspace.LayoutEngines[previousLayoutEngineIdx],
				CurrentLayoutEngine = workspace.LayoutEngines[layoutEngineIdx]
			}
		);

		return workspace;
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
}
