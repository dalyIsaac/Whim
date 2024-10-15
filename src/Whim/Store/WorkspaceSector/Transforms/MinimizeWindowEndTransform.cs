namespace Whim;

/// <summary>
/// Called when a window is being un-minimized - i.e., the window size will no longer be
/// <see cref="WindowSize.Minimized"/>.
///
/// Will un-minimize a window in the active layout engine.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="WindowHandle"></param>
internal record MinimizeWindowEndTransform(WorkspaceId WorkspaceId, HWND WindowHandle)
	: BaseWorkspaceWindowTransform(
		WorkspaceId,
		WindowHandle,
		DefaultToLastFocusedWindow: false,
		IsWindowRequiredInWorkspace: true,
		SkipDoLayout: true
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
		ILayoutEngine layoutEngine = workspace.GetActiveLayoutEngine();
		ILayoutEngine newLayoutEngine = workspace.GetActiveLayoutEngine().MinimizeWindowEnd(window);

		return (newLayoutEngine == layoutEngine)
			? workspace
			: workspace with
			{
				WindowPositions = workspace.WindowPositions.SetItem(window.Handle, new WindowPosition()),

				// Restore in just the active layout engine. MinimizeWindowEnd is not called as part of
				// Whim starting up.
				LayoutEngines = workspace.LayoutEngines.SetItem(workspace.ActiveLayoutEngineIndex, newLayoutEngine),
			};
	}
}
