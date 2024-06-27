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
	: BaseWorkspaceWindowTransform(
		WorkspaceId,
		WindowHandle,
		DefaultToLastFocusedWindow: false,
		IsWindowRequiredInWorkspace: false,
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
		// If the window is already in the workspace, minimize it in just the active layout engine.
		// If it isn't, then we assume it was provided during startup and minimize it in all layouts.
		if (workspace.WindowPositions.ContainsKey(window.Handle))
		{
			ILayoutEngine activeLayoutEngine = workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex];
			return workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(
					workspace.ActiveLayoutEngineIndex,
					activeLayoutEngine.MinimizeWindowStart(window)
				)
			};
		}

		workspace = workspace with
		{
			WindowPositions = workspace.WindowPositions.SetItem(window.Handle, new WindowPosition())
		};

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
