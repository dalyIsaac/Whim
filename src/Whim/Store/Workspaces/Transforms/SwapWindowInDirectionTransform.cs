using System;

namespace Whim;

/// <summary>
/// Swap the <paramref name="Window"/> in the provided <paramref name="Direction"/> for the workspace
/// with the given <paramref name="WorkspaceId"/>
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
/// <param name="Direction"></param>
public record SwapWindowInDirectionTransform(Guid WorkspaceId, IWindow? Window, Direction Direction)
	: BaseWorkspaceWindowTransform(WorkspaceId, Window, true)
{
	/// <inheritdoc/>
	protected override ImmutableWorkspace Operation(ImmutableWorkspace workspace, IWindow window)
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