namespace Whim;

/// <summary>
/// Focus the Windows desktop's window.
/// </summary>
/// <param name="MonitorHandle"></param>
public record FocusMonitorDesktopTransform(HMONITOR MonitorHandle) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		Result<Unit> result = ctx.Store.Dispatch(new ActivateEmptyMonitorTransform(MonitorHandle));
		if (!result.IsSuccessful)
		{
			return result;
		}

		rootSector.WorkspaceSector.WindowHandleToFocus = internalCtx.CoreNativeManager.GetDesktopWindow();
		return result;
	}
}
