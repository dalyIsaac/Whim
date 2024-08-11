namespace Whim.Tests;

public class SavedStateTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given
		SaveStateTransform sut = new();

		// When
		Result<Unit> result = sut.Execute(ctx, internalCtx, rootSector);

		// Then
		Assert.True(result.IsSuccessful);
		ctx.PluginManager.Received(1).SaveState();
	}
}
