namespace Whim;

/// <summary>
/// Moves the window with <paramref name="WindowHandle"/> to the given <paramref name="TargetWorkspaceId"/>.
/// </summary>
/// <param name="TargetWorkspaceId">
/// The id of the workspace to move the window to.
/// </param>
/// <param name="WindowHandle">
/// The window to move. If <see langword="null"/>, this will default to
/// the focused/active window.
/// </param>
public record MoveWindowToWorkspaceTransform(WorkspaceId TargetWorkspaceId, HWND WindowHandle = default) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		Result<IWorkspace> targetWorkspaceResult = ctx.Store.Pick(PickWorkspaceById(TargetWorkspaceId));
		if (!targetWorkspaceResult.TryGet(out IWorkspace targetWorkspace))
		{
			return Result.FromError<Unit>(targetWorkspaceResult.Error!);
		}

		// Get the window.
		HWND windowHandle = WindowHandle.OrLastFocusedWindow(ctx);
		if (windowHandle == default)
		{
			return Result.FromError<Unit>(StoreErrors.NoValidWindow());
		}

		Result<IWindow> windowResult = ctx.Store.Pick(PickWindowByHandle(windowHandle));
		if (!windowResult.TryGet(out IWindow window))
		{
			return Result.FromError<Unit>(windowResult.Error!);
		}

		Logger.Debug($"Moving window {windowHandle} to workspace {TargetWorkspaceId}");

		// Find the current workspace for the window.
		Result<IWorkspace> oldWorkspaceResult = ctx.Store.Pick(PickWorkspaceByWindow(windowHandle));
		if (!oldWorkspaceResult.TryGet(out IWorkspace oldWorkspace))
		{
			return Result.FromError<Unit>(oldWorkspaceResult.Error!);
		}

		if (oldWorkspace.Id == TargetWorkspaceId)
		{
			Logger.Debug($"Window {windowHandle} is already on workspace {TargetWorkspaceId}");
			return Unit.Result;
		}

		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.SetItem(
			windowHandle,
			TargetWorkspaceId
		);

		ctx.Store.Dispatch(new RemoveWindowFromWorkspaceTransform(oldWorkspace.Id, window) { SkipDoLayout = true });
		ctx.Store.Dispatch(new AddWindowToWorkspaceTransform(TargetWorkspaceId, window) { SkipDoLayout = true });

		// If both workspaces are visible, activate both
		// Otherwise, only layout the new workspace.
		if (
			ctx.Store.Pick(PickMonitorByWorkspace(oldWorkspace.Id)).IsSuccessful
			&& ctx.Store.Pick(PickMonitorByWorkspace(targetWorkspace.Id)).IsSuccessful
		)
		{
			ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(TargetWorkspaceId));
			ctx.Store.Dispatch(new DoWorkspaceLayoutTransform(oldWorkspace.Id));
		}
		else
		{
			ctx.Store.Dispatch(new ActivateWorkspaceTransform(TargetWorkspaceId));
		}

		rootSector.WorkspaceSector.WindowHandleToFocus = window.Handle;

		return Unit.Result;
	}
}
