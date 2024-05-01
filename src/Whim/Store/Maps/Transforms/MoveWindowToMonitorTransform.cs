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
			return Result.FromException<Empty>(new WhimException("No window was found"));
		}

		Logger.Debug($"Moving window {Window} to monitor {Monitor}");
		IMonitor? oldMonitor = ctx.Butler.Pantry.GetMonitorForWindow(window);
		if (oldMonitor == null)
		{
			return Result.FromException<Empty>(new WhimException($"Window {Window} was not found in any Monitor"));
		}

		if (oldMonitor.Equals(Monitor))
		{
			return Result.FromException<Empty>(new WhimException($"Window {Window} is already on monitor {Monitor}"));
		}

		IWorkspace? workspace = ctx.Butler.Pantry.GetWorkspaceForMonitor(Monitor);
		if (workspace == null)
		{
			return Result.FromException<Empty>(new WhimException($"Monitor {Monitor} was not found in any workspace"));
		}

		return ctx.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspace, window));
	}
}
