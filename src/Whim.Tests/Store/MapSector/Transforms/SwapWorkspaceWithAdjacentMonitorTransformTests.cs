namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class SwapWorkspaceWithAdjacentMonitorTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoMonitorForWorkspace(IContext ctx)
	{
		// Given there is no monitor for the given workspace
		Guid workspaceId = Guid.NewGuid();
		SwapWorkspaceWithAdjacentMonitorTransform sut = new(workspaceId);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we fail
		Assert.False(result.IsSuccessful);
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>(false)]
	[InlineAutoSubstituteData<StoreCustomization>(true)]
	internal void SingleMonitor(bool reverse, IContext ctx, MutableRootSector rootSector)
	{
		// Given there is a single monitor
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		Workspace workspace = CreateWorkspace(ctx);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, workspace);

		SwapWorkspaceWithAdjacentMonitorTransform sut = new(workspace.Id, reverse);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we succeed
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForNextMonitor(IContext ctx, MutableRootSector rootSector)
	{
		// Given there are multiple monitors
		IMonitor monitor1 = CreateMonitor((HMONITOR)1);
		IMonitor monitor2 = CreateMonitor((HMONITOR)2);
		Workspace workspace1 = CreateWorkspace(ctx);

		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor1, workspace1);
		AddMonitorsToSector(ctx, rootSector, monitor2);

		SwapWorkspaceWithAdjacentMonitorTransform sut = new(workspace1.Id);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we fail
		Assert.False(result.IsSuccessful);
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>(0, false, 1)]
	[InlineAutoSubstituteData<StoreCustomization>(0, true, 2)]
	internal void Success(
		int workspaceIndex,
		bool reverse,
		int resultWorkspaceIndex,
		IContext ctx,
		MutableRootSector rootSector
	)
	{
		// Given the target monitor has an old workspace, and the new workspace wasn't previously activated
		Workspace workspace1 = CreateWorkspace(ctx);
		Workspace workspace2 = CreateWorkspace(ctx);
		Workspace workspace3 = CreateWorkspace(ctx);
		Workspace[] workspaces = [workspace1, workspace2, workspace3];

		IMonitor monitor1 = CreateMonitor((HMONITOR)1);
		IMonitor monitor2 = CreateMonitor((HMONITOR)2);
		IMonitor monitor3 = CreateMonitor((HMONITOR)3);
		IMonitor[] monitors = [monitor1, monitor2, monitor3];

		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor1, workspace1);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor2, workspace2);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor3, workspace3);

		SwapWorkspaceWithAdjacentMonitorTransform sut = new(workspaces[workspaceIndex].Id, reverse);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then the workspaces are swapped
		Assert.True(result.IsSuccessful);

		Assert.Equal(workspaces[resultWorkspaceIndex].Id, rootSector.MapSector.MonitorWorkspaceMap[monitor1.Handle]);
		Assert.Equal(
			workspaces[workspaceIndex].Id,
			rootSector.MapSector.MonitorWorkspaceMap[monitors[resultWorkspaceIndex].Handle]
		);
	}
}
