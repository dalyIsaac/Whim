// using DotNext;
// using NSubstitute;
// using Whim.TestUtils;
// using Xunit;
//
// namespace Whim.Tests;
//
// public class WindowMinimizeStartedTransformTests
// {
// 	private static (Result<Unit>, Assert.RaisedEvent<WindowMinimizeStartedEventArgs>) AssertRaises(
// 		IContext ctx,
// 		MutableRootSector mutableRootSector,
// 		WindowMinimizeStartedTransform sut
// 	)
// 	{
// 		Result<Unit>? result = null;
// 		Assert.RaisedEvent<WindowMinimizeStartedEventArgs> ev;
//
// 		ev = Assert.Raises<WindowMinimizeStartedEventArgs>(
// 			h => mutableRootSector.WindowSector.WindowMinimizeStarted += h,
// 			h => mutableRootSector.WindowSector.WindowMinimizeStarted -= h,
// 			() => result = ctx.Store.Dispatch(sut)
// 		);
//
// 		return (result!.Value, ev);
// 	}
//
// 	[Theory, AutoSubstituteData<StoreCustomization>]
// 	internal void NoWorkspaceForWindow(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
// 	{
// 		// Given
// 		WindowMinimizeStartedTransform sut = new(window);
//
// 		// When
// 		Result<Unit>? result = null;
// 		CustomAssert.DoesNotRaise<WindowMinimizeStartedEventArgs>(
// 			h => mutableRootSector.WindowSector.WindowMinimizeStarted += h,
// 			h => mutableRootSector.WindowSector.WindowMinimizeStarted -= h,
// 			() => result = ctx.Store.Dispatch(sut)
// 		);
//
// 		// Then
// 		Assert.False(result!.Value.IsSuccessful);
// 	}
//
// 	[Theory, AutoSubstituteData<StoreCustomization>]
// 	internal void Success(IContext ctx, MutableRootSector rootSector, IWindow window, Workspace workspace)
// 	{
// 		// Given the window is in a workspace
// 		StoreTestUtils.SetupWindowWorkspaceMapping(ctx, rootSector, window, workspace);
// 		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.Add(
// 			window.Handle,
// 			workspace.Id
// 		);
//
// 		WindowMinimizeStartedTransform sut = new(window);
//
// 		// When
// 		(var result, var ev) = AssertRaises(ctx, rootSector, sut);
//
// 		// Then
// 		Assert.True(result.IsSuccessful);
// 		Assert.Equal(window, ev.Arguments.Window);
// 		workspace.Received(1).MinimizeWindowStart(window);
// 		workspace.Received(1).DoLayout();
// 	}
// }
