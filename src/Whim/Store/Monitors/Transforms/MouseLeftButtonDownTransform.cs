using DotNext;

namespace Whim;

internal record MouseLeftButtonDownTransform : Transform
{
	/// <summary>
	/// Set the active monitor based on the user's last mouse click.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx)
	{
		ctx.Store.WindowSlice.IsLeftMouseButtonDown = false;
		return Empty.Result;
	}
}
