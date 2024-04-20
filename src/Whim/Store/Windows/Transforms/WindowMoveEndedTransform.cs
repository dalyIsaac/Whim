using DotNext;

namespace Whim;

internal record WindowMoveEndedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WindowSlice slice = ctx.Store.WindowSlice;

		IPoint<int>? point = null;
		Direction? movedEdges = null;

		if (!slice.IsMovingWindow)
		{
			return Empty.Result;
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

		slice.IsMovingWindow = false;

		slice.QueueEvent(
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
