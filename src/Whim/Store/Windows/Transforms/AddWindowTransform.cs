using Windows.Win32.Foundation;

namespace Whim;

internal record AddWindowTransform(HWND Handle) : Transform()
{
	internal override void Execute(IContext ctx, IInternalContext internalCtx)
	{
		// Filter the handle.
		if (internalCtx.CoreNativeManager.IsSplashScreen(Handle))
		{
			Logger.Verbose($"Window {Handle} is a splash screen, ignoring");
			return;
		}

		if (internalCtx.CoreNativeManager.IsCloakedWindow(Handle))
		{
			Logger.Verbose($"Window {Handle} is cloaked, ignoring");
			return;
		}

		if (!internalCtx.CoreNativeManager.IsStandardWindow(Handle))
		{
			Logger.Verbose($"Window {Handle} is not a standard window, ignoring");
			return;
		}

		if (!internalCtx.CoreNativeManager.HasNoVisibleOwner(Handle))
		{
			Logger.Verbose($"Window {Handle} has a visible owner, ignoring");
			return;
		}

		IWindow? window = Window.CreateWindow(ctx, internalCtx, Handle);

		// Filter the window.
		if (window == null)
		{
			return;
		}

		if (ctx.FilterManager.ShouldBeIgnored(window))
		{
			return;
		}

		// Store the window.
		ctx.Store.WindowSlice.Windows = ctx.Store.WindowSlice.Windows.Add(Handle, window);

		// Filter the args.
		WindowAddedEventArgs args = new() { Window = window };
		internalCtx.ButlerEventHandlers.OnWindowAdded(args);

		ctx.Store.WindowSlice.QueueEvent(args);
	}
}
