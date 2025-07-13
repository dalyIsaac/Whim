namespace Whim.Tests;

public class MoveWindowEdgesInDirectionTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoValidWindows(IContext ctx, MutableRootSector rootSector)
	{
		// Given there are no valid windows
		AddActiveWorkspaceToStore(rootSector, CreateWorkspace());
		MoveWindowEdgesInDirectionTransform sut = new(Direction.Down, new Point<int>());

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWindowForHandle(IContext ctx)
	{
		// Given there is no window for the handle
		MoveWindowEdgesInDirectionTransform sut = new(Direction.Down, new Point<int>(), (HWND)10);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForWindow(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is no workspace for the window
		IWindow window = CreateWindow((HWND)10);
		AddWindowToSector(rootSector, window);

		MoveWindowEdgesInDirectionTransform sut = new(Direction.Down, new Point<int>(), window.Handle);

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
		Workspace workspace = CreateWorkspace();

		PopulateMonitorWorkspaceMap(rootSector, monitor, workspace);

		MoveWindowEdgesInDirectionTransform sut = new(Direction.Down, new Point<int>(), window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is a workspace for the window
		IWindow window = CreateWindow((HWND)10);
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		Workspace workspace = CreateWorkspace();
		Point<int> pixelDeltas = new(10, 10);

		PopulateThreeWayMap(rootSector, monitor, workspace, window);

		MoveWindowEdgesInDirectionTransform sut = new(Direction.Down, pixelDeltas, window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);

		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as MoveWindowEdgesInDirectionWorkspaceTransform)
				== new MoveWindowEdgesInDirectionWorkspaceTransform(
					workspace.Id,
					Direction.Down,
					new Point<double>(10d / 1920, 10d / 1080),
					window.Handle
				)
		);
	}
}
