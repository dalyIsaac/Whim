namespace Whim;

/// <summary>
/// A transform that saves Whim's state.
/// </summary>
public record SaveStateTransform : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		ctx.PluginManager.SaveState();
		return Unit.Result;
	}
}
