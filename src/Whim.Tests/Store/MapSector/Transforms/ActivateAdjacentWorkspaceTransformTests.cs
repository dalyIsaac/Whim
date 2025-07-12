using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ActivateAdjacentWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorNotFound(IContext ctx)
	{
		// Given the monitor doesn't exist
		ActivateAdjacentWorkspaceTransform sut = new();

		// When we activate the adjacent workspace
		var result = ctx.Store.Dispatch(sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AdjacentWorkspaceNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given the workspace doesn't exist
		Workspace workspace = CreateWorkspace();
		IMonitor monitor = CreateMonitor((HMONITOR)1);

		PopulateMonitorWorkspaceMap(rootSector, monitor, workspace);

		ActivateAdjacentWorkspaceTransform sut = new(monitor.Handle);

		// When we activate the adjacent workspace
		var result = ctx.Store.Dispatch(sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector)
	{
		// Given the workspace exists
		Workspace workspace1 = CreateWorkspace();
		Workspace workspace2 = CreateWorkspace();

		IMonitor monitor = CreateMonitor((HMONITOR)1);

		PopulateMonitorWorkspaceMap(rootSector, monitor, workspace1);
		AddWorkspacesToStore(rootSector, workspace2);

		ActivateAdjacentWorkspaceTransform sut = new(monitor.Handle);

		// When we activate the adjacent workspace
		var result = ctx.Store.Dispatch(sut);

		// Then we get success
		Assert.True(result.IsSuccessful);
	}
}
