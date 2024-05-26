using System;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Called when a window is being unminimized - i.e., the window size will no longer be
/// <see cref="WindowSize.Minimized"/>.
///
/// Will unminimize a window in the active layout engine.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="WindowHandle"></param>
internal record MinimizeWindowEndTransform(Guid WorkspaceId, HWND WindowHandle)
	: BaseWorkspaceWindowTransform(WorkspaceId, WindowHandle, DefaultToLastFocusedWindow: false, SkipDoLayout: true)
{
	private protected override Result<ImmutableWorkspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceSector sector,
		ImmutableWorkspace workspace,
		IWindow window
	) =>
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
