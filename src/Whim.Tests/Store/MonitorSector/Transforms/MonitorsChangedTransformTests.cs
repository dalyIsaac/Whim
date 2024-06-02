using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.MonitorTestUtils;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
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
			h => ctx.Store.MonitorEvents.MonitorsChanged += h,
			h => ctx.Store.MonitorEvents.MonitorsChanged -= h,
			() => ctx.Store.Dispatch(new MonitorsChangedTransform())
		);

	private static void Setup_TryEnqueue(IInternalContext internalCtx) =>
		internalCtx.CoreNativeManager.IsStaThread().Returns(_ => true, _ => false);

	private static IWorkspace[] SetupWorkspaces(IContext ctx, MutableRootSector rootSector)
	{
		IWorkspace workspace1 = CreateWorkspace(ctx);
		IWorkspace workspace2 = CreateWorkspace(ctx);
		IWorkspace workspace3 = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, workspace1, workspace2, workspace3);

		ctx.WorkspaceManager.Add().Returns(_ => workspace1, _ => workspace2, _ => workspace3);

		rootSector.MonitorSector.MonitorsChangedDelay = 0;

		return new[] { workspace1, workspace2, workspace3 };
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsRemoved(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given we've populated monitors
		Setup_TryEnqueue(internalCtx);
		IWorkspace[] workspaces = SetupWorkspaces(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup });

		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// When a monitor is removed
		Setup_TryEnqueue(internalCtx);
		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup });

		// Then the resulting event will have a monitor removed
		var raisedEvent = DispatchTransformEvent(ctx);

		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Single(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());

		Assert.Equal(2, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)2]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)1]);
		Assert.DoesNotContain((HMONITOR)3, rootSector.MapSector.MonitorWorkspaceMap.Keys);

		workspaces[0].Received(1).Deactivate();
		workspaces[0].Received(2).DoLayout();

		workspaces[1].Received(2).Deactivate();
		workspaces[1].Received(1).DoLayout();

		workspaces[2].Received(1).Deactivate();
		workspaces[2].Received(2).DoLayout();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given we've populated monitors
		Setup_TryEnqueue(internalCtx);
		IWorkspace[] workspaces = SetupWorkspaces(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup });
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// When a monitor is added
		Setup_TryEnqueue(internalCtx);
		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup });

		// Then the resulting event will have a monitor added
		var raisedEvent = DispatchTransformEvent(ctx);

		Assert.Single(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());

		Assert.Equal(3, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)2]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)1]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)3]);

		workspaces[0].Received(1).Deactivate();
		workspaces[0].Received(2).DoLayout();

		workspaces[1].Received(1).Deactivate();
		workspaces[1].Received(2).DoLayout();

		workspaces[2].DidNotReceive().Deactivate();
		workspaces[2].Received(1).DoLayout();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded_Initialization(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given we have no monitors
		IWorkspace[] workspaces = SetupWorkspaces(ctx, rootSector);

		// When we add monitors
		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup });
		var raisedEvent = DispatchTransformEvent(ctx);

		// Then the resulting event will have a monitor added, and the other monitors in the sector will be set.
		Assert.Equal(2, raisedEvent.Arguments.AddedMonitors.Count());

		Assert.Equal((HMONITOR)2, ctx.Store.Pick(Pickers.PickLastWhimActiveMonitor()).Handle);
		Assert.Equal((HMONITOR)2, ctx.Store.Pick(Pickers.PickActiveMonitor()).Handle);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)2]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)1]);

		workspaces[0].DidNotReceive().Deactivate();
		workspaces[0].Received(1).DoLayout();

		workspaces[1].DidNotReceive().Deactivate();
		workspaces[1].Received(1).DoLayout();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsUnchanged(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given there are no changes in the monitors.
		Setup_TryEnqueue(internalCtx);
		IWorkspace[] workspaces = SetupWorkspaces(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup });

		// When we dispatch the same transform twice, the first from a clean store
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// Then the second dispatch should receive all monitors as unchanged.
		Setup_TryEnqueue(internalCtx);
		var raisedEvent = DispatchTransformEvent(ctx);

		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(3, raisedEvent.Arguments.UnchangedMonitors.Count());

		Assert.Equal(3, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)2]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)1]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[(HMONITOR)3]);

		workspaces[0].DidNotReceive().Deactivate();
		workspaces[0].Received(1).DoLayout();

		workspaces[1].DidNotReceive().Deactivate();
		workspaces[1].Received(1).DoLayout();

		workspaces[2].DidNotReceive().Deactivate();
		workspaces[2].Received(1).DoLayout();
	}
}
