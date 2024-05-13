using System.Collections.Immutable;
using System.Drawing;
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim;

public class MonitorPickersTests
{
	private static void PopulateMonitors(MutableRootSector mutableRootSector)
	{
		IMonitor monitor1 = Substitute.For<IMonitor>();
		monitor1.Handle.Returns((HMONITOR)1);

		IMonitor monitor2 = Substitute.For<IMonitor>();
		monitor2.Handle.Returns((HMONITOR)2);

		IMonitor monitor3 = Substitute.For<IMonitor>();
		monitor3.Handle.Returns((HMONITOR)3);

		IMonitor monitor4 = Substitute.For<IMonitor>();
		monitor4.Handle.Returns((HMONITOR)4);

		mutableRootSector.MonitorSector.Monitors = ImmutableArray.Create(monitor1, monitor2, monitor3, monitor4);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetMonitorByHandle_Failure(IContext ctx, MutableRootSector mutableRootSector)
	{
		// Given the handle does not exist in the monitors
		PopulateMonitors(mutableRootSector);
		HMONITOR monitorHandle = (HMONITOR)40;

		// When we get the monitor
		Result<IMonitor> result = ctx.Store.Pick(Pickers.GetMonitorByHandle(monitorHandle));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetMonitorByHandle_Success(IContext ctx, MutableRootSector mutableRootSector)
	{
		// Given the handle exists in the monitors
		PopulateMonitors(mutableRootSector);
		HMONITOR monitorHandle = (HMONITOR)2;

		// When we get the monitor
		Result<IMonitor> result = ctx.Store.Pick(Pickers.GetMonitorByHandle(monitorHandle));

		// Then we get the monitor
		Assert.True(result.IsSuccessful);
		Assert.Equal(monitorHandle, result.Value.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetActiveMonitor(IContext ctx, MutableRootSector mutableRootSector)
	{
		// Given there is an active monitor
		PopulateMonitors(mutableRootSector);
		mutableRootSector.MonitorSector.ActiveMonitorHandle = (HMONITOR)2;

		// When we get the monitor
		IMonitor result = ctx.Store.Pick(Pickers.GetActiveMonitor());

		// Then we get the monitor
		Assert.Equal((HMONITOR)2, result.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetPrimaryMonitor(IContext ctx, MutableRootSector mutableRootSector)
	{
		// Given there is a primary monitor
		PopulateMonitors(mutableRootSector);
		mutableRootSector.MonitorSector.PrimaryMonitorHandle = (HMONITOR)1;

		// When we get the monitor
		IMonitor result = ctx.Store.Pick(Pickers.GetPrimaryMonitor());

		// Then we get the monitor
		Assert.Equal((HMONITOR)1, result.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetLastWhimActiveMonitor(IContext ctx, MutableRootSector mutableRootSector)
	{
		// Given there is an active monitor
		PopulateMonitors(mutableRootSector);
		mutableRootSector.MonitorSector.LastWhimActiveMonitorHandle = (HMONITOR)2;

		// When we get the monitor
		IMonitor result = ctx.Store.Pick(Pickers.GetLastWhimActiveMonitor());

		// Then we get the monitor
		Assert.Equal((HMONITOR)2, result.Handle);
	}
}

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
		mutableRootSector.MonitorSector.Monitors = ImmutableArray.Create(monitor);
	}

	private static readonly IPoint<int> _point = new Point<int>(10, 10);

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Monitor_CannotFindMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IMonitor monitor
	)
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
	internal void Monitor_CannotFindMonitor_GetFirst(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IMonitor monitor
	)
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
	internal void Monitor_FoundMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IMonitor monitor
	)
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
