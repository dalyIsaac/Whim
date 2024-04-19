using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal record AddWindowTransform(HWND Handle) : Transform<IWindow>()
{
	internal override Result<IWindow> Execute(IContext ctx, IInternalContext internalCtx)
	{
		// Filter the handle.
		if (internalCtx.CoreNativeManager.IsSplashScreen(Handle))
		{
			return Result.FromException<IWindow>(new WhimException($"Window {Handle} is a splash screen, ignoring"));
		}

		if (internalCtx.CoreNativeManager.IsCloakedWindow(Handle))
		{
			return Result.FromException<IWindow>(new WhimException($"Window {Handle} is cloaked, ignoring"));
		}

		if (!internalCtx.CoreNativeManager.IsStandardWindow(Handle))
		{
			return Result.FromException<IWindow>(
				new WhimException($"Window {Handle} is not a standard window, ignoring")
			);
		}

		if (!internalCtx.CoreNativeManager.HasNoVisibleOwner(Handle))
		{
			return Result.FromException<IWindow>(new WhimException($"Window {Handle} has a visible owner, ignoring"));
		}

		Result<IWindow> windowResult = Window.CreateWindow(ctx, internalCtx, Handle);
		if (!windowResult.TryGet(out IWindow window))
		{
			return windowResult;
		}

		// Filter the window.
		if (ctx.FilterManager.ShouldBeIgnored(window))
		{
			return Result.FromException<IWindow>(new WhimException("Window was ignored by filter"));
		}

		// Store the window.
		ctx.Store.WindowSlice.Windows = ctx.Store.WindowSlice.Windows.Add(Handle, window);

		// Filter the args.
		WindowAddedEventArgs args = new() { Window = window };
		internalCtx.ButlerEventHandlers.OnWindowAdded(args);

		ctx.Store.WindowSlice.QueueEvent(args);

		return Result.FromValue(window);
	}
}
