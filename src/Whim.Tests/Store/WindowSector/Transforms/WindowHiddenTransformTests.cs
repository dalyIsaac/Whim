// using System;
// using System.Collections.Generic;
// using DotNext;
// using NSubstitute;
// using NSubstitute.ReturnsExtensions;
// using Whim.TestUtils;
// using Windows.Win32.Foundation;
// using Xunit;
//
// namespace Whim.Tests;
//
// public class WindowHiddenTransformTests
// {
// 	private static Result<Unit> AssertDoesNotRaise(
// 		IContext ctx,
// 		MutableRootSector mutableRootSector,
// 		WindowHiddenTransform sut
// 	)
// 	{
// 		Result<Unit>? result = null;
//
// 		CustomAssert.DoesNotRaise<WindowRemovedEventArgs>(
// 			h => mutableRootSector.WindowSector.WindowRemoved += h,
// 			h => mutableRootSector.WindowSector.WindowRemoved -= h,
// 			() => result = ctx.Store.Dispatch(sut)
// 		);
//
// 		return result!.Value;
// 	}
//
// 	private static (Result<Unit>, Assert.RaisedEvent<WindowRemovedEventArgs>) AssertRaises(
// 		IContext ctx,
// 		MutableRootSector mutableRootSector,
// 		WindowHiddenTransform sut
// 	)
// 	{
// 		Result<Unit>? result = null;
// 		Assert.RaisedEvent<WindowRemovedEventArgs> ev;
//
// 		ev = Assert.Raises<WindowRemovedEventArgs>(
// 			h => mutableRootSector.WindowSector.WindowRemoved += h,
// 			h => mutableRootSector.WindowSector.WindowRemoved -= h,
// 			() => result = ctx.Store.Dispatch(sut)
// 		);
//
// 		return (result!.Value, ev);
// 	}
//
// 	[Theory, AutoSubstituteData<StoreCustomization>]
// 	internal void Failed(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
// 	{
// 		// Given
// 		ctx.Butler.Pantry.GetMonitorForWindow(window).ReturnsNull();
// 		WindowHiddenTransform sut = new(window);
//
// 		// When
// 		var result = AssertDoesNotRaise(ctx, mutableRootSector, sut);
//
// 		// Then
// 		Assert.True(result.IsSuccessful);
// 	}
//
// 	[Theory, AutoSubstituteData<StoreCustomization>]
// 	internal void Success(
// 		IContext ctx,
// 		MutableRootSector rootSector,
// 		IWindow window,
// 		IMonitor monitor,
// 		IWorkspace workspace
// 	)
// 	{
// 		// Given the window is inside the window sector,
// 		window.Handle.Returns((HWND)2);
// 		rootSector.WindowSector.Windows = rootSector.WindowSector.Windows.Add(window.Handle, window);
//
// 		// and has a monitor - window mapping
// 		Guid workspaceId = Guid.NewGuid();
// 		workspace.Id.Returns(workspaceId);
//
// 		MapSector mapSector = rootSector.MapSector;
// 		mapSector.WindowWorkspaceMap = mapSector.WindowWorkspaceMap.SetItem(window.Handle, workspaceId);
// 		mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(monitor.Handle, workspaceId);
//
// 		ctx.WorkspaceManager.GetEnumerator().Returns(_ => new List<IWorkspace>() { workspace }.GetEnumerator());
//
// 		WindowHiddenTransform sut = new(window);
//
// 		// When
// 		(var result, var ev) = AssertRaises(ctx, rootSector, sut);
//
// 		// Then
// 		Assert.True(result.IsSuccessful);
// 		Assert.Equal(window, ev.Arguments.Window);
// 	}
// }
