namespace Whim;

/// <summary>
/// Moves the window with <paramref name="WindowHandle"/> to the next (or previous) monitor.
/// </summary>
/// <param name="WindowHandle">
/// The handle of the window to move. If not provided, this will default to the focused/active window.
/// </param>
/// <param name="Reverse">
/// When <see langword="true"/>, moves to the previous monitor, otherwise moves to the next monitor. Defaults to <see langword="false" />.
/// </param>
public record MoveWindowToAdjacentMonitorTransform(HWND WindowHandle = default, bool Reverse = false) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		HMONITOR activeMonitorHandle = rootSector.MonitorSector.ActiveMonitorHandle;

		Result<IMonitor> targetMonitorResult = ctx.Store.Pick(PickAdjacentMonitor(activeMonitorHandle, Reverse));
		if (!targetMonitorResult.TryGet(out IMonitor? targetMonitor))
		{
			return Result.FromError<Unit>(targetMonitorResult.Error!);
		}

		return ctx.Store.Dispatch(new MoveWindowToMonitorTransform(targetMonitor.Handle, WindowHandle));
	}
}
