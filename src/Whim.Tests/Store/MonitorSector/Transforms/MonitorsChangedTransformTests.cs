using System.Linq;
using static Whim.TestUtils.MonitorTestUtils;

namespace Whim.Tests;

public class MonitorsChangedTransformTests
{
	private static readonly (RECT Rect, HMONITOR Handle) RightMonitorSetup = (
		new RECT()
		{
			left = 1920,
			top = 0,
			right = 3840,
			bottom = 1080
		},
		(HMONITOR)3
	);

	private static readonly (RECT Rect, HMONITOR Handle) LeftTopMonitorSetup = (
		new RECT()
		{
			left = 0,
			top = 0,
			right = 1920,
			bottom = 1080
		},
		(HMONITOR)1
	);

	private static readonly (RECT Rect, HMONITOR Handle) LeftBottomMonitorSetup = (
		new RECT()
		{
			left = 0,
			top = 1080,
			right = 1920,
			bottom = 2160
		},
		(HMONITOR)2
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

	private static IWorkspace[] SetupWorkspaces_AlreadyAdded(IContext ctx, MutableRootSector rootSector)
	{
		Workspace workspace1 = CreateWorkspace(ctx);
		Workspace workspace2 = CreateWorkspace(ctx);
		Workspace workspace3 = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, rootSector, workspace1, workspace2, workspace3);
		rootSector.WorkspaceSector.HasInitialized = true;
		return new[] { workspace1, workspace2, workspace3 };
	}

	private static IWorkspace[] SetupWorkspaces_AddWorkspaces(IContext ctx, MutableRootSector rootSector)
	{
		Workspace workspace1 = CreateWorkspace(ctx);
		Workspace workspace2 = CreateWorkspace(ctx);
		Workspace workspace3 = CreateWorkspace(ctx);
		ctx.WorkspaceManager.Add()
			.Returns(
				_ =>
				{
					AddWorkspaceToManager(ctx, rootSector, workspace1);
					return workspace1;
				},
				_ =>
				{
					AddWorkspaceToManager(ctx, rootSector, workspace2);
					return workspace2;
				},
				_ =>
				{
					AddWorkspaceToManager(ctx, rootSector, workspace3);
					return workspace3;
				}
			);
		rootSector.WorkspaceSector.HasInitialized = true;
		return new[] { workspace1, workspace2, workspace3 };
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceSectorNotInitialized(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given we've populated monitors
		SetupWorkspaces_AddWorkspaces(ctx, rootSector);
		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup });

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
		IWorkspace[] workspaces = SetupWorkspaces_AlreadyAdded(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup });

		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// When a monitor is removed
		Setup_TryEnqueue(internalCtx);
		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup });

		// Then the resulting event will have a monitor removed
		var raisedEvent = DispatchTransformEvent(
			ctx,
			rootSector,
			new[] { workspaces[0].Id, workspaces[2].Id },
			new[] { workspaces[1].Id }
		);

		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Single(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());

		Assert.Equal(2, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);
		Assert.DoesNotContain(LeftBottomMonitorSetup.Handle, rootSector.MapSector.MonitorWorkspaceMap.Keys);

		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaces[0].Id)
		);

		CustomAssert.Contains(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaces[1].Id),
			2
		);

		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaces[2].Id)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given we've populated monitors
		Setup_TryEnqueue(internalCtx);
		IWorkspace[] workspaces = SetupWorkspaces_AlreadyAdded(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup });
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// When a monitor is added
		Setup_TryEnqueue(internalCtx);
		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup });

		// Then the resulting event will have a monitor added
		var raisedEvent = DispatchTransformEvent(
			ctx,
			rootSector,
			new[] { workspaces[0].Id, workspaces[1].Id, workspaces[2].Id }
		);

		Assert.Single(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());

		Assert.Equal(3, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftBottomMonitorSetup.Handle]);

		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaces[0].Id)
		);

		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaces[1].Id)
		);

		Assert.DoesNotContain(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaces[2].Id)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded_Initialization(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given we have no monitors
		IWorkspace[] workspaces = SetupWorkspaces_AlreadyAdded(ctx, rootSector);

		// When we add monitors
		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup });
		var raisedEvent = DispatchTransformEvent(ctx, rootSector, new[] { workspaces[0].Id, workspaces[1].Id });

		// Then the resulting event will have a monitor added, and the other monitors in the sector will be set.
		Assert.Equal(2, raisedEvent.Arguments.AddedMonitors.Count());

		Assert.Equal(LeftTopMonitorSetup.Handle, ctx.Store.Pick(Pickers.PickLastWhimActiveMonitor()).Handle);
		Assert.Equal(LeftTopMonitorSetup.Handle, ctx.Store.Pick(Pickers.PickActiveMonitor()).Handle);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);

		Assert.DoesNotContain(ctx.GetTransforms(), t => t is DeactivateWorkspaceTransform);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsAdded_Initialization_AddWorkspaces(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given we have no monitors
		IWorkspace[] workspaces = SetupWorkspaces_AddWorkspaces(ctx, rootSector);

		// When we add monitors
		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup });
		var raisedEvent = DispatchTransformEvent(ctx, rootSector, new[] { workspaces[0].Id, workspaces[1].Id });

		// Then the resulting event will have a monitor added, and the other monitors in the sector will be set.
		Assert.Equal(2, raisedEvent.Arguments.AddedMonitors.Count());

		Assert.Equal(LeftTopMonitorSetup.Handle, ctx.Store.Pick(Pickers.PickLastWhimActiveMonitor()).Handle);
		Assert.Equal(LeftTopMonitorSetup.Handle, ctx.Store.Pick(Pickers.PickActiveMonitor()).Handle);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);

		Assert.DoesNotContain(ctx.GetTransforms(), t => t is DeactivateWorkspaceTransform);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsUnchanged(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given there are no changes in the monitors.
		Setup_TryEnqueue(internalCtx);
		IWorkspace[] workspaces = SetupWorkspaces_AlreadyAdded(ctx, rootSector);

		SetupMultipleMonitors(internalCtx, new[] { RightMonitorSetup, LeftTopMonitorSetup, LeftBottomMonitorSetup });

		// When we dispatch the same transform twice, the first from a clean store
		ctx.Store.Dispatch(new MonitorsChangedTransform());

		// Then the second dispatch should receive all monitors as unchanged.
		Setup_TryEnqueue(internalCtx);
		var raisedEvent = DispatchTransformEvent(
			ctx,
			rootSector,
			notLayoutWorkspaceIds: new[] { workspaces[0].Id, workspaces[1].Id, workspaces[2].Id }
		);

		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		Assert.Equal(3, raisedEvent.Arguments.UnchangedMonitors.Count());

		Assert.Equal(3, rootSector.MapSector.MonitorWorkspaceMap.Count);

		Assert.Equal(workspaces[0].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftTopMonitorSetup.Handle]);
		Assert.Equal(workspaces[2].Id, rootSector.MapSector.MonitorWorkspaceMap[RightMonitorSetup.Handle]);
		Assert.Equal(workspaces[1].Id, rootSector.MapSector.MonitorWorkspaceMap[LeftBottomMonitorSetup.Handle]);

		Assert.DoesNotContain(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaces[0].Id)
		);

		Assert.DoesNotContain(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaces[1].Id)
		);

		Assert.DoesNotContain(
			ctx.GetTransforms(),
			t => (t as DeactivateWorkspaceTransform) == new DeactivateWorkspaceTransform(workspaces[2].Id)
		);
	}
}
