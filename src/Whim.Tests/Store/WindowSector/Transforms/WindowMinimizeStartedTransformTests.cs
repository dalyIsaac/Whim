using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class WindowMinimizeStartedTransformTests
{
	private static (Result<Unit>, Assert.RaisedEvent<WindowMinimizeStartedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMinimizeStartedTransform sut
	)
	{
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowMinimizeStartedEventArgs> ev;

		ev = Assert.Raises<WindowMinimizeStartedEventArgs>(
			h => mutableRootSector.WindowSector.WindowMinimizeStarted += h,
			h => mutableRootSector.WindowSector.WindowMinimizeStarted -= h,
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
		WindowMinimizeStartedTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
		internalCtx
			.ButlerEventHandlers.Received(1)
			.OnWindowMinimizeStart(Arg.Is<WindowMinimizeStartedEventArgs>(a => a.Window == window));
	}
}
