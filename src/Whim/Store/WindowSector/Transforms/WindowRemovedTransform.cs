using DotNext;

namespace Whim;

internal record WindowRemovedTransform(IWindow Window) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector windowSector = mutableRootSector.WindowSector;

		if (!windowSector.Windows.TryGetValue(Window.Handle, out IWindow? removedWindow))
		{
			Logger.Debug($"Window {Window} was not tracked, ignoring event");
			return Unit.Result;
		}

		windowSector.Windows = windowSector.Windows.Remove(Window.Handle);
		windowSector.HandledLocationRestoringWindows = windowSector.HandledLocationRestoringWindows.Remove(
			Window.Handle
		);

		WindowRemovedEventArgs args = new() { Window = removedWindow };
		internalCtx.ButlerEventHandlers.OnWindowRemoved(args);
		windowSector.QueueEvent(args);
		return Unit.Result;
	}
}
