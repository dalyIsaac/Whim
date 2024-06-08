using System;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Focus the <paramref name="WindowHandle"/> in the workspace with the given <paramref name="WorkspaceId"/>
/// in the provided <paramref name="Direction"/>.
///
/// Returns whether the active layout engine changed.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="WindowHandle"></param>
/// <param name="Direction"></param>
public record FocusWindowInDirectionTransform(Guid WorkspaceId, HWND WindowHandle, Direction Direction)
	: BaseWorkspaceWindowTransform(
		WorkspaceId,
		WindowHandle,
		DefaultToLastFocusedWindow: true,
		IsWindowRequiredInWorkspace: true,
		SkipDoLayout: false
	)
{
	private protected override Result<Workspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace,
		IWindow window
	)
	{
		ILayoutEngine layoutEngine = workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex];
		ILayoutEngine newLayoutEngine = layoutEngine.FocusWindowInDirection(Direction, window);

		if (newLayoutEngine == layoutEngine)
		{
			Logger.Debug("Window already in focus");
			return workspace;
		}

		return workspace with
		{
			LayoutEngines = workspace.LayoutEngines.SetItem(workspace.ActiveLayoutEngineIndex, newLayoutEngine)
		};
	}
}
