using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Focus the Windows desktop's window.
/// </summary>
/// <param name="Monitor"></param>
public record FocusMonitorDesktopTransform(IMonitor Monitor) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		HWND desktop = internalCtx.CoreNativeManager.GetDesktopWindow();
		internalCtx.CoreNativeManager.SetForegroundWindow(desktop);
		ctx.Store.Dispatch(new WindowFocusedTransform(null));
		internalCtx.MonitorManager.ActivateEmptyMonitor(Monitor);

		return Empty.Result;
	}
}
