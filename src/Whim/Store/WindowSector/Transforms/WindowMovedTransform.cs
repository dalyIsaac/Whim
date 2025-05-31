namespace Whim;

internal record WindowMovedTransform(IWindow Window) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WindowSector windowSector = mutableRootSector.WindowSector;

		if (!windowSector.IsMovingWindow && Window.ProcessFileName == null)
		{
			return Unit.Result;
		}

		IPoint<int>? cursorPoint = null;
		if (windowSector.IsLeftMouseButtonDown && internalCtx.CoreNativeManager.GetCursorPos(out IPoint<int> point))
		{
			cursorPoint = point;
		}

		windowSector.QueueEvent(
			new WindowMovedEventArgs()
			{
				Window = Window,
				CursorDraggedPoint = cursorPoint,
				MovedEdges = WindowUtils.GetMovedEdges(ctx, Window)?.MovedEdges,
			}
		);

		return Unit.Result;
	}
}
