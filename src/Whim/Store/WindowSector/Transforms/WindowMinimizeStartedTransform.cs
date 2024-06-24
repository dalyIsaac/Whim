using DotNext;

namespace Whim;

internal record WindowMinimizeStartedTransform(IWindow Window) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector windowSector = mutableRootSector.WindowSector;

		WindowMinimizeStartedEventArgs args = new() { Window = Window };
		internalCtx.ButlerEventHandlers.OnWindowMinimizeStart(args);

		windowSector.QueueEvent(args);
		return Unit.Result;
	}
}
