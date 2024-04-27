using DotNext;

namespace Whim;

internal record WindowMinimizeEndedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowMinimizeEndedEventArgs args = new() { Window = Window };
		internalCtx.ButlerEventHandlers.OnWindowMinimizeEnd(args);
		mutableRootSector.Windows.QueueEvent(args);
		return Empty.Result;
	}
}
