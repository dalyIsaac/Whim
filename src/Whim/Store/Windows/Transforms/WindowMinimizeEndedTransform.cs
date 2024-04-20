using DotNext;

namespace Whim;

internal record WindowMinimizeEndedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WindowMinimizeEndedEventArgs args = new() { Window = Window };
		internalCtx.ButlerEventHandlers.OnWindowMinimizeEnd(args);
		ctx.Store.WindowSlice.QueueEvent(args);
		return Empty.Result;
	}
}
