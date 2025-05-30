namespace Whim.Tests;

public class WindowMoveStartedTransformTests
{
	private static (Result<Unit>, Assert.RaisedEvent<WindowMoveStartedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMoveStartedTransform sut
	)
	{
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowMoveStartedEventArgs> ev;

		ev = Assert.Raises<WindowMoveStartedEventArgs>(
			h => mutableRootSector.WindowSector.WindowMoveStarted += h,
			h => mutableRootSector.WindowSector.WindowMoveStarted -= h,
			() => result = ctx.Store.WhimDispatch(sut)
		);

		return (result!.Value, ev);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LeftMouseButtonNotDown(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given IsLeftMouseButtonDown is false
		WindowMoveStartedTransform sut = new(window);

		// When we dispatch the transform
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then an event is raised, but with no cursor
		Assert.True(result.IsSuccessful);
		Assert.Null(ev.Arguments.CursorDraggedPoint);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LeftMouseButtonDown(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given IsLeftMouseButtonDown is true
		mutableRootSector.WindowSector.IsLeftMouseButtonDown = true;
		WindowMoveStartedTransform sut = new(window);

		// When we dispatch the transform
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then an event is raised, but with no cursor
		Assert.True(result.IsSuccessful);
		Assert.Null(ev.Arguments.CursorDraggedPoint);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetCursorPos(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given IsLeftMouseButtonDown is true, and the cursor position can be acquired
		mutableRootSector.WindowSector.IsLeftMouseButtonDown = true;
		WindowMoveStartedTransform sut = new(window);

		internalCtx
			.CoreNativeManager.GetCursorPos(out _)
			.Returns(callInfo =>
			{
				callInfo[0] = new Point<int>(1, 2);
				return (BOOL)true;
			});

		// When we dispatch the transform
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then an event is raised, but with no cursor
		Assert.True(result.IsSuccessful);
		Assert.Equal(new Point<int>(1, 2), ev.Arguments.CursorDraggedPoint);
	}
}
