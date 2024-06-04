using System.Collections.Immutable;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

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

		rootSector.MonitorSector.Monitors = ImmutableArray.Create(monitor);
		rootSector.MonitorSector.ActiveMonitorHandle = monitor.Handle;
		AddWindowToSector(rootSector, window);

		MoveWindowToAdjacentMonitorTransform sut = new(window.Handle);

		// When we dispatch the transform
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	// [Theory, AutoSubstituteData<StoreCustomization>]
	// internal void Success(IContext ctx, MutableRootSector rootSector)
	// {
	// 	// Given there are two adjacent monitors
	// 	IMonitor originalMonitor = CreateMonitor((HMONITOR)10);
	// 	IMonitor newMonitor = CreateMonitor((HMONITOR)11);

	// 	IWindow window = CreateWindow((HWND)10);

	// 	PopulateThreeWayMap(ctx, rootSector, originalMonitor, CreateWorkspace(ctx), window);
	// 	PopulateMonitorWorkspaceMap(ctx, rootSector, newMonitor, CreateWorkspace(ctx));
	// 	rootSector.MonitorSector.ActiveMonitorHandle = originalMonitor.Handle;

	// 	MoveWindowToAdjacentMonitorTransform sut = new(window.Handle);

	// 	// When we dispatch the transform
	// 	var result = ctx.Store.Dispatch(sut);

	// 	// Then
	// 	Assert.True(result.IsSuccessful);
	// }
}
