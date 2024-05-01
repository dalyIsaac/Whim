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
			return Result.FromException<Empty>(new WhimException("No window was found"));
		}

		Logger.Debug($"Moving window {window} to workspace {TargetWorkspace}");

		// Find the current workspace for the window.
		if (ctx.Butler.Pantry.GetWorkspaceForWindow(window) is not IWorkspace oldWorkspace)
		{
			return Result.FromException<Empty>(new WhimException($"Window {window} was not found in any workspace"));
		}

		if (oldWorkspace.Equals(TargetWorkspace))
		{
			return Result.FromException<Empty>(
				new WhimException($"Window {window} is already on workspace {TargetWorkspace}")
			);
		}

		ctx.Butler.Pantry.SetWindowWorkspace(window, TargetWorkspace);

		oldWorkspace.RemoveWindow(window);
		TargetWorkspace.AddWindow(window);

		// If both workspaces are visible, activate both
		// Otherwise, only layout the new workspace.
		if (
			ctx.Butler.Pantry.GetMonitorForWorkspace(oldWorkspace) is not null
			&& ctx.Butler.Pantry.GetMonitorForWorkspace(TargetWorkspace) is not null
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
