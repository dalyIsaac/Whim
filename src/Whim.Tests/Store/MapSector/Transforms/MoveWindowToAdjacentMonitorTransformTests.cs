namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MoveWindowToAdjacentMonitorTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoAdjacentMonitors(IContext ctx, MutableRootSector rootSector)
	{
		// Given there are no adjacent monitors
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		IWindow window = CreateWindow((HWND)10);

		rootSector.MonitorSector.Monitors = [monitor];
		rootSector.MonitorSector.ActiveMonitorHandle = monitor.Handle;
		AddWindowToSector(rootSector, window);

		MoveWindowToAdjacentMonitorTransform sut = new(window.Handle);

		// When we dispatch the transform
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>(false, 0, 1)]
	[InlineAutoSubstituteData<StoreCustomization>(false, 1, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(false, 2, 0)]
	[InlineAutoSubstituteData<StoreCustomization>(true, 0, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(true, 2, 1)]
	[InlineAutoSubstituteData<StoreCustomization>(true, 1, 0)]
	internal void Success(bool reverse, int startIdx, int endIdx, IContext ctx, MutableRootSector rootSector)
	{
		// Given the window is on the starting monitor
		IWindow window = CreateWindow((HWND)10);
		AddWindowToSector(rootSector, window);

		// and there are three adjacent monitors
		PopulateMonitorWorkspaceMap(rootSector, CreateMonitor((HMONITOR)10), CreateWorkspace());
		PopulateMonitorWorkspaceMap(rootSector, CreateMonitor((HMONITOR)11), CreateWorkspace());
		PopulateMonitorWorkspaceMap(rootSector, CreateMonitor((HMONITOR)12), CreateWorkspace());

		rootSector.MonitorSector.ActiveMonitorHandle = rootSector.MonitorSector.Monitors[startIdx].Handle;

		Guid startWorkspaceId = rootSector.WorkspaceSector.WorkspaceOrder[startIdx];
		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.SetItem(
			window.Handle,
			startWorkspaceId
		);

		MoveWindowToAdjacentMonitorTransform sut = new(window.Handle, reverse);

		// When we dispatch the transform
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);

		Guid endWorkspaceId = rootSector.WorkspaceSector.WorkspaceOrder[endIdx];
		Assert.Equal(endWorkspaceId, rootSector.MapSector.WindowWorkspaceMap[window.Handle]);
	}
}
