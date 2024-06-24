using DotNext;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

internal static class WindowUtils
{
	/// <summary>
	/// Tries to move the given window's edges in the direction of the mouse movement.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	public static (Direction MovedEdges, IPoint<int> MovedPoint)? GetMovedEdges(IContext ctx, IWindow window)
	{
		Logger.Debug("Trying to move window edges in direction of mouse movement");
		Result<IWorkspace> workspaceResult = ctx.Store.Pick(Pickers.PickWorkspaceByWindow(window.Handle));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			Logger.Debug($"Could not find workspace for window {window}, failed to move window edges");
			return null;
		}

		Result<WindowPosition> windowPositionResult = ctx.Store.Pick(
			Pickers.PickWindowPosition(workspace.Id, window.Handle)
		);
		if (!windowPositionResult.TryGet(out WindowPosition windowPosition))
		{
			Logger.Debug($"Could not find window state for window {window}, failed to move window edges");
			return null;
		}

		IRectangle<int> currRect = windowPosition.LastWindowRectangle;

		// Get the new window position.
		IRectangle<int>? newRect = ctx.NativeManager.DwmGetWindowRectangle(window.Handle);
		if (newRect is null)
		{
			Logger.Debug($"Could not get new rectangle for window {window}, failed to move window edges");
			return null;
		}

		// Find the one or two edges to move.
		int leftEdgeDelta = currRect.X - newRect.X;
		int topEdgeDelta = currRect.Y - newRect.Y;
		int rightEdgeDelta = newRect.X + newRect.Width - (currRect.X + currRect.Width);
		int bottomEdgeDelta = newRect.Y + newRect.Height - (currRect.Y + currRect.Height);

		int movedEdgeCountX = 0;
		int movedEdgeCountY = 0;
		int movedEdgeDeltaX = 0;
		int movedEdgeDeltaY = 0;
		Direction movedEdges = Direction.None;
		if (leftEdgeDelta != 0)
		{
			movedEdges |= Direction.Left;
			movedEdgeDeltaX = -leftEdgeDelta;
			movedEdgeCountX++;
		}
		if (topEdgeDelta != 0)
		{
			movedEdges |= Direction.Up;
			movedEdgeDeltaY = -topEdgeDelta;
			movedEdgeCountY++;
		}
		if (rightEdgeDelta != 0)
		{
			movedEdges |= Direction.Right;
			movedEdgeDeltaX = rightEdgeDelta;
			movedEdgeCountX++;
		}
		if (bottomEdgeDelta != 0)
		{
			movedEdges |= Direction.Down;
			movedEdgeDeltaY = bottomEdgeDelta;
			movedEdgeCountY++;
		}

		if (movedEdgeCountX > 1 || movedEdgeCountY > 1)
		{
			Logger.Debug($"Window {window} moved more than one edge in the same axis, failed to move window edges");
			return null;
		}

		return (movedEdges, new Point<int>() { X = movedEdgeDeltaX, Y = movedEdgeDeltaY, });
	}

	public static HWND OrLastFocusedWindow(this HWND handle, IContext ctx)
	{
		if (handle == default)
		{
			return ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow?.Handle ?? default;
		}

		return handle;
	}

	public static void Focus(this HWND handle, IInternalContext internalCtx)
	{
		// Use SendInput hack to allow Activate to work - required to resolve focus issue https://github.com/microsoft/PowerToys/issues/4270
		unsafe
		{
			INPUT input = new() { type = INPUT_TYPE.INPUT_MOUSE };
			// Send empty mouse event. This makes this thread the last to send input, and hence allows it to pass foreground permission checks
			_ = internalCtx.CoreNativeManager.SendInput(new[] { input }, sizeof(INPUT));
		}

		internalCtx.CoreNativeManager.SetForegroundWindow(handle);
	}
}
