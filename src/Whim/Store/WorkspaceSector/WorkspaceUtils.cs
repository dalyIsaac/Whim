namespace Whim;

/// <summary>
/// Utilities for working with the updated <see cref="IWorkspace"/> immutable type.
/// </summary>
public static class WorkspaceUtils
{
	internal static WorkspaceId OrActiveWorkspace(this WorkspaceId WorkspaceId, IContext ctx) =>
		WorkspaceId == default ? ctx.Store.Pick(PickActiveWorkspaceId()) : WorkspaceId;

	/// <summary>
	/// Set the active layout engine in the workspace.
	/// This also sets the <see cref="Workspace.ActiveLayoutEngineIndex"/>.
	/// </summary>
	/// <param name="sector"></param>
	/// <param name="workspace"></param>
	/// <param name="layoutEngineIdx"></param>
	internal static Workspace SetActiveLayoutEngine(WorkspaceSector sector, Workspace workspace, int layoutEngineIdx)
	{
		int previousLayoutEngineIdx = workspace.ActiveLayoutEngineIndex;
		if (previousLayoutEngineIdx == layoutEngineIdx)
		{
			return workspace;
		}

		workspace = workspace with { ActiveLayoutEngineIndex = layoutEngineIdx };

		sector.QueueEvent(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = workspace,
				PreviousLayoutEngine = workspace.LayoutEngines[previousLayoutEngineIdx],
				CurrentLayoutEngine = workspace.LayoutEngines[layoutEngineIdx],
			}
		);

		sector.WindowHandleToFocus = workspace.LastFocusedWindowHandle;

		return workspace;
	}

	/// <summary>
	/// Returns the window to process. If the window is null, the last focused window is used.
	/// If the given window is not null, it is checked that it is in the workspace.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="workspace"></param>
	/// <param name="windowHandle"></param>
	/// <param name="defaultToLastFocusedWindow">
	/// If <paramref name="windowHandle"/> is <c>null</c>, try to use the last focused window.
	/// </param>
	/// <param name="isWindowRequiredInWorkspace">
	/// When <see langword="true"/>, the window must be in the workspace.
	/// </param>
	/// <returns></returns>
	internal static Result<IWindow> GetValidWorkspaceWindow(
		IContext ctx,
		Workspace workspace,
		HWND windowHandle,
		bool defaultToLastFocusedWindow,
		bool isWindowRequiredInWorkspace
	)
	{
		if (windowHandle.IsNull)
		{
			if (!defaultToLastFocusedWindow)
			{
				return Result.FromError<IWindow>(new WhimError("No window provided"));
			}

			windowHandle = workspace.LastFocusedWindowHandle;
		}

		if (windowHandle.IsNull)
		{
			return Result.FromError<IWindow>(new WhimError("No windows in workspace"));
		}

		if (isWindowRequiredInWorkspace && !workspace.WindowPositions.ContainsKey(windowHandle))
		{
			return Result.FromError<IWindow>(new WhimError("Window not in workspace"));
		}

		return ctx.Store.Pick(PickWindowByHandle(windowHandle));
	}

	/// <summary>
	/// Get the active layout engine in the workspace.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns></returns>
	public static ILayoutEngine GetActiveLayoutEngine(this IWorkspace workspace) =>
		workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex];
}
