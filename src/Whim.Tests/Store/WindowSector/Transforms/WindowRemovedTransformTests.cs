using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim;

public class WindowRemovedTransformTests
{
	private static (Result<Unit>, Assert.RaisedEvent<WindowRemovedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowRemovedTransform sut
	)
	{
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowRemovedEventArgs> ev;

		ev = Assert.Raises<WindowRemovedEventArgs>(
			h => mutableRootSector.WindowSector.WindowRemoved += h,
			h => mutableRootSector.WindowSector.WindowRemoved -= h,
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
		// Given the window is inside the sector
		window.Handle.Returns((HWND)2);
		mutableRootSector.WindowSector.Windows = mutableRootSector.WindowSector.Windows.Add(window.Handle, window);

		WindowRemovedTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
		internalCtx
			.ButlerEventHandlers.Received(1)
			.OnWindowRemoved(Arg.Is<WindowRemovedEventArgs>(a => a.Window == window));
	}
}
