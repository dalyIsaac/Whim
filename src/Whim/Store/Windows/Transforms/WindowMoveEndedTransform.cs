using DotNext;

namespace Whim;

internal record WindowMoveEndedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector sector = mutableRootSector.Windows;

		IPoint<int>? point = null;
		Direction? movedEdges = null;

		if (!sector.IsMovingWindow)
		{
			return Empty.Result;
		}

		if (WindowUtils.GetMovedEdges(ctx, Window) is (Direction, IPoint<int>) moved)
		{
			movedEdges = moved.MovedEdges;
			ctx.Store.Dispatch(new MoveWindowEdgesInDirectionTransform(moved.MovedEdges, moved.MovedPoint, Window));
		}
		else if (internalCtx.CoreNativeManager.GetCursorPos(out point))
		{
			ctx.Store.Dispatch(new MoveWindowToPointTransform(Window, point));
		}

		sector.IsMovingWindow = false;

		sector.QueueEvent(
			new WindowMoveEndedEventArgs()
			{
				Window = Window,
				CursorDraggedPoint = point,
				MovedEdges = movedEdges
			}
		);

		return Empty.Result;
	}
}
