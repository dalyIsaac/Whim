using DotNext;

namespace Whim;

internal record WindowMoveStartedTransform(IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector sector = mutableRootSector.Windows;

		sector.IsMovingWindow = true;

		IPoint<int>? cursorPoint = null;
		if (sector.IsLeftMouseButtonDown && internalCtx.CoreNativeManager.GetCursorPos(out IPoint<int> point))
		{
			cursorPoint = point;
		}

		sector.QueueEvent(
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
