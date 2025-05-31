namespace Whim;

internal record WindowMoveEndedTransform(IWindow Window) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector windowSector = mutableRootSector.WindowSector;

		IPoint<int>? point = null;
		Direction? movedEdges = null;

		if (!windowSector.IsMovingWindow)
		{
			return Unit.Result;
		}

		if (WindowUtils.GetMovedEdges(ctx, Window) is (Direction, IPoint<int>) moved)
		{
			movedEdges = moved.MovedEdges;
			ctx.Butler.MoveWindowEdgesInDirection(moved.MovedEdges, moved.MovedPoint, Window);
		}
		else if (internalCtx.CoreNativeManager.GetCursorPos(out point))
		{
			ctx.Butler.MoveWindowToPoint(Window, point);
		}

		windowSector.IsMovingWindow = false;

		windowSector.QueueEvent(
			new WindowMoveEndedEventArgs()
			{
				Window = Window,
				CursorDraggedPoint = point,
				MovedEdges = movedEdges,
			}
		);

		return Unit.Result;
	}
}
