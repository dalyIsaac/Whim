namespace Whim.Tests;

public class MutableRootSectorTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Initialize_Dispose(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		ILayoutEngine engine1,
		ILayoutEngine engine2
	)
	{
		// Given there is a populated layout engine creator
		MutableRootSector sut = new(ctx, internalCtx);
		var capture = CaptureWinEventProc.Create(internalCtx);

		rootSector.WorkspaceSector.CreateLayoutEngines = () => [(id) => engine1, (id) => engine2];

		ctx.Store.Dispatch(new AddWorkspaceTransform());

		// When we initialize and dispose the root sector
		sut.Initialize();
		sut.Dispose();

		// Then the monitor sector subscribes to the window message monitor
		internalCtx.WindowMessageMonitor.DisplayChanged += Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
		internalCtx.WindowMessageMonitor.DisplayChanged -= Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();

		// and the window sector subscribes to SetWinEventHook
		Assert.Equal(6, capture.Handles.Count);

		// and a workspace is created
		Assert.Single(rootSector.WorkspaceSector.Workspaces);
		Assert.Single(rootSector.WorkspaceSector.WorkspaceOrder);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void HasQueuedEvents_False(MutableRootSector rootSector)
	{
		// Given no queued events

		// When we check if there are queued events
		bool result = rootSector.HasQueuedEvents;

		// Then we get false
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void HasQueuedEvents_MonitorSector(MutableRootSector rootSector)
	{
		// Given queued events in the monitor sector
		rootSector.MonitorSector.QueueEvent(new EventArgs());

		// When we check if there are queued events
		bool result = rootSector.HasQueuedEvents;

		// Then we get true
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void HasQueuedEvents_WindowSector(MutableRootSector rootSector)
	{
		// Given queued events in the window sector
		rootSector.WindowSector.QueueEvent(new EventArgs());

		// When we check if there are queued events
		bool result = rootSector.HasQueuedEvents;

		// Then we get true
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void HasQueuedEvents_WorkspaceSector(MutableRootSector rootSector)
	{
		// Given queued events in the workspace sector
		rootSector.WorkspaceSector.QueueEvent(new EventArgs());

		// When we check if there are queued events
		bool result = rootSector.HasQueuedEvents;

		// Then we get true
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void HasQueuedEvents_MapSector(MutableRootSector rootSector)
	{
		// Given queued events in the map sector
		rootSector.MapSector.QueueEvent(new EventArgs());

		// When we check if there are queued events
		bool result = rootSector.HasQueuedEvents;

		// Then we get true
		Assert.True(result);
	}
}
