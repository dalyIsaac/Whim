using DotNext;

namespace Whim;

/// <summary>
/// Moves the given <paramref name="Window"/> to the given <paramref name="Monitor"/>.
/// </summary>
/// <param name="Monitor">The monitor to move the window to.</param>
/// <param name="Window">
/// The window to move. If <see langword="null"/>, this will default to
/// the focused/active window.
/// </param>
public record MoveWindowToMonitorTransform(IMonitor Monitor, IWindow? Window = null) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		IWindow? window = Window ?? ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {Window} to monitor {Monitor}");

		if (window == null)
		{
			return Result.FromException<Empty>(StoreExceptions.NoValidWindow());
		}

		Logger.Debug($"Moving window {Window} to monitor {Monitor}");
		Result<IMonitor> oldMonitorResult = ctx.Store.Pick(Pickers.GetMonitorForWindow(window));
		if (!oldMonitorResult.TryGet(out IMonitor oldMonitor))
		{
			return Result.FromException<Empty>(oldMonitorResult.Error!);
		}

		if (oldMonitor.Equals(Monitor))
		{
			Logger.Debug($"Window {Window} is already on monitor {Monitor}");
			return Empty.Result;
		}

		Result<IWorkspace> workspaceResult = ctx.Store.Pick(Pickers.GetWorkspaceForMonitor(Monitor));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Empty>(workspaceResult.Error!);
		}

		return ctx.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspace, window));
	}
}
