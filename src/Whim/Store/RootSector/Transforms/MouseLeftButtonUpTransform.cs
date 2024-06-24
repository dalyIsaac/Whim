using DotNext;

namespace Whim;

internal record MouseLeftButtonUpTransform(IPoint<int> Point) : Transform
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		mutableRootSector.WindowSector.IsLeftMouseButtonDown = false;

		if (ctx.Store.Pick(PickMonitorAtPoint(Point)).TryGet(out IMonitor monitor))
		{
			mutableRootSector.MonitorSector.ActiveMonitorHandle = monitor.Handle;
		}

		return Unit.Result;
	}
}
