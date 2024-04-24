using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim;

public class WindowRemovedTransformTests
{
	private static (Result<Empty>, Assert.RaisedEvent<WindowRemovedEventArgs>) AssertRaises(
		IContext ctx,
		WindowRemovedTransform sut
	)
	{
		Result<Empty>? result = null;
		Assert.RaisedEvent<WindowRemovedEventArgs> ev;

		ev = Assert.Raises<WindowRemovedEventArgs>(
			h => ctx.Store.WindowSlice.WindowRemoved += h,
			h => ctx.Store.WindowSlice.WindowRemoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, ev);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		WindowRemovedTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
		internalCtx
			.ButlerEventHandlers.Received(1)
			.OnWindowRemoved(Arg.Is<WindowRemovedEventArgs>(a => a.Window == window));
	}
}
