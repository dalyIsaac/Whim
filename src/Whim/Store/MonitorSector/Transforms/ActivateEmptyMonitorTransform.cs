namespace Whim;

/// <summary>
/// Activate the monitor even if it's empty.
/// </summary>
/// <param name="Handle">
/// The handle of the monitor to activate.
/// </param>
internal record ActivateEmptyMonitorTransform(HMONITOR Handle) : WhimTransform
{
	internal override WhimResult<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		Result<IMonitor> result = ctx.Store.Pick(PickMonitorByHandle(Handle));
		if (result.Error is not null)
		{
			return Result.FromException<Unit>(result.Error);
		}

		root.MonitorSector.ActiveMonitorHandle = Handle;
		return Unit.Result;
	}
}
