namespace Whim;

/// <summary>
/// Move the window with <paramref name="WindowHandle"/> to the monitor at <paramref name="MonitorIndex"/>.
/// </summary>
/// <param name="MonitorIndex">
/// The 0-based index of the monitor to move the window to.
/// </param>
/// <param name="WindowHandle">
/// The handle of the window to move. If not provided, this will default to the focused/active window.
/// </param>
public record MoveWindowToMonitorIndexTransform(int MonitorIndex, HWND WindowHandle = default) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Get the monitor at index.
		Result<IMonitor> monitorResult = ctx.Store.Pick(PickMonitorByIndex(MonitorIndex));
		if (!monitorResult.TryGet(out IMonitor monitor))
		{
			return Result.FromException<Unit>(monitorResult.Error!);
		}

		return ctx.Store.Dispatch(new MoveWindowToMonitorTransform(monitor.Handle, WindowHandle));
	}
}
