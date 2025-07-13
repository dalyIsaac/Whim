namespace Whim.Tests;

public class MoveWindowToWorkspaceTransformTests
{
	private static Result<Unit> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector rootSector,
		MoveWindowToWorkspaceTransform sut
	)
	{
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => rootSector.MapSector.MonitorWorkspaceChanged += h,
			h => rootSector.MapSector.MonitorWorkspaceChanged -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return result!.Value;
	}

	private static (Result<Unit>, List<MonitorWorkspaceChangedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector rootSector,
		MoveWindowToWorkspaceTransform sut
	)
	{
		Result<Unit>? result = null;
		List<MonitorWorkspaceChangedEventArgs> evs = new();

		CustomAssert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => rootSector.MapSector.MonitorWorkspaceChanged += h,
			h => rootSector.MapSector.MonitorWorkspaceChanged -= h,
			() => result = ctx.Store.Dispatch(sut),
			(sender, args) => evs.Add(args)
		);

		return (result!.Value, evs);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given a random workspace id
		Guid workspaceId = Guid.NewGuid();

		MoveWindowToWorkspaceTransform sut = new(workspaceId);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotFound_SpecifiedHandle(IContext ctx, MutableRootSector rootSector)
	{
		// Given a random window id
		Workspace workspace = CreateWorkspace();
		IWindow window = CreateWindow((HWND)10);

		AddWorkspacesToStore(rootSector, workspace);

		MoveWindowToWorkspaceTransform sut = new(workspace.Id, window.Handle);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotFound_Default(IContext ctx, MutableRootSector rootSector)
	{
		// Given a random window id
		Workspace workspace = CreateWorkspace();
		AddActiveWorkspaceToStore(rootSector, workspace);

		MoveWindowToWorkspaceTransform sut = new(workspace.Id);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceForWindowNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given a window which isn't in a workspace
		Workspace workspace = CreateWorkspace();
		IWindow window = CreateWindow((HWND)10);

		AddWorkspacesToStore(rootSector, workspace);
		rootSector.WindowSector.Windows = rootSector.WindowSector.Windows.Add(window.Handle, window);

		MoveWindowToWorkspaceTransform sut = new(workspace.Id, window.Handle);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowAlreadyOnWorkspace(IContext ctx, MutableRootSector rootSector)
	{
		// Given the window is already on the workspace
		Workspace workspace = CreateWorkspace();
		IWindow window = CreateWindow((HWND)10);

		PopulateThreeWayMap(rootSector, CreateMonitor((HMONITOR)1), workspace, window);

		MoveWindowToWorkspaceTransform sut = new(workspace.Id, window.Handle);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_BothWorkspacesLayout(IContext ctx, MutableRootSector rootSector)
	{
		// Given the window switches workspaces, and both workspaces are visible
		Workspace workspace1 = CreateWorkspace();
		Workspace workspace2 = CreateWorkspace();

		IWindow window = CreateWindow((HWND)10);

		PopulateThreeWayMap(rootSector, CreateMonitor((HMONITOR)1), workspace1, window);
		PopulateMonitorWorkspaceMap(rootSector, CreateMonitor((HMONITOR)2), workspace2);

		MoveWindowToWorkspaceTransform sut = new(workspace2.Id, window.Handle);

		// When we move the window to the workspace
		// var result = AssertDoesNotRaise(ctx, rootSector, sut);
		Result<Unit>? result = null;
		CustomAssert.Layout(
			rootSector,
			() => result = AssertDoesNotRaise(ctx, rootSector, sut),
			[workspace1.Id, workspace2.Id]
		);

		// Then we succeed
		Assert.True(result!.Value.IsSuccessful);
		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as RemoveWindowFromWorkspaceTransform)
				== new RemoveWindowFromWorkspaceTransform(workspace1.Id, window) { SkipDoLayout = true }
		);
		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as AddWindowToWorkspaceTransform)
				== new AddWindowToWorkspaceTransform(workspace2.Id, window) { SkipDoLayout = true }
		);
		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as DoWorkspaceLayoutTransform) == new DoWorkspaceLayoutTransform(workspace1.Id)
		);
		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as DoWorkspaceLayoutTransform) == new DoWorkspaceLayoutTransform(workspace2.Id)
		);

		window.Received().Focus();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_SingleWorkspaceLayout(IContext ctx, MutableRootSector rootSector)
	{
		// Given the window gets activated on a hidden workspace
		Workspace workspace1 = CreateWorkspace();
		Workspace workspace2 = CreateWorkspace();

		IWindow window = CreateWindow((HWND)10);

		IMonitor monitor = CreateMonitor((HMONITOR)1);

		PopulateThreeWayMap(rootSector, monitor, workspace1, window);
		AddWorkspacesToStore(rootSector, workspace2);
		rootSector.MonitorSector.ActiveMonitorHandle = monitor.Handle;

		MoveWindowToWorkspaceTransform sut = new(workspace2.Id, window.Handle);

		// When we move the window to the workspace
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then we succeed
		Assert.True(result.IsSuccessful);

		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as RemoveWindowFromWorkspaceTransform)
				== new RemoveWindowFromWorkspaceTransform(workspace1.Id, window) { SkipDoLayout = true }
		);
		Assert.Contains(
			ctx.GetTransforms(),
			t =>
				(t as AddWindowToWorkspaceTransform)
				== new AddWindowToWorkspaceTransform(workspace2.Id, window) { SkipDoLayout = true }
		);
		Assert.Contains(
			ctx.GetTransforms(),
			t => (t as ActivateWorkspaceTransform) == new ActivateWorkspaceTransform(workspace2.Id)
		);

		Assert.Single(evs);

		window.Received().Focus();
	}
}
