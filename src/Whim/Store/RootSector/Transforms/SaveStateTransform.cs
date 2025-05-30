namespace Whim;

/// <summary>
/// A transform that saves Whim's state.
/// </summary>
public record SaveStateTransform : WhimTransform
{
	internal override WhimResult<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		ctx.PluginManager.SaveState();
		return Unit.Result;
	}
}
