using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal static class WorkspaceUtils
{
	public static WorkspaceId OrActiveWorkspace(this WorkspaceId WorkspaceId, IContext ctx) =>
		WorkspaceId == default ? ctx.WorkspaceManager.ActiveWorkspace.Id : WorkspaceId;

	public static Workspace SetActiveLayoutEngine(WorkspaceSector sector, Workspace workspace, int layoutEngineIdx)
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
	/// <param name="ctx"></param>
	/// <param name="workspace"></param>
	/// <param name="windowHandle"></param>
	/// <param name="defaultToLastFocusedWindow"></param>
	/// <returns></returns>
	public static Result<IWindow> GetValidWorkspaceWindow(
		IContext ctx,
		Workspace workspace,
		HWND windowHandle,
		bool defaultToLastFocusedWindow
	)
	{
		if (windowHandle.IsNull)
		{
			if (!defaultToLastFocusedWindow)
			{
				return Result.FromException<IWindow>(new WhimException("No window provided"));
			}

			windowHandle = workspace.LastFocusedWindowHandle;
		}

		if (windowHandle.IsNull)
		{
			return Result.FromException<IWindow>(new WhimException("No windows in workspace"));
		}

		if (!workspace.WindowPositions.ContainsKey(windowHandle))
		{
			return Result.FromException<IWindow>(new WhimException("Window not in workspace"));
		}

		return ctx.Store.Pick(Pickers.PickWindowByHandle(windowHandle));
	}
}
