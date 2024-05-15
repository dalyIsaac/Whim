using DotNext;

namespace Whim;

internal record MouseLeftButtonDownTransform : Transform
{
	/// <summary>
	/// Set the active monitor based on the user's last mouse click.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="mutableRootSector"></param>
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		// TODO: test
		mutableRootSector.WindowSector.IsLeftMouseButtonDown = true;
		return Unit.Result;
	}
}
