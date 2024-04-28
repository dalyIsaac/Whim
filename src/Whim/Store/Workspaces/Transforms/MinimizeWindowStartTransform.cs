using System;

namespace Whim;

/// <summary>
/// Called when a window is being minimized - i.e., the window size will become
/// <see cref="WindowSize.Minimized"/>.
///
/// Will minimize a window in the active layout engine.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
internal record MinimizeWindowStartTransform(Guid WorkspaceId, IWindow Window)
	: BaseWorkspaceWindowTransform(WorkspaceId, Window, true)
{
	/// <inheritdoc/>
	protected override ImmutableWorkspace Operation(ImmutableWorkspace workspace, IWindow window)
	{
		// If the window is already in the workspace, minimize it in just the active layout engine.
		// If it isn't, then we assume it was provided during startup and minimize it in all layouts.
		if (workspace.Windows.Contains(window))
		{
			// _layoutEngines[_activeLayoutEngineIndex] = _layoutEngines[_activeLayoutEngineIndex]
			// 	.MinimizeWindowStart(window);
			ILayoutEngine activeLayoutEngine = workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex];
			workspace = workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(
					workspace.ActiveLayoutEngineIndex,
					activeLayoutEngine.MinimizeWindowStart(window)
				)
			};
		}

		workspace = workspace with { Windows = workspace.Windows.Add(window) };

		for (int idx = 0; idx < workspace.LayoutEngines.Count; idx++)
		{
			workspace = workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(
					idx,
					workspace.LayoutEngines[idx].MinimizeWindowStart(window)
				)
			};
		}

		return workspace;
	}
}
