using DotNext;

namespace Whim;

/// <summary>
/// Moves the given <paramref name="Window"/> to the next (or previous) workspace.
/// </summary>
/// <param name="Window">
/// The window to move. If <see langword="null"/>, this will default to the focused/active window.
/// </param>
/// <param name="Reverse">
/// When <see langword="true"/>, moves to the previous workspace, otherwise moves to the next workspace. Defaults to <see langword="false" />.
/// </param>
/// <param name="SkipActive">
/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
/// </param>
public record MoveWindowToAdjacentWorkspaceTransform(
	IWindow? Window = null,
	bool Reverse = false,
	bool SkipActive = false
) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		IWindow? window = Window ?? ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		if (window == null)
		{
			return Result.FromException<Empty>(new WhimException("No window was found"));
		}

		MapSector sector = rootSector.Maps;

		// Find the current workspace for the window.
		if (
			!sector.WindowWorkspaceMap.TryGetValue(window, out IWorkspace? currentWorkspace)
			|| currentWorkspace == null
		)
		{
			return Result.FromException<Empty>(new WhimException($"Window {window} was not found in any workspace"));
		}

		// Get the adjacent workspace for the current workspace.
		if (
			!ctx
				.Store.Pick(Pickers.GetAdjacentWorkspace(currentWorkspace, Reverse, SkipActive))
				.TryGet(out IWorkspace? nextWorkspace)
		)
		{
			Logger.Debug($"No next workspace found");
			return Empty.Result;
		}

		sector.WindowWorkspaceMap = sector.WindowWorkspaceMap.SetItem(window, nextWorkspace);

		currentWorkspace.RemoveWindow(window);
		nextWorkspace.AddWindow(window);

		currentWorkspace.DoLayout();
		nextWorkspace.DoLayout();

		window.Focus();
		return Empty.Result;
	}
}
