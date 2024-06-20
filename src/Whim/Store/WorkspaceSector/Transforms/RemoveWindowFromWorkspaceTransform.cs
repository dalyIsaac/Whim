using System.Collections.Immutable;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Remove the window for the given <paramref name="Window"/> from the workspace with the
/// given <paramref name="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
internal record RemoveWindowFromWorkspaceTransform(WorkspaceId WorkspaceId, IWindow Window)
	: BaseWorkspaceTransform(WorkspaceId, SkipDoLayout: false)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		ImmutableDictionary<HWND, WindowPosition> updatedPositions = workspace.WindowPositions.Remove(Window.Handle);
		if (updatedPositions == workspace.WindowPositions)
		{
			return workspace;
		}

		workspace = workspace with { WindowPositions = updatedPositions };

		workspace = ResetLastFocusedWindow(workspace, Window);
		workspace = RemoveWindowFromLayoutEngines(workspace, Window);
		return workspace;
	}

	/// <summary>
	/// Reset the last focused window if the removed window was the last focused window.
	/// </summary>
	/// <param name="workspace"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	private static Workspace ResetLastFocusedWindow(Workspace workspace, IWindow window)
	{
		if (!window.Handle.Equals(workspace.LastFocusedWindowHandle))
		{
			return workspace;
		}

		// Find the next window to focus.
		foreach ((HWND handle, WindowPosition pos) in workspace.WindowPositions)
		{
			if (handle.Equals(window.Handle))
			{
				continue;
			}

			if (pos.WindowSize == WindowSize.Minimized)
			{
				continue;
			}

			workspace = workspace with { LastFocusedWindowHandle = handle };
			break;
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
		// TODO: Optimize
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
