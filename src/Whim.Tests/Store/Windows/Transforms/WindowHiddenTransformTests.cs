using DotNext;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Whim.TestUtils;
using Xunit;

namespace Whim;

public class WindowHiddenTransformTests
{
	private static Result<Empty> AssertDoesNotRaise(IContext ctx, WindowHiddenTransform sut)
	{
		Result<Empty>? result = null;

		CustomAssert.DoesNotRaise<WindowRemovedEventArgs>(
			h => ctx.Store.WindowSlice.WindowRemoved += h,
			h => ctx.Store.WindowSlice.WindowRemoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return result!.Value;
	}

	private static (Result<Empty>, Assert.RaisedEvent<WindowRemovedEventArgs>) AssertRaises(
		IContext ctx,
		WindowHiddenTransform sut
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
	internal void Failed(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		ctx.Butler.Pantry.GetMonitorForWindow(window).ReturnsNull();
		WindowHiddenTransform sut = new(window);

		// When
		var result = AssertDoesNotRaise(ctx, sut);

		// Then
		Assert.True(result.IsSuccessful);
		internalCtx.ButlerEventHandlers.DidNotReceive().OnWindowRemoved(Arg.Any<WindowRemovedEventArgs>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, IInternalContext internalCtx, IWindow window, IMonitor monitor)
	{
		// Given
		ctx.Butler.Pantry.GetMonitorForWindow(window).Returns(monitor);
		WindowHiddenTransform sut = new(window);

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
