using DotNext;

namespace Whim;

internal record WindowMinimizeStartedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector sector = mutableRootSector.Windows;

		WindowMinimizeStartedEventArgs args = new() { Window = Window };
		internalCtx.ButlerEventHandlers.OnWindowMinimizeStart(args);

		sector.QueueEvent(args);
		return Empty.Result;
	}
}
