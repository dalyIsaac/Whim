using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim;

public class WindowMinimizeStartedTransformTests
{
	private static (Result<Empty>, Assert.RaisedEvent<WindowMinimizeStartedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMinimizeStartedTransform sut
	)
	{
		Result<Empty>? result = null;
		Assert.RaisedEvent<WindowMinimizeStartedEventArgs> ev;

		ev = Assert.Raises<WindowMinimizeStartedEventArgs>(
			h => mutableRootSector.Windows.WindowMinimizeStarted += h,
			h => mutableRootSector.Windows.WindowMinimizeStarted -= h,
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
