using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class WindowMinimizeEndedTransformTests
{
	private static (Result<Unit>, Assert.RaisedEvent<WindowMinimizeEndedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMinimizeEndedTransform sut
	)
	{
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowMinimizeEndedEventArgs> ev;

		ev = Assert.Raises<WindowMinimizeEndedEventArgs>(
			h => mutableRootSector.WindowSector.WindowMinimizeEnded += h,
			h => mutableRootSector.WindowSector.WindowMinimizeEnded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, ev);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given
		WindowMinimizeEndedTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
		internalCtx
			.ButlerEventHandlers.Received(1)
			.OnWindowMinimizeEnd(Arg.Is<WindowMinimizeEndedEventArgs>(a => a.Window == window));
	}
}
