using DotNext;

namespace Whim;

internal record WindowMoveStartedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		ctx.Store.WindowSlice.IsMovingWindow = true;

		IPoint<int>? cursorPoint = null;
		if (
			ctx.Store.WindowSlice.IsLeftMouseButtonDown
			&& internalCtx.CoreNativeManager.GetCursorPos(out IPoint<int> point)
		)
		{
			cursorPoint = point;
		}

		ctx.Store.WindowSlice.QueueEvent(
			new WindowMoveStartedEventArgs()
			{
				Window = Window,
				CursorDraggedPoint = cursorPoint,
				MovedEdges = WindowUtils.GetMovedEdges(ctx, Window)?.MovedEdges
			}
		);

		return Empty.Result;
	}
}
