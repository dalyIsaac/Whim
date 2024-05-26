using System;
using DotNext;

namespace Whim;

/// <summary>
/// Focus the <paramref name="Window"/> in the workspace with the given <paramref name="WorkspaceId"/>
/// in the provided <paramref name="Direction"/>.
///
/// Returns whether the active layout engine changed.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
/// <param name="Direction"></param>
public record FocusWindowInDirectionTransform(Guid WorkspaceId, IWindow? Window, Direction Direction)
	: BaseWorkspaceWindowTransform(WorkspaceId, Window, true)
{
	private protected override Result<ImmutableWorkspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceSector sector,
		ImmutableWorkspace workspace,
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
