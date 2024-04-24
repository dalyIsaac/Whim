using System.Collections.Immutable;
using DotNext;
using Microsoft.UI.Dispatching;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

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

	private static (Result<Empty>?, Assert.RaisedEvent<WindowMovedEventArgs>) AssertRaises(
		IContext ctx,
		WindowMovedTransform sut
	)
	{
		Result<Empty>? result = null;
		Assert.RaisedEvent<WindowMovedEventArgs> ev;

		ev = Assert.Raises<WindowMovedEventArgs>(
			h => ctx.Store.WindowSlice.WindowMoved += h,
			h => ctx.Store.WindowSlice.WindowMoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result, ev);
	}

	private static Result<Empty> AssertDoesNotRaise(IContext ctx, WindowMovedTransform sut)
	{
		Result<Empty>? result = null;

		CustomAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => ctx.Store.WindowSlice.WindowMoved += h,
			h => ctx.Store.WindowSlice.WindowMoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return result!.Value;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsNotMoving_NullProcessFileName(IContext ctx, IWindow window)
	{
		// Given the window is not moving and the process file name is null
		ctx.Store.WindowSlice.IsMovingWindow = false;
		window.ProcessFileName.ReturnsNull();
		ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(window).Returns(true);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		var result = AssertDoesNotRaise(ctx, sut);

		// Then we get an empty result
		Assert.True(result.IsSuccessful);
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsNotMoving_IsHandledLocation(IContext ctx, IWindow window)
	{
		// Given the window is not moving and the window is a handled restoring location window
		ctx.Store.WindowSlice.IsMovingWindow = false;
		ctx.Store.WindowSlice.HandledLocationRestoringWindows = ImmutableHashSet.Create(window);
		ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(window).Returns(true);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		var result = AssertDoesNotRaise(ctx, sut);

		// Then we get an empty result
		Assert.True(result.IsSuccessful);
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsNotMoving_IgnoredByLocationRestoringFilter(IContext ctx, IWindow window)
	{
		// Given the window is not moving and the window is a handled restoring location window
		ctx.Store.WindowSlice.IsMovingWindow = false;
		ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(window).Returns(false);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		var result = AssertDoesNotRaise(ctx, sut);

		// Then we get an empty result
		Assert.True(result.IsSuccessful);
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsNotMoving_GetPos(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given the window is not moving, we don't ignore the window moving event, but we can get the pos
		NativeManagerUtils.SetupTryEnqueue(ctx);
		ctx.Store.WindowSlice.IsMovingWindow = false;
		ctx.Store.WindowSlice.WindowMovedDelay = 0;
		ctx.Store.WindowSlice.IsLeftMouseButtonDown = true;
		ctx.WindowManager.LocationRestoringFilterManager.ShouldBeIgnored(window).Returns(true);
		Setup_GetCursorPos(internalCtx);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		(Result<Empty>? result, Assert.RaisedEvent<WindowMovedEventArgs> ev) = AssertRaises(ctx, sut);

		// Then we get a result
		ctx.NativeManager.Received(1).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
		Assert.True(result!.Value.IsSuccessful);
		Assert.Equal(new Point<int>(1, 2), ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
		ctx.Butler.Pantry.Received(2).GetWorkspaceForWindow(window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsMoving_NotGetPos(IContext ctx, IWindow window)
	{
		// Given the window is moving, but we don't get the cursor point
		ctx.Store.WindowSlice.IsMovingWindow = true;

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		(Result<Empty>? result, Assert.RaisedEvent<WindowMovedEventArgs> ev) = AssertRaises(ctx, sut);

		// Then
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
		Assert.True(result!.Value.IsSuccessful);
		Assert.Null(ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IsMoving_GetPos(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given the window is moving but we get the cursor point
		ctx.Store.WindowSlice.IsMovingWindow = true;
		ctx.Store.WindowSlice.IsLeftMouseButtonDown = true;
		Setup_GetCursorPos(internalCtx);

		WindowMovedTransform sut = new(window);

		// When we dispatch the transform
		(Result<Empty>? result, Assert.RaisedEvent<WindowMovedEventArgs> ev) = AssertRaises(ctx, sut);

		// Then
		ctx.NativeManager.DidNotReceive().TryEnqueue(Arg.Any<DispatcherQueueHandler>());
		Assert.True(result!.Value.IsSuccessful);
		Assert.Equal(new Point<int>(1, 2), ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
	}
}
