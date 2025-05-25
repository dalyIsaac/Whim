namespace Whim;

internal record WindowAddedTransform(HWND Handle, RouterOptions? CustomRouterOptions = null) : Transform<IWindow>()
{
	internal override Result<IWindow> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		Result<IWindow> windowResult = GetWindow(ctx, internalCtx);
		if (!windowResult.TryGet(out IWindow window))
		{
			return windowResult;
		}

		Logger.Debug($"Adding window {window}");
		UpdateWindowSector(mutableRootSector, window);
		UpdateMapSector(ctx, internalCtx, mutableRootSector, window);

		return windowResult;
	}

	private Result<IWindow> GetWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Filter the handle.		if (internalCtx.CoreNativeManager.IsSplashScreen(Handle))
		{
			return new Result<IWindow>(
				new WhimError($"Window {Handle} is a splash screen, ignoring")
			);
		}
		if (internalCtx.CoreNativeManager.IsCloakedWindow(Handle))
		{
			return new Result<IWindow>(
				new WhimError($"Window {Handle} is cloaked, ignoring")
			);
		}

		if (!internalCtx.CoreNativeManager.IsStandardWindow(Handle))
		{
			return new Result<IWindow>(
				new WhimError($"Window {Handle} is not a standard window, ignoring")
			);
		}

		if (!internalCtx.CoreNativeManager.HasNoVisibleOwner(Handle))
		{
			return new Result<IWindow>(
				new WhimError($"Window {Handle} is a tooltip, ignoring")
			);
		}

		Result<IWindow> windowResult = Window.CreateWindow(ctx, internalCtx, Handle);
		if (!windowResult.TryGet(out IWindow window))
		{
			return windowResult;
		}

		// Filter the window.
		if (ctx.FilterManager.ShouldBeIgnored(window))
		{
			return Result.FromError<IWindow>(new WhimError("Window was ignored by filter"));
		}

		return Result.FromValue(window);
	}

	private static void UpdateWindowSector(MutableRootSector mutableRootSector, IWindow window)
	{
		WindowSector sector = mutableRootSector.WindowSector;
		sector.Windows = sector.Windows.Add(window.Handle, window);
		sector.QueueEvent(new WindowAddedEventArgs() { Window = window });
	}

	/// <summary>
	/// Updates the workspace for the added window.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="rootSector"></param>
	/// <param name="window"></param>
	private void UpdateMapSector(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		IWindow window
	)
	{
		MapSector mapSector = rootSector.MapSector;
		WorkspaceSector workspaceSector = rootSector.WorkspaceSector;

		IWorkspace? workspace = TryGetWorkspaceFromRouter(ctx, rootSector, window);

		// Check the workspace exists. If it doesn't, clear the workspace.
		if (workspace != null && !rootSector.WorkspaceSector.Workspaces.ContainsKey(workspace.Id))
		{
			Logger.Error($"Workspace {workspace} was not found");
			workspace = null;
		}

		// If the workspace is still null, try to find a workspace for the window's monitor.
		workspace ??= TryGetWorkspaceFromWindow(ctx, internalCtx, window);

		// If that fails too, route the window to the active workspace.
		workspace ??= PickMutableActiveWorkspace(rootSector);

		// Update the window workspace mapping.
		mapSector.WindowWorkspaceMap = mapSector.WindowWorkspaceMap.SetItem(window.Handle, workspace.Id);

		if (window.IsMinimized)
		{
			ctx.Store.Dispatch(new MinimizeWindowStartTransform(workspace.Id, window.Handle));
		}
		else
		{
			ctx.Store.Dispatch(new AddWindowToWorkspaceTransform(workspace.Id, window));
		}

		mapSector.QueueEvent(RouteEventArgs.WindowAdded(window, workspace));

		workspaceSector.WorkspacesToLayout = workspaceSector.WorkspacesToLayout.Add(workspace.Id);
		rootSector.WorkspaceSector.WindowHandleToFocus = window.Handle;
	}

	/// <summary>
	/// Try to get the workspace based on routing preferences.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="rootSector"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	private IWorkspace? TryGetWorkspaceFromRouter(IContext ctx, MutableRootSector rootSector, IWindow window)
	{
		IWorkspace? workspace = null;

		RouterOptions routerOptions = CustomRouterOptions ?? ctx.RouterManager.RouterOptions;

		// RouteWindow takes precedence over RouterOptions.
		if (ctx.RouterManager.RouteWindow(window) is IWorkspace routedWorkspace)
		{
			workspace = routedWorkspace;
		}
		else if (routerOptions == RouterOptions.RouteToActiveWorkspace)
		{
			workspace = PickMutableActiveWorkspace(rootSector);
		}
		else if (routerOptions == RouterOptions.RouteToLastTrackedActiveWorkspace)
		{
			workspace = ctx
				.Store.Pick(PickWorkspaceByMonitor(rootSector.MonitorSector.LastWhimActiveMonitorHandle))
				.Value!;
		}

		return workspace;
	}

	/// <summary>
	/// Try get the workspace based on the active window.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	private static IWorkspace? TryGetWorkspaceFromWindow(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		HMONITOR hmonitor = internalCtx.CoreNativeManager.MonitorFromWindow(
			window.Handle,
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
		);

		return ctx.Store.Pick(PickWorkspaceByMonitor(hmonitor)).ValueOrDefault;
	}
}
