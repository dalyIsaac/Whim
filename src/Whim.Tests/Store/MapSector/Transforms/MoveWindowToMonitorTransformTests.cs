namespace Whim.Tests;

public class MoveWindowToMonitorTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoValidWindows(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is no valid windows
		AddActiveWorkspaceToStore(rootSector, CreateWorkspace());
		MoveWindowToMonitorTransform sut = new((HMONITOR)10);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWindowForHandle(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is no window for the handle
		AddActiveWorkspaceToStore(rootSector, CreateWorkspace());
		MoveWindowToMonitorTransform sut = new((HMONITOR)10);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForMonitor(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is no workspace for the monitor
		IWindow window = CreateWindow((HWND)10);
		HMONITOR monitorHandle = (HMONITOR)10;
		AddWindowToSector(rootSector, window);

		MoveWindowToMonitorTransform sut = new(monitorHandle, window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoMonitorForWindow(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is no monitor for the window
		IWindow window = CreateWindow((HWND)10);
		IMonitor monitor = CreateMonitor((HMONITOR)10);

		AddWindowToSector(rootSector, window);
		PopulateMonitorWorkspaceMap(rootSector, monitor, CreateWorkspace());

		MoveWindowToMonitorTransform sut = new(monitor.Handle, window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowAlreadyOnMonitor(IContext ctx, MutableRootSector rootSector)
	{
		// Given the window is already on the monitor
		IWindow window = CreateWindow((HWND)10);
		IMonitor monitor = CreateMonitor((HMONITOR)10);

		PopulateThreeWayMap(rootSector, monitor, CreateWorkspace(), window);

		MoveWindowToMonitorTransform sut = new(monitor.Handle, window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is a three way map
		IWindow window = CreateWindow((HWND)10);
		IMonitor originalMonitor = CreateMonitor((HMONITOR)10);
		IMonitor newMonitor = CreateMonitor((HMONITOR)11);

		PopulateThreeWayMap(rootSector, originalMonitor, CreateWorkspace(), window);
		PopulateMonitorWorkspaceMap(rootSector, newMonitor, CreateWorkspace());

		MoveWindowToMonitorTransform sut = new(newMonitor.Handle, window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);
	}
}
