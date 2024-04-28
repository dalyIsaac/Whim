using System;

namespace Whim;

/// <summary>
/// Remove the <paramref name="Window"/> from the workspace with the given <paramref name="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
public record RemoveWindowFromWorkspaceTransform(Guid WorkspaceId, IWindow Window)
	: BaseWorkspaceWindowTransform(WorkspaceId, Window, false)
{
	/// <inheritdoc />
	protected override ImmutableWorkspace Operation(ImmutableWorkspace workspace, IWindow window)
	{
		workspace = workspace with { Windows = workspace.Windows.Remove(window) };

		workspace = ResetLastFocusedWindow(workspace, window);
		workspace = RemoveWindowFromLayoutEngines(workspace, window);
		return workspace;
	}

	/// <summary>
	/// Reset the last focused window if the removed window was the last focused window.
	/// </summary>
	/// <param name="workspace"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	private static ImmutableWorkspace ResetLastFocusedWindow(ImmutableWorkspace workspace, IWindow window)
	{
		if (!window.Equals(workspace.LastFocusedWindow))
		{
			return workspace;
		}

		// Find the next window to focus.
		foreach (IWindow nextWindow in workspace.Windows)
		{
			if (nextWindow.Equals(window))
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
		if (workspace.LastFocusedWindow.Equals(window))
		{
			workspace = workspace with { LastFocusedWindow = null };
		}

		return workspace;
	}

	private static ImmutableWorkspace RemoveWindowFromLayoutEngines(ImmutableWorkspace workspace, IWindow window)
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
