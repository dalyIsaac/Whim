using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class MouseLeftButtonDownTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx)
	{
		// Given the left mouse button is down
		ctx.Store.WindowSlice.IsLeftMouseButtonDown = true;

		MouseLeftButtonDownTransform sut = new();

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then the left mouse button is no longer down
		Assert.True(result.IsSuccessful);
		Assert.False(ctx.Store.WindowSlice.IsLeftMouseButtonDown);
	}
}
