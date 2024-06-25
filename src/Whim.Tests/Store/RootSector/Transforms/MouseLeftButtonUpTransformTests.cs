using System.Drawing;

namespace Whim.Tests;

public class MouseLeftButtonUpTransformTests
{
	private static void SetMonitorAtPoint(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		internalCtx
			.CoreNativeManager.MonitorFromPoint(Arg.Any<Point>(), Arg.Any<MONITOR_FROM_FLAGS>())
			.Returns(hmonitor);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoMonitorAtPoint(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IMonitor monitor
	)
	{
		// Given there is no monitor at the point
		SetMonitorAtPoint(internalCtx, (HMONITOR)2);

		monitor.Handle.Returns((HMONITOR)1);
		mutableRootSector.MonitorSector.Monitors = ImmutableArray.Create(monitor);

		Point<int> point = new(10, 10);
		MouseLeftButtonUpTransform sut = new(point);

		// When we execute the transform
		ctx.Store.Dispatch(sut);

		// The active monitor didn't update
		Assert.Equal((HMONITOR)0, mutableRootSector.MonitorSector.ActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorAtPoint(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IMonitor monitor
	)
	{
		// Given there is a monitor at the point
		SetMonitorAtPoint(internalCtx, (HMONITOR)2);

		monitor.Handle.Returns((HMONITOR)2);
		mutableRootSector.MonitorSector.Monitors = ImmutableArray.Create(monitor);

		Point<int> point = new(10, 10);
		MouseLeftButtonUpTransform sut = new(point);

		// When we execute the transform
		ctx.Store.Dispatch(sut);

		// The active monitor updated
		Assert.Equal(monitor.Handle, mutableRootSector.MonitorSector.ActiveMonitorHandle);
	}
}
