using System;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Swap the <paramref name="WindowHandle"/> in the provided <paramref name="Direction"/> for the workspace
/// with the given <paramref name="WorkspaceId"/>
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="WindowHandle"></param>
/// <param name="Direction"></param>
public record SwapWindowInDirectionTransform(Guid WorkspaceId, HWND WindowHandle, Direction Direction)
	: BaseWorkspaceWindowTransform(WorkspaceId, WindowHandle, true)
{
	private protected override Result<Workspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace,
		IWindow window
	)
	{
		ILayoutEngine oldEngine = workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex];
		ILayoutEngine newEngine = oldEngine.SwapWindowInDirection(Direction, window);

		return oldEngine == newEngine
			? workspace
			: workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(workspace.ActiveLayoutEngineIndex, newEngine)
			};
	}
}
