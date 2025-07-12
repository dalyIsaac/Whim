using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MoveWindowToMonitorIndexTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoMonitorAtIndex(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is no monitor at the index
		IWindow window = CreateWindow((HWND)10);
		IMonitor originalMonitor = CreateMonitor((HMONITOR)10);
		IMonitor newMonitor = CreateMonitor((HMONITOR)11);

		PopulateThreeWayMap(rootSector, originalMonitor, CreateWorkspace(), window);
		PopulateMonitorWorkspaceMap(rootSector, newMonitor, CreateWorkspace());

		MoveWindowToMonitorIndexTransform sut = new(2, window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is a three way map
		IWindow window = CreateWindow((HWND)10);

		IMonitor originalMonitor = CreateMonitor((HMONITOR)10);
		IMonitor newMonitor = CreateMonitor((HMONITOR)11);

		Workspace originalWorkspace = CreateWorkspace();
		Workspace newWorkspace = CreateWorkspace();

		PopulateThreeWayMap(rootSector, originalMonitor, originalWorkspace, window);
		PopulateMonitorWorkspaceMap(rootSector, newMonitor, newWorkspace);

		MoveWindowToMonitorIndexTransform sut = new(1, window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(newWorkspace.Id, rootSector.MapSector.WindowWorkspaceMap[window.Handle]);
		Assert.Equal(newWorkspace.Id, rootSector.MapSector.MonitorWorkspaceMap[newMonitor.Handle]);
	}
}
