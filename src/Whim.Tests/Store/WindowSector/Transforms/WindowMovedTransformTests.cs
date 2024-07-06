using Microsoft.UI.Dispatching;

namespace Whim.Tests;

public class WindowMovedTransformTests
{
	private static void Setup_GetCursorPos(IInternalContext internalCtx)
	{
		internalCtx
			.CoreNativeManager.GetCursorPos(out _)
			.Returns(callInfo =>
			{
				callInfo[0] = new Point<int>(1, 2);
				return (BOOL)true;
			});
	}

	private static (Result<Unit>?, Assert.RaisedEvent<WindowMovedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMovedTransform sut
	)
	{
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowMovedEventArgs> ev;

		ev = Assert.Raises<WindowMovedEventArgs>(
			h => ctx.Store.WindowEvents.WindowMoved += h,
			h => ctx.Store.WindowEvents.WindowMoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result, ev);
	}

	private static Result<Unit> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMovedTransform sut
	)
	{
		Result<Unit>? result = null;

		CustomAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => ctx.Store.WindowEvents.WindowMoved += h,
			h => ctx.Store.WindowEvents.WindowMoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return result!.Value;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsNotMoving_NullProcessFileName(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given the window is not moving and the process file name is null
		mutableRootSector.WindowSector.IsMovingWindow = false;
		window.ProcessFileName.ReturnsNull();
		ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(window).Returns(true);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		var result = AssertDoesNotRaise(ctx, mutableRootSector, sut);

		// Then we get an empty result
		Assert.True(result.IsSuccessful);
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsNotMoving_IsHandledLocation(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given the window is not moving and the window is a handled restoring location window
		mutableRootSector.WindowSector.IsMovingWindow = false;
		mutableRootSector.WindowSector.HandledLocationRestoringWindows = ImmutableHashSet.Create(window.Handle);
		ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(window).Returns(true);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		var result = AssertDoesNotRaise(ctx, mutableRootSector, sut);

		// Then we get an empty result
		Assert.True(result.IsSuccessful);
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsNotMoving_IgnoredByLocationRestoringFilter(
		IContext ctx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given the window is not moving and the window is a handled restoring location window
		mutableRootSector.WindowSector.IsMovingWindow = false;
		ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(window).Returns(false);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		var result = AssertDoesNotRaise(ctx, mutableRootSector, sut);

		// Then we get an empty result
		Assert.True(result.IsSuccessful);
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsNotMoving_GetPos(
		IContext ctx,
		MutableRootSector mutableRootSector,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given the window is not moving, we don't ignore the window moving event, but we can get the pos
		mutableRootSector.WindowSector.IsMovingWindow = false;
		mutableRootSector.WindowSector.WindowMovedDelay = 0;
		mutableRootSector.WindowSector.IsLeftMouseButtonDown = true;
		ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(window).Returns(true);
		Setup_GetCursorPos(internalCtx);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		(Result<Unit>? result, Assert.RaisedEvent<WindowMovedEventArgs> ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then we get a resulting window, a NativeManager.TryEnqueue to restore a window's position, and a second TryEnqueue
		// to dispatch the events.
		ctx.NativeManager.Received(2).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
		Assert.True(result!.Value.IsSuccessful);
		Assert.Equal(new Point<int>(1, 2), ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsMoving_NotGetPos(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given the window is moving, but we don't get the cursor point
		mutableRootSector.WindowSector.IsMovingWindow = true;

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		(Result<Unit>? result, Assert.RaisedEvent<WindowMovedEventArgs> ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then we get no resulting windows, and a TryEnqueue to dispatch the events.
		ctx.NativeManager.Received(1).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
		Assert.True(result!.Value.IsSuccessful);
		Assert.Null(ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsMoving_GetPos(
		IContext ctx,
		MutableRootSector mutableRootSector,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given the window is moving but we get the cursor point
		mutableRootSector.WindowSector.IsMovingWindow = true;
		mutableRootSector.WindowSector.IsLeftMouseButtonDown = true;
		Setup_GetCursorPos(internalCtx);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		(Result<Unit>? result, Assert.RaisedEvent<WindowMovedEventArgs> ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then we get a resulting window, and a TryEnqueue to dispatch the events.
		ctx.NativeManager.Received(1).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
		Assert.True(result!.Value.IsSuccessful);
		Assert.Equal(new Point<int>(1, 2), ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
	}
}
