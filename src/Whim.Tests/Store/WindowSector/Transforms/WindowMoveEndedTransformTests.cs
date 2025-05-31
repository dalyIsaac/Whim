namespace Whim.Tests;

public class WindowMoveEndedTransformTests
{
	private static void Setup_GetMovedEdges(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
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
					WindowSize = WindowSize.Normal,
				}
			);
		ctx.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns(newRect);
	}

	private static (Result<Unit>, Assert.RaisedEvent<WindowMoveEndedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMoveEndedTransform sut
	)
	{
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowMoveEndedEventArgs> ev;

		ev = Assert.Raises<WindowMoveEndedEventArgs>(
			h => mutableRootSector.WindowSector.WindowMoveEnded += h,
			h => mutableRootSector.WindowSector.WindowMoveEnded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, ev);
	}

	private static Result<Unit> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMoveEndedTransform sut
	)
	{
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<WindowMoveEndedEventArgs>(
			h => mutableRootSector.WindowSector.WindowMoveEnded += h,
			h => mutableRootSector.WindowSector.WindowMoveEnded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return result!.Value;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowIsNotMoving(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given the window is not moving
		mutableRootSector.WindowSector.IsMovingWindow = false;
		WindowMoveEndedTransform sut = new(window);

		// When we dispatch the transform
		var result = AssertDoesNotRaise(ctx, mutableRootSector, sut);

		// Then nothing happens
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void EdgesMoved(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given the window's edges moved
		mutableRootSector.WindowSector.IsMovingWindow = true;
		Setup_GetMovedEdges(ctx, mutableRootSector, window);
		WindowMoveEndedTransform sut = new(window);

		// When we dispatch the transform
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then an event is raised
		Assert.True(result.IsSuccessful);
		Assert.Null(ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowToPoint(
		IContext ctx,
		MutableRootSector mutableRootSector,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given the window
		mutableRootSector.WindowSector.IsMovingWindow = true;
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
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then an event is raised
		Assert.True(result.IsSuccessful);
		Assert.Equal(new Point<int>(1, 2), ev.Arguments.CursorDraggedPoint);
		Assert.Equal(window, ev.Arguments.Window);
	}
}
