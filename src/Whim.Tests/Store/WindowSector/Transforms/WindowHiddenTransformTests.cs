using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WindowHiddenTransformTests
{
	private static Result<Unit> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowHiddenTransform sut
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

	private static (Result<Unit>, Assert.RaisedEvent<WindowRemovedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowHiddenTransform sut
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

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Failed(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		ctx.Butler.Pantry.GetMonitorForWindow(window).ReturnsNull();
		WindowHiddenTransform sut = new(window);

		// When
		var result = AssertDoesNotRaise(ctx, mutableRootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector)
	{
		// Given the window is inside the window sector,
		IWindow window = CreateWindow((HWND)2);
		IMonitor monitor = CreateMonitor((HMONITOR)3);

		Workspace workspace = CreateWorkspace(ctx);
		PopulateThreeWayMap(ctx, rootSector, monitor, workspace, window);

		WindowHiddenTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
	}
}
