namespace Whim;

/// <summary>
/// Focus the Windows desktop's window.
/// </summary>
/// <param name="MonitorHandle"></param>
public record FocusMonitorDesktopTransform(HMONITOR MonitorHandle) : WhimTransform
{
	internal override WhimResult<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		Result<IMonitor> monitorResult = ctx.Store.Pick(PickMonitorByHandle(MonitorHandle));
		if (!monitorResult.TryGet(out IMonitor monitor))
		{
			return WhimResult.FromException<Unit>(monitorResult.Error!);
		}

		rootSector.WorkspaceSector.WindowHandleToFocus = internalCtx.CoreNativeManager.GetDesktopWindow();
		internalCtx.MonitorManager.ActivateEmptyMonitor(monitor);

		return Unit.Result;
	}
}
