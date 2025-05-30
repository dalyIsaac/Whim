namespace Whim;

internal record WindowMoveStartedTransform(IWindow Window) : WhimTransform
{
	internal override WhimResult<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector windowSector = mutableRootSector.WindowSector;

		windowSector.IsMovingWindow = true;

		IPoint<int>? cursorPoint = null;
		if (windowSector.IsLeftMouseButtonDown && internalCtx.CoreNativeManager.GetCursorPos(out IPoint<int> point))
		{
			cursorPoint = point;
		}

		windowSector.QueueEvent(
			new WindowMoveStartedEventArgs()
			{
				Window = Window,
				CursorDraggedPoint = cursorPoint,
				MovedEdges = WindowUtils.GetMovedEdges(ctx, Window)?.MovedEdges,
			}
		);

		return Unit.Result;
	}
}
