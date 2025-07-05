using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Whim.TestUtils.MonitorTestUtils;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MonitorsChangedTransformTests
{
	private static readonly (RECT Rect, HMONITOR Handle) LeftTopMonitorSetup = (
		new RECT()
		{
			left = 0,
			top = 0,
			right = 1920,
			bottom = 1080,
		},
		(HMONITOR)1
	);

	private static readonly (RECT Rect, HMONITOR Handle) LeftBottomMonitorSetup = (
		new RECT()
		{
			left = 0,
			top = 1080,
			right = 1920,
			bottom = 2160,
		},
		(HMONITOR)2
	);

	private static readonly (RECT Rect, HMONITOR Handle) RightMonitorSetup = (
		new RECT()
		{
			left = 1920,
			top = 0,
			right = 3840,
			bottom = 1080,
		},
		(HMONITOR)3
	);

	private static Assert.RaisedEvent<MonitorsChangedEventArgs> DispatchTransformEvent(
		IContext ctx,
		MutableRootSector rootSector,
		Guid[]? layoutWorkspaceIds = null,
		Guid[]? notLayoutWorkspaceIds = null
	) =>
		Assert.Raises<MonitorsChangedEventArgs>(
			h => ctx.Store.MonitorEvents.MonitorsChanged += h,
			h => ctx.Store.MonitorEvents.MonitorsChanged -= h,
			() =>
				CustomAssert.Layout(
					rootSector,
					() => ctx.Store.Dispatch(new MonitorsChangedTransform()),
					layoutWorkspaceIds,
					notLayoutWorkspaceIds
				)
		);

	private static void Setup_TryEnqueue(IInternalContext internalCtx) =>
		internalCtx.CoreNativeManager.IsStaThread().Returns(_ => true, _ => false);

	/// <summary>
	/// Populate the sector with workspaces.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="rootSector"></param>
	/// <returns></returns>
	private static IWorkspace[] PopulateWorkspaces(IContext ctx, MutableRootSector rootSector)
	{
		Workspace workspace1 = CreateWorkspace(ctx);
		Workspace workspace2 = CreateWorkspace(ctx);
		Workspace workspace3 = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, rootSector, workspace1, workspace2, workspace3);
		rootSector.WorkspaceSector.HasInitialized = true;
		return [workspace1, workspace2, workspace3];
	}

	/// <summary>
	/// Setup the adding of workspaces to the context.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="rootSector"></param>
	/// <returns></returns>
	private static IWorkspace[] SetupAddWorkspaces(IContext ctx, MutableRootSector rootSector)
	{
		Workspace workspace1 = CreateWorkspace(ctx);
		Workspace workspace2 = CreateWorkspace(ctx);
		Workspace workspace3 = CreateWorkspace(ctx);

		((StoreWrapper)ctx.Store)
			.AddInterceptor(
				t => t is AddWorkspaceTransform,
				t =>
				{
					AddWorkspaceToManager(ctx, rootSector, workspace1);
					return workspace1.Id;
				}
			)
			.AddInterceptor(
				t => t is AddWorkspaceTransform,
				t =>
				{
					AddWorkspaceToManager(ctx, rootSector, workspace2);
					return workspace2.Id;
				}
			)
			.AddInterceptor(
				t => t is AddWorkspaceTransform,
				t =>
				{
					AddWorkspaceToManager(ctx, rootSector, workspace3);
					return workspace3.Id;
				}
			);

		rootSector.WorkspaceSector.HasInitialized = true;
		return [workspace1, workspace2, workspace3];
	}

	private static void AssertContainsTransform(IContext ctx, Guid workspaceId, int times = 1)
	{
		CustomAssert.Contains(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaceId),
			times
		);
	}

	private static void AssertDoesNotContainTransform(IContext ctx, Guid workspaceId)
	{
		Assert.DoesNotContain(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaceId)
		);
	}

	private static void AssertPrimaryMonitor(MutableRootSector rootSector, HMONITOR handle)
	{
		Assert.Equal(handle, rootSector.MonitorSector.PrimaryMonitorHandle);
		Assert.Equal(handle, rootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal(handle, rootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceSectorNotInitialized(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given we've populated monitors
		SetupAddWorkspaces(ctx, rootSector);
		SetupMultipleMonitors(internalCtx, [RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup]);

		rootSector.WorkspaceSector.HasInitialized = false;

		// When something happens
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// Then no workspaces are created
		Assert.Empty(rootSector.WorkspaceSector.WorkspaceOrder);
		Assert.Empty(rootSector.MapSector.MonitorWorkspaceMap);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsRemoved(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given we've populated monitors
		Setup_TryEnqueue(internalCtx);
		IWorkspace[] workspaces = PopulateWorkspaces(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, [RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup]);

		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// When a monitor is removed
		Setup_TryEnqueue(internalCtx);
		SetupMultipleMonitors(internalCtx, [RightMonitorSetup, LeftTopMonitorSetup]);

		// Then the resulting event will have a monitor removed
		var raisedEvent = DispatchTransformEvent(
			ctx,
			rootSector,
			[workspaces[0].Id, workspaces[2].Id],
			[workspaces[1].Id]
		);

		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Single(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());

		Assert.Equal(2, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);
		Assert.DoesNotContain(LeftBottomMonitorSetup.Handle, rootSector.MapSector.MonitorWorkspaceMap.Keys);

		AssertContainsTransform(ctx, workspaces[0].Id);
		AssertContainsTransform(ctx, workspaces[1].Id, 2);
		AssertContainsTransform(ctx, workspaces[2].Id);

		AssertPrimaryMonitor(rootSector, LeftTopMonitorSetup.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given we've populated monitors
		Setup_TryEnqueue(internalCtx);
		IWorkspace[] workspaces = PopulateWorkspaces(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, [RightMonitorSetup, LeftTopMonitorSetup]);
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// When a monitor is added
		Setup_TryEnqueue(internalCtx);
		SetupMultipleMonitors(internalCtx, [RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup]);

		// Then the resulting event will have a monitor added
		var raisedEvent = DispatchTransformEvent(
			ctx,
			rootSector,
			[workspaces[0].Id, workspaces[1].Id, workspaces[2].Id]
		);

		Assert.Single(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());

		Assert.Equal(3, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftBottomMonitorSetup.Handle]);

		AssertContainsTransform(ctx, workspaces[0].Id);
		AssertContainsTransform(ctx, workspaces[1].Id);
		AssertDoesNotContainTransform(ctx, workspaces[2].Id);

		AssertPrimaryMonitor(rootSector, LeftTopMonitorSetup.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded_PrimaryChanged(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given we've populated monitors
		Setup_TryEnqueue(internalCtx);
		IWorkspace[] workspaces = PopulateWorkspaces(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, [RightMonitorSetup, LeftTopMonitorSetup]);
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// When a monitor is added, and the new monitor changes to become the primary monitor
		Setup_TryEnqueue(internalCtx);
		SetupMultipleMonitors(
			internalCtx,
			[RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup],
			LeftBottomMonitorSetup.Handle
		);

		// Then the resulting event will have:
		// - two monitors added (the new one and the previous primary as a non-primary)
		// - one monitor removed (the previous primary as a non-primary)
		var raisedEvent = DispatchTransformEvent(
			ctx,
			rootSector,
			[workspaces[0].Id, workspaces[1].Id, workspaces[2].Id]
		);

		Assert.Equal(2, raisedEvent.Arguments.AddedMonitors.Count());
		Assert.Single(raisedEvent.Arguments.RemovedMonitors);
		Assert.Single(raisedEvent.Arguments.UnchangedMonitors);

		Assert.Equal(3, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftBottomMonitorSetup.Handle]);

		AssertContainsTransform(ctx, workspaces[0].Id, 2);
		AssertContainsTransform(ctx, workspaces[1].Id);
		AssertDoesNotContainTransform(ctx, workspaces[2].Id);

		AssertPrimaryMonitor(rootSector, LeftBottomMonitorSetup.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded_Initialization(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given we have no monitors
		IWorkspace[] workspaces = PopulateWorkspaces(ctx, rootSector);

		// When we add monitors
		SetupMultipleMonitors(internalCtx, [RightMonitorSetup, LeftTopMonitorSetup]);
		var raisedEvent = DispatchTransformEvent(ctx, rootSector, [workspaces[0].Id, workspaces[1].Id]);

		// Then the resulting event will have a monitor added, and the other monitors in the sector will be set.
		Assert.Equal(2, raisedEvent.Arguments.AddedMonitors.Count());

		Assert.Equal(LeftTopMonitorSetup.Handle, ctx.Store.Pick(Pickers.PickLastWhimActiveMonitor()).Handle);
		Assert.Equal(LeftTopMonitorSetup.Handle, ctx.Store.Pick(Pickers.PickActiveMonitor()).Handle);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);

		Assert.DoesNotContain(ctx.GetTransforms(), t => t is DeactivateWorkspaceTransform);

		AssertPrimaryMonitor(rootSector, LeftTopMonitorSetup.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded_Initialization_AddWorkspaces(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given we have no monitors
		IWorkspace[] workspaces = SetupAddWorkspaces(ctx, rootSector);

		// When we add monitors
		SetupMultipleMonitors(internalCtx, [RightMonitorSetup, LeftTopMonitorSetup]);
		var raisedEvent = DispatchTransformEvent(ctx, rootSector, [workspaces[0].Id, workspaces[1].Id]);

		// Then the resulting event will have a monitor added, and the other monitors in the sector will be set.
		Assert.Equal(2, raisedEvent.Arguments.AddedMonitors.Count());

		Assert.Equal(LeftTopMonitorSetup.Handle, ctx.Store.Pick(Pickers.PickLastWhimActiveMonitor()).Handle);
		Assert.Equal(LeftTopMonitorSetup.Handle, ctx.Store.Pick(Pickers.PickActiveMonitor()).Handle);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);

		Assert.DoesNotContain(ctx.GetTransforms(), t => t is DeactivateWorkspaceTransform);

		AssertPrimaryMonitor(rootSector, LeftTopMonitorSetup.Handle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsUnchanged(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given there are no changes in the monitors.
		Setup_TryEnqueue(internalCtx);
		IWorkspace[] workspaces = PopulateWorkspaces(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, [RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup]);

		// When we dispatch the same transform twice, the first from a clean store
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// Then the second dispatch should receive all monitors as unchanged.
		Setup_TryEnqueue(internalCtx);
		var raisedEvent = DispatchTransformEvent(
			ctx,
			rootSector,
			notLayoutWorkspaceIds: [workspaces[0].Id, workspaces[1].Id, workspaces[2].Id]
		);

		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(3, raisedEvent.Arguments.UnchangedMonitors.Count());

		Assert.Equal(3, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftBottomMonitorSetup.Handle]);

		AssertDoesNotContainTransform(ctx, workspaces[0].Id);
		AssertDoesNotContainTransform(ctx, workspaces[1].Id);
		AssertDoesNotContainTransform(ctx, workspaces[2].Id);

		AssertPrimaryMonitor(rootSector, LeftTopMonitorSetup.Handle);
	}
}
