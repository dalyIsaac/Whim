using DotNext;

namespace Whim;

internal record MouseLeftButtonUpTransform(IPoint<int> Point) : Transform
{
	/// <summary>
	/// Set the active monitor based on the user's last mouse click.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		if (ctx.Store.Pick(new GetMonitorIndexAtPointPicker(Point)).TryGet(out int idx))
		{
			ctx.Store.MonitorSlice.ActiveMonitorIndex = idx;
		}

		return Empty.Result;
	}
}
