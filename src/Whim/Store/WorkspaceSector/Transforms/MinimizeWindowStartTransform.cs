using System;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Called when a window is being minimized - i.e., the window size will become
/// <see cref="WindowSize.Minimized"/>.
///
/// Will minimize a window in the active layout engine.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="WindowHandle"></param>
internal record MinimizeWindowStartTransform(Guid WorkspaceId, HWND WindowHandle)
	: BaseWorkspaceWindowTransform(WorkspaceId, WindowHandle, DefaultToLastFocusedWindow: false, SkipDoLayout: true)
{
	private protected override Result<ImmutableWorkspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		ImmutableWorkspace workspace,
		IWindow window
	)
	{
		// If the window is already in the workspace, minimize it in just the active layout engine.
		// If it isn't, then we assume it was provided during startup and minimize it in all layouts.
		if (workspace.WindowHandles.Contains(window.Handle))
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

		workspace = workspace with { WindowHandles = workspace.WindowHandles.Add(window.Handle) };

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
