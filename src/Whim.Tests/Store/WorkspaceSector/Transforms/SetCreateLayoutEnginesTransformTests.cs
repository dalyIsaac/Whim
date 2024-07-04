namespace Whim.Tests;

public class SetCreateLayoutEnginesTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void CreateLayoutEnginesTransform(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine engine1,
		ILayoutEngine engine2
	)
	{
		// Given
		Func<CreateLeafLayoutEngine[]> createLayoutEnginesFn = () =>
			new CreateLeafLayoutEngine[] { (id) => engine1, (id) => engine2 };

		// When
		var result = ctx.Store.Dispatch(new SetCreateLayoutEnginesTransform(createLayoutEnginesFn));

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Same(createLayoutEnginesFn, root.WorkspaceSector.CreateLayoutEngines);
	}
}
