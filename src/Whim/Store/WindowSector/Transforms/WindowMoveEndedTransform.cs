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
			ctx.Store.Dispatch(
				new MoveWindowEdgesInDirectionTransform(moved.MovedEdges, moved.MovedPoint, Window.Handle)
			);
		}
		else if (internalCtx.CoreNativeManager.GetCursorPos(out point))
		{
			ctx.Store.Dispatch(new MoveWindowToPointTransform(Window.Handle, point));
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
