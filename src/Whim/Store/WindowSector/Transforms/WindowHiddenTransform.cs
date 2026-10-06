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
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		// While monitors are being added/removed, Windows can briefly hide a window as part of
		// the display topology change itself (observed with wezterm, which has no window chrome).
		// If that hide event lands after the window's workspace has already been remapped back
		// onto a (possibly different) monitor, the monitor lookup below would wrongly succeed and
		// we'd treat a transient, OS-driven hide as if the app had genuinely hidden its own window,
		// permanently removing it from its workspace while the process keeps running, untracked and
		// invisible. Defer to the layout that runs once the monitor change settles instead.
		if (mutableRootSector.MonitorSector.MonitorsChangingTasks > 0)
		{
			Logger.Debug($"Monitors are changing, ignoring hide event for {Window}");
			return Unit.Result;
		}

		if (!ctx.Store.Pick(PickMonitorByWindow(Window.Handle)).IsSuccessful)
		{
			Logger.Debug($"Window {Window} is not on a monitor, ignoring event");
			return Unit.Result;
		}

		return base.Execute(ctx, internalCtx, mutableRootSector);
	}
}
