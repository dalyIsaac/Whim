using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim;

public class WindowMinimizeEndedTransformTests
{
	private static (Result<Empty>, Assert.RaisedEvent<WindowMinimizeEndedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMinimizeEndedTransform sut
	)
	{
		Result<Empty>? result = null;
		Assert.RaisedEvent<WindowMinimizeEndedEventArgs> ev;

		ev = Assert.Raises<WindowMinimizeEndedEventArgs>(
			h => mutableRootSector.Windows.WindowMinimizeEnded += h,
			h => mutableRootSector.Windows.WindowMinimizeEnded -= h,
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
