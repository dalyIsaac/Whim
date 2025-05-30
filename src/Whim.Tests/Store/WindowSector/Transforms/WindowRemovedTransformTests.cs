using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
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
			() => result = ctx.Store.WhimDispatch(sut)
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
			() => result = ctx.Store.WhimDispatch(sut)
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
	internal void Success(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given the window is tracked
		window.Handle.Returns((HWND)2);
		Workspace workspace = CreateWorkspace(ctx);
		PopulateWindowWorkspaceMap(ctx, mutableRootSector, window, workspace);

		mutableRootSector.MapSector.WindowWorkspaceMap = mutableRootSector.MapSector.WindowWorkspaceMap.Add(
			window.Handle,
			workspace.Id
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
