using DotNext;

namespace Whim;

internal record WindowRemovedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WindowSlice slice = ctx.Store.WindowSlice;

		slice.Windows = slice.Windows.Remove(Window.Handle);
		slice.HandledLocationRestoringWindows = slice.HandledLocationRestoringWindows.Remove(Window);

		WindowRemovedEventArgs args = new() { Window = Window };
		internalCtx.ButlerEventHandlers.OnWindowRemoved(args);
		slice.QueueEvent(args);
		return Empty.Result;
	}
}
