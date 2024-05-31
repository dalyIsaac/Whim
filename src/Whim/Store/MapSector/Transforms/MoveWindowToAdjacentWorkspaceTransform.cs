using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Moves the window with handle <paramref name="WindowHandle"/> to the next (or previous) workspace.
/// </summary>
/// <param name="WindowHandle">
/// The handle of the window to move. If not provided, this will default to the focused/active window.
/// </param>
/// <param name="Reverse">
/// When <see langword="true"/>, moves to the previous workspace, otherwise moves to the next workspace. Defaults to <see langword="false" />.
/// </param>
/// <param name="SkipActive">
/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
/// </param>
public record MoveWindowToAdjacentWorkspaceTransform(
	HWND WindowHandle = default,
	bool Reverse = false,
	bool SkipActive = false
) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		HWND windowHandle = WindowHandle.OrLastFocusedWindow(ctx);
		if (windowHandle == default)
		{
			return Result.FromException<Unit>(StoreExceptions.NoValidWindow());
		}

		Result<IWindow> windowResult = ctx.Store.Pick(Pickers.PickWindowByHandle(windowHandle));
		if (!windowResult.TryGet(out IWindow window))
		{
			return Result.FromException<Unit>(windowResult.Error!);
		}

		MapSector sector = rootSector.MapSector;

		// Find the current workspace for the window.
		if (
			!sector.WindowWorkspaceMap.TryGetValue(windowHandle, out WorkspaceId currentWorkspaceId)
			|| !ctx.Store.Pick(Pickers.PickWorkspaceById(currentWorkspaceId)).TryGet(out IWorkspace currentWorkspace)
		)
		{
			return Result.FromException<Unit>(StoreExceptions.NoWorkspaceFoundForWindow(windowHandle));
		}

		// Get the adjacent workspace for the current workspace.
		if (
			!ctx
				.Store.Pick(Pickers.PickAdjacentWorkspace(currentWorkspaceId, Reverse, SkipActive))
				.TryGet(out IWorkspace? nextWorkspace)
		)
		{
			Logger.Debug($"No next workspace found");
			return Unit.Result;
		}

		sector.WindowWorkspaceMap = sector.WindowWorkspaceMap.SetItem(windowHandle, nextWorkspace.Id);

		currentWorkspace.RemoveWindow(window);
		nextWorkspace.AddWindow(window);

		currentWorkspace.DoLayout();
		nextWorkspace.DoLayout();

		window.Focus();
		return Unit.Result;
	}
}
