namespace Whim;

internal record WindowRemovedTransform(IWindow Window) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector windowSector = mutableRootSector.WindowSector;
		MapSector mapSector = mutableRootSector.MapSector;

		if (!windowSector.Windows.ContainsKey(Window.Handle))
		{
			Logger.Debug($"Window {Window} was not tracked, ignoring event");
			return Unit.Result;
		}

		Result<IWorkspace> workspaceResult = ctx.Store.Pick(PickWorkspaceByWindow(Window.Handle));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Unit.Result;
		}

		UpdateWindowSector(mutableRootSector.WindowSector);
		UpdateMapSector(mapSector, workspace);

		return Unit.Result;
	}

	private void UpdateWindowSector(WindowSector windowSector)
	{
		windowSector.Windows = windowSector.Windows.Remove(Window.Handle);
		windowSector.QueueEvent(new WindowRemovedEventArgs() { Window = Window });
	}

	private void UpdateMapSector(MapSector mapSector, IWorkspace workspace)
	{
		mapSector.WindowWorkspaceMap = mapSector.WindowWorkspaceMap.Remove(Window.Handle);
		workspace.RemoveWindow(Window);

		mapSector.QueueEvent(RouteEventArgs.WindowRemoved(Window, workspace));
		workspace.DoLayout();
	}
}
