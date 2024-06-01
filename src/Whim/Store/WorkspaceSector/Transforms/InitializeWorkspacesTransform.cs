using System.Collections.Generic;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Initializes the state with the saved workspaces, and adds windows.
/// </summary>
internal record InitializeWorkspacesTransform : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		MapSector mapSector = rootSector.MapSector;
		WorkspaceSector workspaceSector = rootSector.WorkspaceSector;
		WindowSector windowSector = rootSector.WindowSector;
		MonitorSector monitorSector = rootSector.MonitorSector;

		CreatePreInitializationWorkspaces(ctx, workspaceSector);
		PopulatedSavedWorkspaces(ctx, internalCtx, mapSector, windowSector);
		ActivateWorkspaces(ctx, workspaceSector, monitorSector);

		return Unit.Result;
	}

	/// <summary>
	/// Create the workspaces which were specified prior to initialization.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="workspaceSector"></param>
	private static void CreatePreInitializationWorkspaces(IContext ctx, WorkspaceSector workspaceSector)
	{
		workspaceSector.HasInitialized = true;
		foreach (WorkspaceToCreate w in workspaceSector.WorkspacesToCreate)
		{
			ctx.Store.Dispatch(new AddWorkspaceTransform(w.Name, w.LayoutEngines));
		}

		workspaceSector.WorkspacesToCreate = workspaceSector.WorkspacesToCreate.Clear();
	}

	/// <summary>
	/// Populate the existing workspaces with their saved windows, where possible.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="mapSector"></param>
	/// <param name="windowSector"></param>
	private static void PopulatedSavedWorkspaces(
		IContext ctx,
		IInternalContext internalCtx,
		MapSector mapSector,
		WindowSector windowSector
	)
	{
		// Add the saved windows at their saved locations inside their saved workspaces.
		// Other windows are routed to the monitor they're on.
		List<HWND> processedWindows = new();

		// Route windows to their saved workspaces.
		foreach (SavedWorkspace savedWorkspace in internalCtx.CoreSavedStateManager.SavedState?.Workspaces ?? new())
		{
			IWorkspace? workspace = ctx.WorkspaceManager.TryGet(savedWorkspace.Name);
			if (workspace == null)
			{
				Logger.Debug($"Could not find workspace {savedWorkspace.Name}");
				continue;
			}

			foreach (SavedWindow savedWindow in savedWorkspace.Windows)
			{
				HWND hwnd = (HWND)savedWindow.Handle;
				if (!ctx.WindowManager.CreateWindow(hwnd).TryGet(out IWindow window))
				{
					Logger.Debug($"Could not find window with handle {savedWindow.Handle}");
					continue;
				}

				mapSector.WindowWorkspaceMap = mapSector.WindowWorkspaceMap.SetItem(window.Handle, workspace.Id);
				windowSector.Windows = windowSector.Windows.Add(window.Handle, window);

				workspace.MoveWindowToPoint(window, savedWindow.Rectangle.Center, deferLayout: false);
				processedWindows.Add(hwnd);

				windowSector.QueueEvent(new WindowAddedEventArgs() { Window = window });
			}
		}

		// Route the rest of the windows to the monitor they're on.
		// Don't route to the active workspace while we're adding all the windows.

		// Add all existing windows.
		foreach (HWND hwnd in internalCtx.CoreNativeManager.GetAllWindows())
		{
			if (processedWindows.Contains(hwnd))
			{
				continue;
			}

			ctx.Store.Dispatch(new WindowAddedTransform(hwnd, RouterOptions.RouteToLaunchedWorkspace));
		}
	}

	private static void ActivateWorkspaces(IContext ctx, WorkspaceSector workspaceSector, MonitorSector monitorSector)
	{
		// Assign workspaces to monitors.
		for (int idx = 0; idx < monitorSector.Monitors.Length; idx++)
		{
			IMonitor monitor = monitorSector.Monitors[idx];

			if (idx < workspaceSector.Workspaces.Count)
			{
				ctx.Store.Dispatch(new AddWorkspaceTransform($"Workspace {idx}"));
			}

			WorkspaceId workspaceId = workspaceSector.WorkspaceOrder[idx];

			ctx.Store.Dispatch(new ActivateWorkspaceTransform(workspaceId, monitor.Handle));
		}
	}
}
