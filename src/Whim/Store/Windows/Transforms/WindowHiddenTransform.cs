using DotNext;

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
/// <param name="Window"></param>
internal record WindowHiddenTransform(IWindow Window) : WindowRemovedTransform(Window)
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		if (ctx.Store.Pick(Pickers.GetMonitorForWindow(Window)).IsSuccessful)
		{
			return base.Execute(ctx, internalCtx, mutableRootSector);
		}

		Logger.Debug($"Window {Window} is not tracked in a monitor, ignoring event");
		return Empty.Result;
	}
}
