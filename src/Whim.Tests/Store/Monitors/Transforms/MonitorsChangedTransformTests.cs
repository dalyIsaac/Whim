using System.Linq;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Tests;

public class MonitorsChangedTransformTests
{
	private static readonly (RECT, HMONITOR) RightMonitorSetup = (
		new RECT()
		{
			left = 1920,
			top = 0,
			right = 3840,
			bottom = 1080
		},
		(HMONITOR)1
	);

	private static readonly (RECT, HMONITOR) LeftTopMonitorSetup = (
		new RECT()
		{
			left = 0,
			top = 0,
			right = 1920,
			bottom = 1080
		},
		(HMONITOR)2
	);

	private static readonly (RECT, HMONITOR) LeftBottomMonitorSetup = (
		new RECT()
		{
			left = 0,
			top = 1080,
			right = 1920,
			bottom = 2160
		},
		(HMONITOR)3
	);

	private static Assert.RaisedEvent<MonitorsChangedEventArgs> DispatchTransformEvent(IContext ctx) =>
		Assert.Raises<MonitorsChangedEventArgs>(
			h => ctx.Store.Monitors.MonitorsChanged += h,
			h => ctx.Store.Monitors.MonitorsChanged -= h,
			() => ctx.Store.Dispatch(new MonitorsChangedTransform())
		);

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsRemoved(IContext ctx, IInternalContext internalCtx)
	{
		// Given we've populated monitors
		MonitorTestUtils.SetupMultipleMonitors(
			internalCtx,
			new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup }
		);
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// When a monitor is removed
		MonitorTestUtils.SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup });

		// Then the resulting event will have a monitor removed
		var raisedEvent = DispatchTransformEvent(ctx);

		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Single(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded(IContext ctx, IInternalContext internalCtx)
	{
		// Given we've populated monitors
		MonitorTestUtils.SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup });
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// When a monitor is added
		MonitorTestUtils.SetupMultipleMonitors(
			internalCtx,
			new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup }
		);

		// Then the resulting event will have a monitor added
		var raisedEvent = DispatchTransformEvent(ctx);

		Assert.Single(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsUnchanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given there are no changes in the monitors.
		MonitorTestUtils.SetupMultipleMonitors(
			internalCtx,
			new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup }
		);

		// When we dispatch the same transform twice, the first from a clean store
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// Then the second dispatch should receive all monitors as unchanged.
		var raisedEvent = DispatchTransformEvent(ctx);

		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(3, raisedEvent.Arguments.UnchangedMonitors.Count());
	}
}
