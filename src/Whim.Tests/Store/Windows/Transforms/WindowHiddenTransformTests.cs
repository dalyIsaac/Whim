using DotNext;
using Whim.TestUtils;
using Xunit;

namespace Whim;

public class WindowHiddenTransformTests
{
	private static Result<Empty> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowHiddenTransform sut
	)
	{
		Result<Empty>? result = null;

		CustomAssert.DoesNotRaise<WindowRemovedEventArgs>(
			h => mutableRootSector.Windows.WindowRemoved += h,
			h => mutableRootSector.Windows.WindowRemoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return result!.Value;
	}

	private static (Result<Empty>, Assert.RaisedEvent<WindowRemovedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector mutableRootSector,
		WindowHiddenTransform sut
	)
	{
		Result<Empty>? result = null;
		Assert.RaisedEvent<WindowRemovedEventArgs> ev;

		ev = Assert.Raises<WindowRemovedEventArgs>(
			h => mutableRootSector.Windows.WindowRemoved += h,
			h => mutableRootSector.Windows.WindowRemoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, ev);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Failed(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowHiddenTransform sut = new(window);

		// When
		var result = AssertDoesNotRaise(ctx, mutableRootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(
		IContext ctx,
		MutableRootSector mutableRootSector,
		IMonitor monitor,
		IWorkspace workspace,
		IWindow window
	)
	{
		// Given
		mutableRootSector.Maps.MonitorWorkspaceMap = mutableRootSector.Maps.MonitorWorkspaceMap.SetItem(
			monitor,
			workspace
		);
		mutableRootSector.Maps.WindowWorkspaceMap = mutableRootSector.Maps.WindowWorkspaceMap.SetItem(
			window,
			workspace
		);

		WindowHiddenTransform sut = new(window);

		// When
		(var result, var ev) = AssertRaises(ctx, mutableRootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, ev.Arguments.Window);
	}
}
