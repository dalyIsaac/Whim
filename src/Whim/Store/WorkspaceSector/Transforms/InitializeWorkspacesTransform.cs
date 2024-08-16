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
			ctx.Store.Dispatch(new AddWorkspaceTransform(w.Name, w.CreateLeafLayoutEngines, w.WorkspaceId));
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

		windowSector.StartupWindows = internalCtx.CoreNativeManager.GetAllWindows().ToImmutableHashSet();

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

			foreach (SavedWindow savedWindow in savedWorkspace.Windows)
			{
				HWND hwnd = (HWND)savedWindow.Handle;
				processedWindows.Add(hwnd);
				if (!ctx.WindowManager.CreateWindow(hwnd).TryGet(out IWindow window))
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

	private static void ActivateWorkspaces(IContext ctx, MutableRootSector rootSector)
	{
		WorkspaceSector workspaceSector = rootSector.WorkspaceSector;
		MonitorSector monitorSector = rootSector.MonitorSector;

		// Assign workspaces to monitors.
		for (int idx = 0; idx < monitorSector.Monitors.Length; idx++)
		{
			IMonitor monitor = monitorSector.Monitors[idx];

			if (idx >= workspaceSector.Workspaces.Count)
			{
				ctx.Store.Dispatch(new AddWorkspaceTransform($"Workspace {idx + 1}"));
			}

			ctx.Store.Dispatch(new ActivateWorkspaceTransform(workspaceSector.WorkspaceOrder[idx], monitor.Handle));
		}
	}
}
