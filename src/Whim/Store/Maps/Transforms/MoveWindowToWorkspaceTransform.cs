using DotNext;

namespace Whim;

/// <summary>
/// Moves the given <paramref name="Window"/> to the given <paramref name="TargetWorkspace"/>.
/// </summary>
/// <param name="TargetWorkspace">The workspace to move the window to.</param>
/// <param name="Window">
/// The window to move. If <see langword="null"/>, this will default to
/// the focused/active window.
/// </param>
public record MoveWindowToWorkspaceTransform(IWorkspace TargetWorkspace, IWindow? Window = null) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		IWindow? window = Window ?? ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to workspace {TargetWorkspace}");

		if (window == null)
		{
			return Result.FromException<Empty>(StoreExceptions.NoValidWindow());
		}

		Logger.Debug($"Moving window {window} to workspace {TargetWorkspace}");

		// Find the current workspace for the window.
		Result<IWorkspace> oldWorkspaceResult = ctx.Store.Pick(Pickers.GetWorkspaceForWindow(window));
		if (!oldWorkspaceResult.TryGet(out IWorkspace oldWorkspace))
		{
			return Result.FromException<Empty>(oldWorkspaceResult.Error!);
		}

		if (oldWorkspace.Equals(TargetWorkspace))
		{
			Logger.Debug($"Window {window} is already on workspace {TargetWorkspace}");
			return Empty.Result;
		}

		rootSector.Maps.WindowWorkspaceMap = rootSector.Maps.WindowWorkspaceMap.SetItem(window, TargetWorkspace);

		oldWorkspace.RemoveWindow(window);
		TargetWorkspace.AddWindow(window);

		// If both workspaces are visible, activate both
		// Otherwise, only layout the new workspace.
		if (
			ctx.Store.Pick(Pickers.GetMonitorForWorkspace(oldWorkspace)).IsSuccessful
			&& ctx.Store.Pick(Pickers.GetMonitorForWorkspace(TargetWorkspace)).IsSuccessful
		)
		{
			TargetWorkspace.DoLayout();
			oldWorkspace.DoLayout();
		}
		else
		{
			ctx.Store.Dispatch(new ActivateWorkspaceTransform(TargetWorkspace));
		}

		window.Focus();

		return Empty.Result;
	}
}
