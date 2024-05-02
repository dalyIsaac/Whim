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
		if (!ctx.Store.Pick(Pickers.GetWorkspaceForWindow(window)).TryGet(out IWorkspace workspace))
		{
			Logger.Debug($"Could not find workspace for window {window}, failed to move window edges");
			return null;
		}

		IWindowState? windowState = workspace.TryGetWindowState(window);
		if (windowState is null)
		{
			Logger.Debug($"Could not find window state for window {window}, failed to move window edges");
			return null;
		}

		// Get the new window position.
		IRectangle<int>? newRect = ctx.NativeManager.DwmGetWindowRectangle(window.Handle);
		if (newRect is null)
		{
			Logger.Debug($"Could not get new rectangle for window {window}, failed to move window edges");
			return null;
		}

		// Find the one or two edges to move.
		int leftEdgeDelta = windowState.Rectangle.X - newRect.X;
		int topEdgeDelta = windowState.Rectangle.Y - newRect.Y;
		int rightEdgeDelta = newRect.X + newRect.Width - (windowState.Rectangle.X + windowState.Rectangle.Width);
		int bottomEdgeDelta = newRect.Y + newRect.Height - (windowState.Rectangle.Y + windowState.Rectangle.Height);

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
}
