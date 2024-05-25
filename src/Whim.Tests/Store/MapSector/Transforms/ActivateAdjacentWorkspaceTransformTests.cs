using System.Diagnostics.CodeAnalysis;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

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
		IWorkspace workspace = CreateWorkspace();
		IMonitor monitor = CreateMonitor((HMONITOR)1);

		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, workspace);

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
		IWorkspace workspace1 = CreateWorkspace();
		IWorkspace workspace2 = CreateWorkspace();

		IMonitor monitor = CreateMonitor((HMONITOR)1);

		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, workspace1);
		AddWorkspaceToManager(ctx, workspace2);

		ActivateAdjacentWorkspaceTransform sut = new(monitor.Handle);

		// When we activate the adjacent workspace
		var result = ctx.Store.Dispatch(sut);

		// Then we get an error
		Assert.True(result.IsSuccessful);
	}
}
