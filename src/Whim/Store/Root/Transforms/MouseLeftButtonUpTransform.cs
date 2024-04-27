using DotNext;

namespace Whim;

internal record MouseLeftButtonUpTransform(IPoint<int> Point) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		// TODO: Test
		mutableRootSector.Windows.IsLeftMouseButtonDown = false;

		if (ctx.Store.Pick(new GetMonitorIndexAtPointPicker(Point)).TryGet(out int idx))
		{
			mutableRootSector.Monitors.ActiveMonitorIndex = idx;
		}

		return Empty.Result;
	}
}
