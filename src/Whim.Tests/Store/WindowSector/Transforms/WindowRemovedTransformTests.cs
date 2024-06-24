using System;
using System.Collections.Generic;
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

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

	private static Result<Unit> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowRemovedTransform sut
	)
	{
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<WindowRemovedEventArgs>(
			h => mutableRootSector.WindowSector.WindowRemoved += h,
			h => mutableRootSector.WindowSector.WindowRemoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return result!.Value;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotTracked(IContext ctx, MutableRootSector rootSector, IWindow window)
	{
		// Given the window is not tracked
		WindowRemovedTransform sut = new(window);

		var originalWindows = rootSector.WindowSector.Windows;
		var originalWindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap;

		// When
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then the window is not removed
		Assert.True(result.IsSuccessful);
		Assert.Same(originalWindows, rootSector.WindowSector.Windows);
		Assert.Same(originalWindowWorkspaceMap, rootSector.MapSector.WindowWorkspaceMap);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotInWorkspace(IContext ctx, MutableRootSector rootSector, IWindow window)
	{
		// Given the window is inside the WindowSector but not in the workspace
		window.Handle.Returns((HWND)2);
		rootSector.WindowSector.Windows = rootSector.WindowSector.Windows.Add(window.Handle, window);

		WindowRemovedTransform sut = new(window);

		var originalWindows = rootSector.WindowSector.Windows;
		var originalWindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap;

		// When
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then the window is not removed
		Assert.True(result.IsSuccessful);
		Assert.Same(originalWindows, rootSector.WindowSector.Windows);
		Assert.Same(originalWindowWorkspaceMap, rootSector.MapSector.WindowWorkspaceMap);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector mutableRootSector, IWindow window, IWorkspace workspace)
	{
		// Given the window is inside the WindowSector
		window.Handle.Returns((HWND)2);
		mutableRootSector.WindowSector.Windows = mutableRootSector.WindowSector.Windows.Add(window.Handle, window);

		// ...and inside the MapSector
		Guid workspaceId = Guid.NewGuid();
		workspace.Id.Returns(workspaceId);
		ctx.WorkspaceManager.GetEnumerator().Returns(_ => new List<IWorkspace>() { workspace }.GetEnumerator());

		mutableRootSector.MapSector.WindowWorkspaceMap = mutableRootSector.MapSector.WindowWorkspaceMap.Add(
			window.Handle,
			workspaceId
		);

		WindowRemovedTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);

		Assert.Empty(mutableRootSector.WindowSector.Windows);
		Assert.Empty(mutableRootSector.MapSector.WindowWorkspaceMap);
	}
}
