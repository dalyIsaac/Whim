namespace Whim;

internal record MouseLeftButtonUpTransform(IPoint<int> Point) : Transform
{
	/// <summary>
	/// Set the active monitor based on the user's last mouse click.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	internal override void Execute(IContext ctx, IInternalContext internalCtx)
	{
		int? idx = ctx.Store.Pick(new GetMonitorIndexAtPointPicker(Point));

		if (idx is not int idxVal)
		{
			return;
		}

		ctx.Store.MonitorSlice.ActiveMonitorIndex = idxVal;
	}
}
