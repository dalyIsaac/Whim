namespace Whim;

/// <summary>
/// Remove the <paramref name="Window"/> from the provided <paramref name="Workspace"/>
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Window"></param>
public record RemoveWindowFromWorkspaceTransform(ImmutableWorkspace Workspace, IWindow Window)
	: BaseWorkspaceWindowTransform(Workspace, Window, false)
{
	/// <summary>
	/// Remove the <see cref="Window"/> from the provided <see cref="Workspace"/>
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	protected override ImmutableWorkspace Operation(IWindow window)
	{
		ImmutableWorkspace workspace = Workspace with { Windows = Workspace.Windows.Remove(window) };

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
