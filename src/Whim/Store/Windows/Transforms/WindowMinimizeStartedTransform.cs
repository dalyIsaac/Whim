namespace Whim;

internal record WindowMinimizeStartedTransform(IWindow Window) : Transform
{
	internal override void Execute(IContext ctx, IInternalContext internalCtx)
	{
		WindowSlice slice = ctx.Store.WindowSlice;

		WindowMinimizeStartedEventArgs args = new() { Window = Window };
		internalCtx.ButlerEventHandlers.OnWindowMinimizeStart(args);

		slice.QueueEvent(args);
	}
}