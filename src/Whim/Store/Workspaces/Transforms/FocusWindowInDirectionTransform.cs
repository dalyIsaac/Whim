namespace Whim;

/// <summary>
/// Focus the <paramref name="Window"/> in the provided <paramref name="Workspace"/>
/// in the provided <paramref name="Direction"/>.
///
/// Returns whether the active layout engine changed.
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Window"></param>
/// <param name="Direction"></param>
public record FocusWindowInDirectionTransform(ImmutableWorkspace Workspace, IWindow? Window, Direction Direction)
	: BaseWorkspaceWindowTransform(Workspace, Window, true)
{
	/// <summary>
	/// Focus the <paramref name="window"/> in the provided workspace.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	protected override ImmutableWorkspace Operation(IWindow window)
	{
		ILayoutEngine layoutEngine = Workspace.LayoutEngines[Workspace.ActiveLayoutEngineIndex];
		ILayoutEngine newLayoutEngine = layoutEngine.FocusWindowInDirection(Direction, window);

		if (newLayoutEngine == layoutEngine)
		{
			Logger.Debug("Window already in focus");
			return Workspace;
		}

		return Workspace with
		{
			LayoutEngines = Workspace.LayoutEngines.SetItem(Workspace.ActiveLayoutEngineIndex, newLayoutEngine)
		};
	}
}
