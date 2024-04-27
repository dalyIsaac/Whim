using System.Collections.Immutable;
using System.Drawing;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;

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
	internal void NoMonitorAtPoint(IContext ctx, IInternalContext internalCtx, IMonitor monitor)
	{
		// Given there is no monitor at the point
		SetMonitorAtPoint(internalCtx, (HMONITOR)2);

		monitor.Handle.Returns((HMONITOR)1);
		ctx.Store.Monitors.Monitors = ImmutableArray.Create(monitor);

		Point<int> point = new(10, 10);
		MouseLeftButtonUpTransform sut = new(point);

		// When we execute the transform
		ctx.Store.Dispatch(sut);

		// The active monitor index doesn't update
		Assert.Equal(-1, ctx.Store.Monitors.ActiveMonitorIndex);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorAtPoint(IContext ctx, IInternalContext internalCtx, IMonitor monitor)
	{
		// Given there is a monitor at the point
		SetMonitorAtPoint(internalCtx, (HMONITOR)2);

		monitor.Handle.Returns((HMONITOR)2);
		ctx.Store.Monitors.Monitors = ImmutableArray.Create(monitor);

		Point<int> point = new(10, 10);
		MouseLeftButtonUpTransform sut = new(point);

		// When we execute the transform
		ctx.Store.Dispatch(sut);

		// The active monitor index updated
		Assert.Equal(0, ctx.Store.Monitors.ActiveMonitorIndex);
	}
}
