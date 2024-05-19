using DotNext;

namespace Whim;

internal record WindowRemovedTransform(IWindow Window) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		Result<Unit> windowSectorResult = UpdateWindowSector(mutableRootSector);
		if (!windowSectorResult.IsSuccessful)
		{
			return windowSectorResult;
		}

		return UpdateMapSector(ctx, mutableRootSector);
	}

	private Result<Unit> UpdateWindowSector(MutableRootSector mutableRootSector)
	{
		WindowSector windowSector = mutableRootSector.WindowSector;

		if (!windowSector.Windows.TryGetValue(Window.Handle, out IWindow? removedWindow))
		{
			Logger.Debug($"Window {Window} was not tracked, ignoring event");
			return Unit.Result;
		}

		windowSector.Windows = windowSector.Windows.Remove(Window.Handle);
		windowSector.HandledLocationRestoringWindows = windowSector.HandledLocationRestoringWindows.Remove(
			Window.Handle
		);

		windowSector.QueueEvent(new WindowRemovedEventArgs() { Window = removedWindow });

		return Unit.Result;
	}

	private Result<Unit> UpdateMapSector(IContext ctx, MutableRootSector rootSector)
	{
		Result<IWorkspace> workspaceResult = ctx.Store.Pick(Pickers.PickWorkspaceByWindow(Window.Handle));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Unit>(workspaceResult.Error!);
		}

		MapSector sector = rootSector.MapSector;

		sector.WindowWorkspaceMap = sector.WindowWorkspaceMap.Remove(Window.Handle);
		workspace.RemoveWindow(Window);

		sector.QueueEvent(RouteEventArgs.WindowRemoved(Window, workspace));
		workspace.DoLayout();

		return Unit.Result;
	}
}
