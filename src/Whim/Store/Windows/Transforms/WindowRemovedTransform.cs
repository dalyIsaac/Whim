using DotNext;

namespace Whim;

internal record WindowRemovedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		ctx.Store.WindowSlice.Windows = ctx.Store.WindowSlice.Windows.Remove(Window.Handle);
		ctx.Store.WindowSlice.HandledLocationRestoringWindows =
			ctx.Store.WindowSlice.HandledLocationRestoringWindows.Remove(Window);

		WindowRemovedEventArgs args = new() { Window = Window };
		internalCtx.ButlerEventHandlers.OnWindowRemoved(args);
		ctx.Store.WindowSlice.QueueEvent(args);
		return Empty.Result;
	}
}
