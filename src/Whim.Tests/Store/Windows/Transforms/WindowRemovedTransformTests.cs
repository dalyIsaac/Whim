using DotNext;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class WindowRemovedTransformTests
{
	private static (Result<Empty>, Assert.RaisedEvent<WindowRemovedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowRemovedTransform sut
	)
	{
		Result<Empty>? result = null;
		Assert.RaisedEvent<WindowRemovedEventArgs> ev;

		ev = Assert.Raises<WindowRemovedEventArgs>(
			h => mutableRootSector.Windows.WindowRemoved += h,
			h => mutableRootSector.Windows.WindowRemoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, ev);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowRemovedTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
	}
}
