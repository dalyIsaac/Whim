namespace Whim;

internal record MouseLeftButtonDownTransform : WhimTransform
{
	/// <summary>
	/// Set the active monitor based on the user's last mouse click.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="mutableRootSector"></param>
	internal override WhimResult<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		mutableRootSector.WindowSector.IsLeftMouseButtonDown = true;
		return Unit.Result;
	}
}
