using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Remove the window for the given <paramref name="WindowHandle"/> from the workspace with the
/// given <paramref name="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="WindowHandle"></param>
public record RemoveWindowFromWorkspaceTransform(WorkspaceId WorkspaceId, HWND WindowHandle)
	: BaseWorkspaceWindowTransform(WorkspaceId, WindowHandle, false)
{
	private protected override Result<Workspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace,
		IWindow window
	)
	{
		workspace = workspace with { WindowHandles = workspace.WindowHandles.Remove(window.Handle) };

		workspace = ResetLastFocusedWindow(internalCtx, workspace, window);
		workspace = RemoveWindowFromLayoutEngines(workspace, window);
		return workspace;
	}

	/// <summary>
	/// Reset the last focused window if the removed window was the last focused window.
	/// </summary>
	/// <param name="internalCtx"></param>
	/// <param name="workspace"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	private static Workspace ResetLastFocusedWindow(IInternalContext internalCtx, Workspace workspace, IWindow window)
	{
		if (!window.Handle.Equals(workspace.LastFocusedWindowHandle))
		{
			return workspace;
		}

		// Find the next window to focus.
		foreach (HWND nextWindowHandle in workspace.WindowHandles)
		{
			if (nextWindowHandle.Equals(window))
			{
				continue;
			}

			if (!nextWindowHandle.IsMinimized(internalCtx))
			{
				workspace = workspace with { LastFocusedWindowHandle = nextWindowHandle };
				break;
			}
		}

		// If there are no other windows, set the last focused window to null.
		if (workspace.LastFocusedWindowHandle.Equals(window.Handle))
		{
			workspace = workspace with { LastFocusedWindowHandle = default };
		}

		return workspace;
	}

	private static Workspace RemoveWindowFromLayoutEngines(Workspace workspace, IWindow window)
	{
		for (int idx = 0; idx < workspace.LayoutEngines.Count; idx++)
		{
			workspace = workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(idx, workspace.LayoutEngines[idx].RemoveWindow(window))
			};
		}

		return workspace;
	}
}
