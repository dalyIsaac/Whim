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
		WindowSector sector = mutableRootSector.Windows;

		sector.Windows = sector.Windows.Remove(Window.Handle);
		sector.HandledLocationRestoringWindows = sector.HandledLocationRestoringWindows.Remove(Window);

		WindowRemovedEventArgs args = new() { Window = Window };
		internalCtx.ButlerEventHandlers.OnWindowRemoved(args);
		sector.QueueEvent(args);
		return Empty.Result;
	}
}
