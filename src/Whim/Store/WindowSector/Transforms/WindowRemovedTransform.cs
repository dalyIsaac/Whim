using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal record WindowRemovedTransform(HWND Handle) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector windowSector = mutableRootSector.WindowSector;

		if (!windowSector.Windows.TryGetValue(Handle, out IWindow? removedWindow))
		{
			Logger.Debug($"Window {Handle} was not tracked, ignoring event");
			return Unit.Result;
		}

		windowSector.Windows = windowSector.Windows.Remove(Handle);
		windowSector.HandledLocationRestoringWindows = windowSector.HandledLocationRestoringWindows.Remove(Handle);

		WindowRemovedEventArgs args = new() { Window = removedWindow };
		internalCtx.ButlerEventHandlers.OnWindowRemoved(args);
		windowSector.QueueEvent(args);
		return Unit.Result;
	}
}
