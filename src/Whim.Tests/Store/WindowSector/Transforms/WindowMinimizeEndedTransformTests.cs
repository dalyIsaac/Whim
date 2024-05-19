<<<<<<< HEAD
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class WindowMinimizeEndedTransformTests
{
	private static (Result<Unit>, Assert.RaisedEvent<WindowMinimizeEndedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMinimizeEndedTransform sut
	)
	{
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowMinimizeEndedEventArgs> ev;

		ev = Assert.Raises<WindowMinimizeEndedEventArgs>(
			h => mutableRootSector.WindowSector.WindowMinimizeEnded += h,
			h => mutableRootSector.WindowSector.WindowMinimizeEnded -= h,
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
=======
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class WindowMinimizeEndedTransformTests
{
	private static (Result<Unit>, Assert.RaisedEvent<WindowMinimizeEndedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowMinimizeEndedTransform sut
	)
	{
		Result<Unit>? result = null;
		Assert.RaisedEvent<WindowMinimizeEndedEventArgs> ev;

		ev = Assert.Raises<WindowMinimizeEndedEventArgs>(
			h => mutableRootSector.WindowSector.WindowMinimizeEnded += h,
			h => mutableRootSector.WindowSector.WindowMinimizeEnded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, ev);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForWindow(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowMinimizeEndedTransform sut = new(window);

		// When
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<WindowMinimizeEndedEventArgs>(
			h => mutableRootSector.WindowSector.WindowMinimizeEnded += h,
			h => mutableRootSector.WindowSector.WindowMinimizeEnded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		// Then
		Assert.False(result!.Value.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector, IWorkspace workspace, IWindow window)
	{
		// Given the window is in a workspace
		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.Add(
			window.Handle,
			workspace.Id
		);

		WindowMinimizeEndedTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
		workspace.Received(1).MinimizeWindowEnd(window);
		workspace.Received(1).DoLayout();
	}
}
>>>>>>> 305c778 (Remove Butler tests)
