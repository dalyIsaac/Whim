namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MoveWindowToPointTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoMonitorAtPoint(IContext ctx)
	{
		// Given there is monitor at the given point
		Point<int> point = new(10, 10);

		MoveWindowToPointTransform sut = new((HWND)10, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we fail
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForMonitor(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given there is no workspace for the monitor at the given point
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		Point<int> point = new(10, 10);
		SetupMonitorAtPoint(ctx, internalCtx, rootSector, point, monitor);

		MoveWindowToPointTransform sut = new((HWND)10, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we fail
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForWindow(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given there is no workspace for the window
		IWindow window = CreateWindow((HWND)10);
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		Workspace workspace = CreateWorkspace(ctx);
		Point<int> point = new(10, 10);

		AddWindowToSector(rootSector, window);
		SetupMonitorAtPoint(ctx, internalCtx, rootSector, point, monitor);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, workspace);

		MoveWindowToPointTransform sut = new(window.Handle, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we fail
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowToPointOnSameMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given there is a workspace for the window
		IWindow window = CreateWindow((HWND)10);
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		Workspace workspace = CreateWorkspace(ctx);
		Point<int> point = new(10, 10);

		AddWindowToSector(rootSector, window);
		SetupMonitorAtPoint(ctx, internalCtx, rootSector, point, monitor);
		PopulateThreeWayMap(ctx, rootSector, monitor, workspace, window);

		MoveWindowToPointTransform sut = new(window.Handle, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then we succeed
		Assert.True(result.IsSuccessful);

		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as MoveWindowToPointInWorkspaceTransform)
				== new MoveWindowToPointInWorkspaceTransform(
					workspace.Id,
					window.Handle,
					new Point<double>(10d / 1920, 10d / 1080)
				)
		);
		window.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MoveWindowToPointOnDifferentMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given there is a workspace for the window
		IWindow window = CreateWindow((HWND)10);
		IMonitor sourceMonitor = CreateMonitor((HMONITOR)10);
		Workspace sourceWorkspace = CreateWorkspace(ctx);

		IMonitor targetMonitor = CreateMonitor((HMONITOR)11);
		Workspace targetWorkspace = CreateWorkspace(ctx);

		Point<int> point = new(10, 10);

		AddWindowToSector(rootSector, window);
		SetupMonitorAtPoint(ctx, internalCtx, rootSector, point, targetMonitor);
		PopulateThreeWayMap(ctx, rootSector, sourceMonitor, sourceWorkspace, window);
		PopulateMonitorWorkspaceMap(ctx, rootSector, targetMonitor, targetWorkspace);

		MoveWindowToPointTransform sut = new(window.Handle, point);

		// When we execute the transform
		Result<Unit>? result = null;
		// var result = ctx.Store.Dispatch(sut);
		CustomAssert.Layout(rootSector, () => result = ctx.Store.Dispatch(sut), new[] { sourceWorkspace.Id });

		// Then we succeed
		Assert.True(result!.Value.IsSuccessful);

		Assert.Equal(targetWorkspace.Id, rootSector.MapSector.WindowWorkspaceMap[window.Handle]);

		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as RemoveWindowFromWorkspaceTransform)
				== new RemoveWindowFromWorkspaceTransform(sourceWorkspace.Id, window)
		);
		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as MoveWindowToPointInWorkspaceTransform)
				== new MoveWindowToPointInWorkspaceTransform(
					targetWorkspace.Id,
					window.Handle,
					new Point<double>(10d / 1920, 10d / 1080)
				)
		);

		window.Received(1).Focus();
	}
}
