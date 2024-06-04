using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MoveWindowToMonitorTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoValidWindows(IContext ctx)
	{
		// Given there is no valid windows
		MoveWindowToMonitorTransform sut = new((HMONITOR)10);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWindowForHandle(IContext ctx)
	{
		// Given there is no window for the handle
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
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, CreateWorkspace(ctx));

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

		PopulateThreeWayMap(ctx, rootSector, monitor, CreateWorkspace(ctx), window);

		MoveWindowToMonitorTransform sut = new(monitor.Handle, window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);
	}

	// [Theory, AutoSubstituteData<StoreCustomization>]
	// internal void Success(IContext ctx, MutableRootSector rootSector)
	// {
	// 	// Given there is a three way map
	// 	IWindow window = CreateWindow((HWND)10);
	// 	IMonitor originalMonitor = CreateMonitor((HMONITOR)10);
	// 	IMonitor newMonitor = CreateMonitor((HMONITOR)11);

	// 	PopulateThreeWayMap(ctx, rootSector, originalMonitor, CreateWorkspace(ctx), window);
	// 	PopulateMonitorWorkspaceMap(ctx, rootSector, newMonitor, CreateWorkspace(ctx));

	// 	MoveWindowToMonitorTransform sut = new(newMonitor.Handle, window.Handle);

	// 	// When
	// 	var result = ctx.Store.Dispatch(sut);

	// 	// Then
	// 	Assert.True(result.IsSuccessful);
	// }
}
