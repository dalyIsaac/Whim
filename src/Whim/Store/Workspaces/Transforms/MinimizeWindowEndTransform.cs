using System;

namespace Whim;

/// <summary>
/// Called when a window is being unminimized - i.e., the window size will no longer be
/// <see cref="WindowSize.Minimized"/>.
///
/// Will unminimize a window in the active layout engine.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
internal record MinimizeWindowEndTransform(Guid WorkspaceId, IWindow Window)
	: BaseWorkspaceWindowTransform(WorkspaceId, Window, DefaultToLastFocusedWindow: false, SkipDoLayout: true)
{
	protected override ImmutableWorkspace Operation(ImmutableWorkspace workspace, IWindow window) =>
		workspace with
		{
			Windows = workspace.Windows.Add(window),

			// Restore in just the active layout engine. MinimizeWindowEnd is not called as part of
			// Whim starting up.
			LayoutEngines = workspace.LayoutEngines.SetItem(
				workspace.ActiveLayoutEngineIndex,
				workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex].MinimizeWindowEnd(window)
			)
		};
}
