namespace Whim.Tests;

public class MouseLeftButtonDownTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector mutableRootSector)
	{
		// Given the left mouse button is down
		mutableRootSector.WindowSector.IsLeftMouseButtonDown = true;

		MouseLeftButtonDownTransform sut = new();

		// When
		var result = ctx.Store.WhimDispatch(sut);

		// Then the left mouse button is longer down
		Assert.True(result.IsSuccessful);
		Assert.True(mutableRootSector.WindowSector.IsLeftMouseButtonDown);
	}
}
