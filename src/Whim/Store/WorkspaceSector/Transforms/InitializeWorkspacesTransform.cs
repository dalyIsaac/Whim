using System.Linq;

namespace Whim;

/// <summary>
/// Initializes the state with the saved workspaces, and adds windows.
/// </summary>
internal record InitializeWorkspacesTransform : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		CreatePreInitializationWorkspaces(ctx, rootSector);
		PopulatedSavedWorkspaces(ctx, internalCtx, rootSector);

		return Unit.Result;
	}

	/// <summary>
	/// Create the workspaces which were specified prior to initialization.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="rootSector"></param>
	private static void CreatePreInitializationWorkspaces(IContext ctx, MutableRootSector rootSector)
	{
		WorkspaceSector workspaceSector = rootSector.WorkspaceSector;

		workspaceSector.HasInitialized = true;
		foreach (WorkspaceToCreate w in workspaceSector.WorkspacesToCreate)
		{
			ctx.Store.Dispatch(
				new AddWorkspaceTransform(w.Name, w.CreateLeafLayoutEngines, w.WorkspaceId, w.MonitorIndices)
			);
		}

		workspaceSector.WorkspacesToCreate = workspaceSector.WorkspacesToCreate.Clear();
	}

	/// <summary>
	/// Populate the existing workspaces with their saved windows, where possible.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="rootSector"></param>
	private static void PopulatedSavedWorkspaces(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		WorkspaceSector workspaceSector = rootSector.WorkspaceSector;
		WindowSector windowSector = rootSector.WindowSector;
		MapSector mapSector = rootSector.MapSector;

		windowSector.StartupWindows = [.. internalCtx.CoreNativeManager.GetAllWindows()];

		// Add the saved windows at their saved locations inside their saved workspaces.
		// Other windows are routed to the monitor they're on.
		List<HWND> processedWindows = [];

		// Route windows to their saved workspaces.
		foreach (SavedWorkspace savedWorkspace in internalCtx.CoreSavedStateManager.SavedState?.Workspaces ?? [])
		{
			Workspace? workspace = workspaceSector.Workspaces.Values.FirstOrDefault(w =>
				w.BackingName == savedWorkspace.Name
			);

			if (workspace == null)
			{
				Logger.Information($"Could not find workspace {savedWorkspace.Name}");
				continue;
			}

			PopulateSavedWindows(ctx, windowSector, mapSector, processedWindows, savedWorkspace, workspace);
		}

		// Activate the workspaces before we add the unprocessed windows to make sure we have a workspace for each monitor.
		ActivateWorkspaces(ctx, rootSector);

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

	private static void PopulateSavedWindows(
		IContext ctx,
		WindowSector windowSector,
		MapSector mapSector,
		List<HWND> processedWindows,
		SavedWorkspace savedWorkspace,
		Workspace workspace
	)
	{
		foreach (SavedWindow savedWindow in savedWorkspace.Windows)
		{
			HWND hwnd = (HWND)savedWindow.Handle;
			processedWindows.Add(hwnd);
			if (!ctx.CreateWindow(hwnd).TryGet(out IWindow window))
			{
				Logger.Information($"Could not find window with handle {savedWindow.Handle}");
				continue;
			}

			mapSector.WindowWorkspaceMap = mapSector.WindowWorkspaceMap.SetItem(window.Handle, workspace.Id);
			windowSector.Windows = windowSector.Windows.Add(window.Handle, window);

			ctx.Store.Dispatch(
				new MoveWindowToPointInWorkspaceTransform(workspace.Id, window.Handle, savedWindow.Rectangle.Center)
			);

			windowSector.QueueEvent(new WindowAddedEventArgs() { Window = window });
		}
	}

	private static void ActivateWorkspaces(IContext ctx, MutableRootSector rootSector)
	{
		WorkspaceSector workspaceSector = rootSector.WorkspaceSector;
		MonitorSector monitorSector = rootSector.MonitorSector;
		MapSector mapSector = rootSector.MapSector;

		// Assign workspaces to monitors.
		List<HMONITOR> processedMonitors = [];
		for (int idx = 0; idx < workspaceSector.WorkspaceOrder.Length; idx++)
		{
			WorkspaceId workspaceId = workspaceSector.WorkspaceOrder[idx];

			if (
				!ctx.Store.Pick(PickStickyMonitorsByWorkspace(workspaceId)).TryGet(out IReadOnlyList<HMONITOR> monitors)
			)
			{
				continue;
			}

			foreach (HMONITOR monitor in monitors)
			{
				if (!processedMonitors.Contains(monitor))
				{
					processedMonitors.Add(monitor);
					ctx.Store.Dispatch(new ActivateWorkspaceTransform(workspaceId, monitor));
					break;
				}
			}
		}

		// If there are any monitors left, create workspaces for them.
		IEnumerable<HMONITOR> unprocessedMonitors = monitorSector
			.Monitors.Select(m => m.Handle)
			.Except(processedMonitors);

		foreach (HMONITOR monitor in unprocessedMonitors)
		{
			ctx.Store.Dispatch(new AddWorkspaceTransform($"Workspace {workspaceSector.Workspaces.Count + 1}"));
			ctx.Store.Dispatch(new ActivateWorkspaceTransform(workspaceSector.WorkspaceOrder[^1], monitor));
		}
	}
}
