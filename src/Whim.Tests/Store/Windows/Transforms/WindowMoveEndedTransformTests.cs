using DotNext;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class WindowMoveEndedTransformTests
{
	private static void Setup_GetMovedEdges(IContext ctx, IWindow window)
	{
		IRectangle<int> originalRect = new Rectangle<int>() { Y = 4, Height = 4 };
		IRectangle<int> newRect = new Rectangle<int>() { Y = 4, Height = 3 };

		IWorkspace workspace = Substitute.For<IWorkspace>();
		ctx.Butler.Pantry.GetWorkspaceForWindow(window).Returns(workspace);
		workspace
			.TryGetWindowState(window)
			.Returns(
				new WindowState()
				{
					Rectangle = originalRect,
					Window = window,
					WindowSize = WindowSize.Normal
				}
			);
		ctx.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns(newRect);
	}

	private static (Result<Empty>, Assert.RaisedEvent<WindowMoveEndedEventArgs>) AssertRaises(
		IContext ctx,
		WindowMoveEndedTransform sut
	)
	{
		Result<Empty>? result = null;
		Assert.RaisedEvent<WindowMoveEndedEventArgs> ev;

		ev = Assert.Raises<WindowMoveEndedEventArgs>(
			h => ctx.Store.WindowSlice.WindowMoveEnded += h,
			h => ctx.Store.WindowSlice.WindowMoveEnded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, ev);
	}

	private static Result<Empty> AssertDoesNotRaise(IContext ctx, WindowMoveEndedTransform sut)
	{
		Result<Empty>? result = null;
		CustomAssert.DoesNotRaise<WindowMoveEndedEventArgs>(
			h => ctx.Store.WindowSlice.WindowMoveEnded += h,
			h => ctx.Store.WindowSlice.WindowMoveEnded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return result!.Value;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowIsNotMoving(IContext ctx, IWindow window)
	{
		// Given the window is not moving
		ctx.Store.WindowSlice.IsMovingWindow = false;
		WindowMoveEndedTransform sut = new(window);

		// When we dispatch the transform
		var result = AssertDoesNotRaise(ctx, sut);

		// Then nothing happens
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void EdgesMoved(IContext ctx, IWindow window)
	{
		// Given the window's edges moved
		ctx.Store.WindowSlice.IsMovingWindow = true;
		Setup_GetMovedEdges(ctx, window);
		WindowMoveEndedTransform sut = new(window);

		// When we dispatch the transform
		(var result, var ev) = AssertRaises(ctx, sut);

		// Then an event is raised
		Assert.True(result.IsSuccessful);
		Assert.Null(ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowToPoint(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given the window
		ctx.Store.WindowSlice.IsMovingWindow = true;
		ctx.Butler.Pantry.GetWorkspaceForWindow(window).ReturnsNull();

		internalCtx
			.CoreNativeManager.GetCursorPos(out _)
			.Returns(callInfo =>
			{
				callInfo[0] = new Point<int>(1, 2);
				return (BOOL)true;
			});

		WindowMoveEndedTransform sut = new(window);

		// When we dispatch the transform
		(var result, var ev) = AssertRaises(ctx, sut);

		// Then an event is raised
		Assert.True(result.IsSuccessful);
		Assert.Equal(new Point<int>(1, 2), ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
	}
}
