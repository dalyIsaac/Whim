using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim;

public class WindowMinimizeEndedTransformTests
{
	private static (Result<Empty>, Assert.RaisedEvent<WindowMinimizeEndedEventArgs>) AssertRaises(
		IContext ctx,
		WindowMinimizeEndedTransform sut
	)
	{
		Result<Empty>? result = null;
		Assert.RaisedEvent<WindowMinimizeEndedEventArgs> ev;

		ev = Assert.Raises<WindowMinimizeEndedEventArgs>(
			h => ctx.Store.WindowSlice.WindowMinimizeEnded += h,
			h => ctx.Store.WindowSlice.WindowMinimizeEnded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, ev);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		WindowMinimizeEndedTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
		internalCtx
			.ButlerEventHandlers.Received(1)
			.OnWindowMinimizeEnd(Arg.Is<WindowMinimizeEndedEventArgs>(a => a.Window == window));
	}
}
