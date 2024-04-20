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
		ctx.Store.WindowSlice.IsLeftMouseButtonDown = false;

		int? idx = ctx.Store.Pick(new GetMonitorIndexAtPointPicker(Point));

		if (idx is not int idxVal)
		{
			mutableRootSector.Monitors.ActiveMonitorIndex = idx;
		}

		return Empty.Result;
	}
}
