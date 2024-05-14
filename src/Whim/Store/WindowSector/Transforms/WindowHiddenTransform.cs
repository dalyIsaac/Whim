using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Handles when a window is hidden.
/// This will be called when a workspace is deactivated, or when a process hides a window.
/// For example, Discord will hide its window when it is minimized.
/// We only care about the hide event if the workspace is active.	/// Handles when a window is hidden.
/// This will be called when a workspace is deactivated, or when a process hides a window.
/// For example, Discord will hide its window when it is minimized.
/// We only care about the hide event if the workspace is active.
/// </summary>
/// <param name="Handle">
/// The <see cref="HWND"/> handle of the window.
/// </param>
internal record WindowHiddenTransform(HWND Handle) : WindowRemovedTransform(Handle)
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		IWindow window = WindowUtils.GetWindow(mutableRootSector, Handle)!;
		if (ctx.Butler.Pantry.GetMonitorForWindow(window) == null)
		{
			Logger.Debug($"Window {window} is not tracked in a monitor, ignoring event");
			return Unit.Result;
		}

		return base.Execute(ctx, internalCtx, mutableRootSector);
	}
}
