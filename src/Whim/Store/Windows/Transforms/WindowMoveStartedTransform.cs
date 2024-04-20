using DotNext;

namespace Whim;

internal record WindowMoveStartedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WindowSlice slice = ctx.Store.WindowSlice;

		slice.IsMovingWindow = true;

		IPoint<int>? cursorPoint = null;
		if (slice.IsLeftMouseButtonDown && internalCtx.CoreNativeManager.GetCursorPos(out IPoint<int> point))
		{
			cursorPoint = point;
		}

		slice.QueueEvent(
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
