using DotNext;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal record WindowAddedTransform(HWND Handle) : Transform<IWindow>()
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

		UpdateWindowSector(mutableRootSector, window);
		UpdateMapSector(ctx, internalCtx, mutableRootSector, window);

		return Result.FromValue(window);
	}

	private Result<IWindow> GetWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Filter the handle.
		if (internalCtx.CoreNativeManager.IsSplashScreen(Handle))
		{
			return Result.FromException<IWindow>(new WhimException($"Window {Handle} is a splash screen, ignoring"));
		}

		if (internalCtx.CoreNativeManager.IsCloakedWindow(Handle))
		{
			return Result.FromException<IWindow>(new WhimException($"Window {Handle} is cloaked, ignoring"));
		}

		if (!internalCtx.CoreNativeManager.IsStandardWindow(Handle))
		{
			return Result.FromException<IWindow>(
				new WhimException($"Window {Handle} is not a standard window, ignoring")
			);
		}

		if (!internalCtx.CoreNativeManager.HasNoVisibleOwner(Handle))
		{
			return Result.FromException<IWindow>(new WhimException($"Window {Handle} has a visible owner, ignoring"));
		}

		Result<IWindow> windowResult = Window.CreateWindow(ctx, internalCtx, Handle);
		if (!windowResult.TryGet(out IWindow window))
		{
			return windowResult;
		}

		// Filter the window.
		if (ctx.FilterManager.ShouldBeIgnored(window))
		{
			return Result.FromException<IWindow>(new WhimException("Window was ignored by filter"));
		}

		return Result.FromValue(window);
	}

	private static void UpdateWindowSector(MutableRootSector mutableRootSector, IWindow window)
	{
		WindowSector sector = mutableRootSector.Windows;
		sector.Windows = sector.Windows.Add(window.Handle, window);
		sector.QueueEvent(new WindowAddedEventArgs() { Window = window });
	}

	/// <summary>
	/// Updates the workspace for the added window.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="mutableRootSector"></param>
	/// <param name="window"></param>
	private static void UpdateMapSector(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		IWorkspace? workspace = TryGetWorkspaceFromRouter(ctx, internalCtx, window);

		// TODO: With WorkspaceSlice, this can be updated.
		// Check the workspace exists. If it doesn't, clear the workspace.
		if (workspace != null && !ctx.WorkspaceManager.Contains(workspace))
		{
			Logger.Error($"Workspace {workspace} was not found");
			workspace = null;
		}

		// If the workspace is still null, try to find a workspace for the window's monitor.
		workspace ??= TryGetWorkspaceFromMonitor(ctx, internalCtx, window);

		// If that fails too, route the window to the active workspace.
		workspace ??= ctx.WorkspaceManager.ActiveWorkspace;

		// Update the window workspace mapping.
		mutableRootSector.Maps.WindowWorkspaceMap = mutableRootSector.Maps.WindowWorkspaceMap.SetItem(
			window,
			workspace
		);

		if (window.IsMinimized)
		{
			workspace.MinimizeWindowStart(window);
		}
		else
		{
			workspace.AddWindow(window);
		}

		mutableRootSector.Maps.QueueEvent(RouteEventArgs.WindowAdded(window, workspace));

		workspace.DoLayout();
		window.Focus();
		Logger.Debug($"Window {window} added to workspace {workspace.Name}");
	}

	/// <summary>
	/// Try get the workspace based on routing preferences.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	private static IWorkspace? TryGetWorkspaceFromRouter(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		IWorkspace? workspace = null;

		// RouteWindow takes precedence over RouterOptions.
		if (ctx.RouterManager.RouteWindow(window) is IWorkspace routedWorkspace)
		{
			workspace = routedWorkspace;
		}
		else if (ctx.RouterManager.RouterOptions == RouterOptions.RouteToActiveWorkspace)
		{
			workspace = ctx.WorkspaceManager.ActiveWorkspace;
		}
		else if (ctx.RouterManager.RouterOptions == RouterOptions.RouteToLastTrackedActiveWorkspace)
		{
			workspace = internalCtx.MonitorManager.LastWhimActiveMonitor is IMonitor lastWhimActiveMonitor
				? ctx.Store.Pick(MapPickers.GetWorkspaceForMonitor(lastWhimActiveMonitor)).OrDefault()
				: ctx.WorkspaceManager.ActiveWorkspace;
		}

		return workspace;
	}

	/// <summary>
	/// Try get the workspace based on the active monitor.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	private static IWorkspace? TryGetWorkspaceFromMonitor(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		HMONITOR hmonitor = internalCtx.CoreNativeManager.MonitorFromWindow(
			window.Handle,
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
		);

		if (internalCtx.MonitorManager.GetMonitorByHandle(hmonitor) is IMonitor monitor)
		{
			return ctx.Store.Pick(MapPickers.GetWorkspaceForMonitor(monitor)).OrDefault();
		}

		return null;
	}
}
