using DotNext;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Focus the Windows desktop's window.
/// </summary>
/// <param name="MonitorHandle"></param>
public record FocusMonitorDesktopTransform(HMONITOR MonitorHandle) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		Result<IMonitor> monitorResult = ctx.Store.Pick(Pickers.PickMonitorByHandle(MonitorHandle));
		if (!monitorResult.TryGet(out IMonitor monitor))
		{
			return Result.FromException<Unit>(monitorResult.Error!);
		}

		HWND desktop = internalCtx.CoreNativeManager.GetDesktopWindow();
		internalCtx.CoreNativeManager.SetForegroundWindow(desktop);
		ctx.Store.Dispatch(new WindowFocusedTransform(null));

		internalCtx.MonitorManager.ActivateEmptyMonitor(monitor);

		return Unit.Result;
	}
}
