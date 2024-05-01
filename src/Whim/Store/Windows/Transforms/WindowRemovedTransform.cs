using DotNext;

namespace Whim;

internal record WindowRemovedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		UpdateWindowSector(mutableRootSector);
		return UpdateMapSector(ctx, mutableRootSector, Window);
	}

	private void UpdateWindowSector(MutableRootSector mutableRootSector)
	{
		WindowSector sector = mutableRootSector.Windows;

		sector.Windows = sector.Windows.Remove(Window.Handle);
		sector.HandledLocationRestoringWindows = sector.HandledLocationRestoringWindows.Remove(Window);

		sector.QueueEvent(new WindowRemovedEventArgs() { Window = Window });
	}

	private static Result<Empty> UpdateMapSector(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		if (!ctx.Store.Pick(Pickers.GetWorkspaceForWindow(window)).TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Empty>(new WhimException($"Window {window} was not found in any workspace"));
		}

		MapSector sector = mutableRootSector.Maps;

		sector.WindowWorkspaceMap = sector.WindowWorkspaceMap.Remove(window);
		workspace.RemoveWindow(window);

		sector.QueueEvent(RouteEventArgs.WindowRemoved(window, workspace));
		workspace.DoLayout();

		return Empty.Result;
	}
}
