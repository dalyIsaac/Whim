using DotNext;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Moves the window with <paramref name="WindowHandle"/> to the monitor with <paramref name="MonitorHandle"/>.
/// </summary>
/// <param name="MonitorHandle">
/// The handle of the monitor to move the window to.
/// </param>
/// <param name="WindowHandle">
/// The handle of the window to move. If not provided, this will default to the focused/active window.
/// </param>
public record MoveWindowToMonitorTransform(HMONITOR MonitorHandle, HWND WindowHandle = default) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		MapSector mapSector = rootSector.MapSector;

		HWND windowHandle = WindowHandle;
		if (windowHandle == default)
		{
			if (ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow == null)
			{
				return Result.FromException<Unit>(StoreExceptions.NoValidWindow());
			}

			windowHandle = ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Handle;
		}

		Logger.Debug($"Moving window {windowHandle} to monitor {MonitorHandle}");

		Result<IMonitor> oldMonitorResult = ctx.Store.Pick(Pickers.PickMonitorByWindow(WindowHandle));
		if (!oldMonitorResult.TryGet(out IMonitor oldMonitor))
		{
			return Result.FromException<Unit>(oldMonitorResult.Error!);
		}

		if (oldMonitor.Handle == MonitorHandle)
		{
			Logger.Debug($"Window {WindowHandle} is already on monitor {MonitorHandle}");
			return Unit.Result;
		}

		Result<IWorkspace> workspaceResult = ctx.Store.Pick(Pickers.PickWorkspaceByMonitor(MonitorHandle));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Unit>(workspaceResult.Error!);
		}

		return ctx.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspace.Id, windowHandle));
	}
}
