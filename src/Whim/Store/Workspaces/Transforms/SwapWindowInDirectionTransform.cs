namespace Whim;

/// <summary>
/// Swap the <paramref name="Window"/> in the provided <paramref name="Direction"/> for the provided
/// <paramref name="Workspace"/>
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Window"></param>
/// <param name="Direction"></param>
public record SwapWindowInDirectionTransform(ImmutableWorkspace Workspace, IWindow? Window, Direction Direction)
	: BaseWorkspaceWindowTransform(Workspace, Window, true)
{
	/// <summary>
	/// Swap the <paramref name="window"/> in the provided <see cref="Direction"/>
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	protected override ImmutableWorkspace Operation(IWindow window)
	{
		ILayoutEngine oldEngine = Workspace.LayoutEngines[Workspace.ActiveLayoutEngineIndex];
		ILayoutEngine newEngine = oldEngine.SwapWindowInDirection(Direction, window);

		return oldEngine == newEngine
			? Workspace
			: Workspace with
			{
				LayoutEngines = Workspace.LayoutEngines.SetItem(Workspace.ActiveLayoutEngineIndex, newEngine)
			};
	}
}
