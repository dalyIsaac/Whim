using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MoveWindowToPointTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForMonitor(IContext ctx)
	{
		// Given there is no workspace for the monitor at the given point
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		Point<int> point = new(10, 10);
		SetupMonitorAtPoint(ctx, point, monitor);

		MoveWindowToPointTransform sut = new((HWND)10, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we fail
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForWindow(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is no workspace for the window
		IWindow window = CreateWindow((HWND)10);
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		IWorkspace workspace = CreateWorkspace();
		Point<int> point = new(10, 10);

		AddWindowToSector(rootSector, window);
		SetupMonitorAtPoint(ctx, point, monitor);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, workspace);

		MoveWindowToPointTransform sut = new(window.Handle, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we fail
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowToPointOnSameMonitor(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is a workspace for the window
		IWindow window = CreateWindow((HWND)10);
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		IWorkspace workspace = CreateWorkspace();
		Point<int> point = new(10, 10);

		AddWindowToSector(rootSector, window);
		SetupMonitorAtPoint(ctx, point, monitor);
		PopulateThreeWayMap(ctx, rootSector, monitor, workspace, window);

		MoveWindowToPointTransform sut = new(window.Handle, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we succeed
		Assert.True(result.IsSuccessful);

		workspace.Received(1).MoveWindowToPoint(window, new Point<double>(10d / 1920, 10d / 1080), deferLayout: false);
		window.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowToPointOnDifferentMonitor(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is a workspace for the window
		IWindow window = CreateWindow((HWND)10);
		IMonitor sourceMonitor = CreateMonitor((HMONITOR)10);
		IWorkspace sourceWorkspace = CreateWorkspace();

		IMonitor targetMonitor = CreateMonitor((HMONITOR)11);
		IWorkspace targetWorkspace = CreateWorkspace();

		Point<int> point = new(10, 10);

		AddWindowToSector(rootSector, window);
		SetupMonitorAtPoint(ctx, point, targetMonitor);
		PopulateThreeWayMap(ctx, rootSector, sourceMonitor, sourceWorkspace, window);
		PopulateMonitorWorkspaceMap(ctx, rootSector, targetMonitor, targetWorkspace);

		MoveWindowToPointTransform sut = new(window.Handle, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we succeed
		Assert.True(result.IsSuccessful);

		Assert.Equal(targetWorkspace.Id, rootSector.MapSector.WindowWorkspaceMap[window.Handle]);

		sourceWorkspace.Received(1).RemoveWindow(window);
		sourceWorkspace.Received(1).DoLayout();
		targetWorkspace
			.Received(1)
			.MoveWindowToPoint(window, new Point<double>(10d / 1920, 10d / 1080), deferLayout: false);

		window.Received(1).Focus();
	}
}
