using System.Collections.Immutable;
using System.Drawing;
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Tests;

public class GetMonitorAtPointPickerTests
{
	/// <summary>
	/// Set up the monitor retrieved from the CoreNativeManager, and the handle of the currently
	/// tracked monitor.
	/// </summary>
	/// <param name="mutableRootSector"></param>
	/// <param name="internalCtx"></param>
	/// <param name="foundMonitorHandle">The handle of the monitor at the point.</param>
	/// <param name="monitorHandle">The handle of the monitor which the store currently tracks.</param>
	/// <param name="monitor"></param>
	private static void SetMonitorAtPoint(
		MutableRootSector mutableRootSector,
		IInternalContext internalCtx,
		HMONITOR foundMonitorHandle,
		HMONITOR monitorHandle,
		IMonitor monitor
	)
	{
		internalCtx
			.CoreNativeManager.MonitorFromPoint(Arg.Any<Point>(), Arg.Any<MONITOR_FROM_FLAGS>())
			.Returns(foundMonitorHandle);

		monitor.Handle.Returns(monitorHandle);
		mutableRootSector.Monitors.Monitors = ImmutableArray.Create(monitor);
	}

	private static readonly IPoint<int> _point = new Point<int>(10, 10);

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Index_CannotFindMonitor(IContext ctx, IInternalContext internalCtx, MutableRootSector mutableRootSector, IMonitor monitor)
	{
		// Given there is no monitor at the point
		SetMonitorAtPoint(mutableRootSector, internalCtx, (HMONITOR)2, (HMONITOR)1, monitor);
		GetMonitorIndexAtPointPicker sut = new(_point);

		// When we try get the monitor at said point
		Result<int> result = ctx.Store.Pick(sut);

		// Then we don't get the value
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Index_CannotFindMonitor_GetFirst(IContext ctx, IInternalContext internalCtx, MutableRootSector mutableRootSector, IMonitor monitor)
	{
		// Given there is no monitor at the point
		SetMonitorAtPoint(mutableRootSector, internalCtx, (HMONITOR)2, (HMONITOR)1, monitor);
		GetMonitorIndexAtPointPicker sut = new(_point, true);

		// When we try get the monitor at said point
		Result<int> result = ctx.Store.Pick(sut);

		// Then we get the first monitor
		Assert.Equal(0, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Index_FoundMonitor(IContext ctx, IInternalContext internalCtx, MutableRootSector mutableRootSector, IMonitor monitor)
	{
		// Given there is a monitor at the point
		SetMonitorAtPoint(mutableRootSector, internalCtx, (HMONITOR)1, (HMONITOR)1, monitor);
		GetMonitorIndexAtPointPicker sut = new(_point);

		// When we try get the monitor at said point
		Result<int> result = ctx.Store.Pick(sut);

		// Then we get it
		Assert.Equal(0, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Monitor_CannotFindMonitor(IContext ctx, IInternalContext internalCtx, MutableRootSector mutableRootSector, IMonitor monitor)
	{
		// Given there is no monitor at the point
		SetMonitorAtPoint(mutableRootSector, internalCtx, (HMONITOR)2, (HMONITOR)1, monitor);
		GetMonitorAtPointPicker sut = new(_point);

		// When we try get the monitor at said point
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then we don't get the value
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Monitor_CannotFindMonitor_GetFirst(IContext ctx, IInternalContext internalCtx, MutableRootSector mutableRootSector, IMonitor monitor)
	{
		// Given there is no monitor at the point
		SetMonitorAtPoint(mutableRootSector, internalCtx, (HMONITOR)2, (HMONITOR)1, monitor);
		GetMonitorAtPointPicker sut = new(_point, true);

		// When we try get the monitor at said point
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then we get the first monitor
		Assert.Equal(monitor, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Monitor_FoundMonitor(IContext ctx, IInternalContext internalCtx, MutableRootSector mutableRootSector, IMonitor monitor)
	{
		// Given there is a monitor at the point
		SetMonitorAtPoint(mutableRootSector, internalCtx, (HMONITOR)1, (HMONITOR)1, monitor);
		GetMonitorAtPointPicker sut = new(_point);

		// When we try get the monitor at said point
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then we get it
		Assert.Equal(monitor, result.Value);
	}
}
