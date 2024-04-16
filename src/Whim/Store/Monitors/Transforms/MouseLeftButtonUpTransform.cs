namespace Whim;

internal record MouseLeftButtonUpTransform(IPoint<int> Point) : Transform
{
	internal override void Execute(IContext ctx, IInternalContext internalCtx)
	{
		Logger.Debug($"Mouse left button up");
		int? idx = ctx.Store.Pick(new GetMonitorIndexAtPointPicker(Point));

		if (idx is not int idxVal)
		{
			return;
		}

		ctx.Store.MonitorSlice.ActiveMonitorIndex = idxVal;
	}
}
