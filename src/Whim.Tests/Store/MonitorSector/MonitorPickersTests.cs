using System.Drawing;

namespace Whim.Tests;

public class MonitorPickersTests
{
	private static void PopulateMonitors(MutableRootSector root)
	{
		IMonitor monitor1 = Substitute.For<IMonitor>();
		monitor1.Handle.Returns((HMONITOR)1);

		IMonitor monitor2 = Substitute.For<IMonitor>();
		monitor2.Handle.Returns((HMONITOR)2);

		IMonitor monitor3 = Substitute.For<IMonitor>();
		monitor3.Handle.Returns((HMONITOR)3);

		IMonitor monitor4 = Substitute.For<IMonitor>();
		monitor4.Handle.Returns((HMONITOR)4);

		root.MonitorSector.Monitors = ImmutableArray.Create(monitor1, monitor2, monitor3, monitor4);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickMonitorByHandle_Failure(IContext ctx, MutableRootSector root)
	{
		// Given the handle does not exist in the monitors
		PopulateMonitors(root);
		HMONITOR monitorHandle = (HMONITOR)40;

		// When we get the monitor
		Result<IMonitor> result = ctx.Store.Pick(Pickers.PickMonitorByHandle(monitorHandle));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickMonitorByHandle_Success(IContext ctx, MutableRootSector root)
	{
		// Given the handle exists in the monitors
		PopulateMonitors(root);
		HMONITOR monitorHandle = (HMONITOR)2;

		// When we get the monitor
		Result<IMonitor> result = ctx.Store.Pick(Pickers.PickMonitorByHandle(monitorHandle));

		// Then we get the monitor
		Assert.True(result.IsSuccessful);
		Assert.Equal(monitorHandle, result.Value.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveMonitor(IContext ctx, MutableRootSector root)
	{
		// Given there is an active monitor
		PopulateMonitors(root);
		root.MonitorSector.ActiveMonitorHandle = (HMONITOR)2;

		// When we get the monitor
		IMonitor result = ctx.Store.Pick(Pickers.PickActiveMonitor());

		// Then we get the monitor
		Assert.Equal((HMONITOR)2, result.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickPrimaryMonitor(IContext ctx, MutableRootSector root)
	{
		// Given there is a primary monitor
		PopulateMonitors(root);
		root.MonitorSector.PrimaryMonitorHandle = (HMONITOR)1;

		// When we get the monitor
		IMonitor result = ctx.Store.Pick(Pickers.PickPrimaryMonitor());

		// Then we get the monitor
		Assert.Equal((HMONITOR)1, result.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastWhimActiveMonitor(IContext ctx, MutableRootSector root)
	{
		// Given there is an active monitor
		PopulateMonitors(root);
		root.MonitorSector.LastWhimActiveMonitorHandle = (HMONITOR)2;

		// When we get the monitor
		IMonitor result = ctx.Store.Pick(Pickers.PickLastWhimActiveMonitor());

		// Then we get the monitor
		Assert.Equal((HMONITOR)2, result.Handle);
	}

	[InlineAutoSubstituteData<StoreCustomization>(true)]
	[InlineAutoSubstituteData<StoreCustomization>(false)]
	[Theory]
	internal void PickAdjacentMonitor_GetFirst(bool reverse, IContext ctx, MutableRootSector root)
	{
		// Given the handle does not exist in the monitors
		PopulateMonitors(root);
		HMONITOR unknownHandle = (HMONITOR)40;

		// When
		Result<IMonitor> result = ctx.Store.Pick(Pickers.PickAdjacentMonitor(unknownHandle, reverse, getFirst: true));

		// Then
		Assert.Equal(root.MonitorSector.Monitors[0].Handle, result.Value.Handle);
	}

	[InlineAutoSubstituteData<StoreCustomization>(true, 0, 3)]
	[InlineAutoSubstituteData<StoreCustomization>(true, 3, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(false, 0, 1)]
	[InlineAutoSubstituteData<StoreCustomization>(false, 3, 0)]
	[Theory]
	internal void PickAdjacentMonitor_DoNotGetFirst(
		bool reverse,
		int startIdx,
		int endIdx,
		IContext ctx,
		MutableRootSector root
	)
	{
		// Given the handle does exist in the monitors
		PopulateMonitors(root);

		// When
		Result<IMonitor> result = ctx.Store.Pick(
			Pickers.PickAdjacentMonitor(root.MonitorSector.Monitors[startIdx].Handle, reverse)
		);

		// Then
		Assert.Equal(root.MonitorSector.Monitors[endIdx].Handle, result.Value.Handle);
	}
}

public class GetMonitorAtPointPickerTests
{
	/// <summary>
	/// Set up the monitor retrieved from the CoreNativeManager, and the handle of the currently
	/// tracked monitor.
	/// </summary>
	/// <param name="root"></param>
	/// <param name="internalCtx"></param>
	/// <param name="foundMonitorHandle">The handle of the monitor at the point.</param>
	/// <param name="monitorHandle">The handle of the monitor which the store currently tracks.</param>
	/// <param name="monitor"></param>
	private static void SetMonitorAtPoint(
		MutableRootSector root,
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
		root.MonitorSector.Monitors = ImmutableArray.Create(monitor);
	}

	private static readonly IPoint<int> _point = new Point<int>(10, 10);

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Monitor_CannotFindMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector root,
		IMonitor monitor
	)
	{
		// Given there is no monitor at the point
		SetMonitorAtPoint(root, internalCtx, (HMONITOR)2, (HMONITOR)1, monitor);
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
		MutableRootSector root,
		IMonitor monitor
	)
	{
		// Given there is no monitor at the point
		SetMonitorAtPoint(root, internalCtx, (HMONITOR)2, (HMONITOR)1, monitor);
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
		MutableRootSector root,
		IMonitor monitor
	)
	{
		// Given there is a monitor at the point
		SetMonitorAtPoint(root, internalCtx, (HMONITOR)1, (HMONITOR)1, monitor);
		GetMonitorAtPointPicker sut = new(_point);

		// When we try get the monitor at said point
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then we get it
		Assert.Equal(monitor, result.Value);
	}
}
