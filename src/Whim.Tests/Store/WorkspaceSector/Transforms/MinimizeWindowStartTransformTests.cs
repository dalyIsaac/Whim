namespace Whim.Tests;

public class MinimizeWindowStartTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowAlreadyInWorkspace(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine engine1,
		ILayoutEngine engine2
	)
	{
		// Given the window is already in the workspace
		IWindow window = CreateWindow((HWND)1);
		IMonitor monitor = CreateMonitor((HMONITOR)1);

		// Configure engines to return empty layout
		engine1.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>()).Returns([]);
		engine2.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>()).Returns([]);

		Workspace workspace = CreateWorkspace() with { LayoutEngines = [engine1, engine2] };
		workspace = PopulateThreeWayMap(root, monitor, workspace, window);

		MinimizeWindowStartTransform sut = new(workspace.Id, window.Handle);

		// When we execute the transform
		Result<bool>? result = null;
		CustomAssert.Layout(root, () => result = ctx.Store.Dispatch(sut), [workspace.Id]);

		// Then it succeeds
		Assert.True(result!.Value.IsSuccessful);

		engine1.Received().MinimizeWindowStart(window);
		engine2.DidNotReceive().MinimizeWindowStart(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotInWorkspace(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine engine1,
		ILayoutEngine engine2
	)
	{
		// Given the window is not in the workspace
		IWindow window = CreateWindow((HWND)1);
		AddWindowToSector(root, window);

		// Configure engines to return empty layout
		engine1.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>()).Returns([]);
		engine2.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>()).Returns([]);

		IMonitor monitor = CreateMonitor((HMONITOR)1);
		Workspace workspace = CreateWorkspace() with { LayoutEngines = [engine1, engine2] };
		PopulateMonitorWorkspaceMap(root, monitor, workspace);
		AddWorkspaceToStore(root, workspace);

		MinimizeWindowStartTransform sut = new(workspace.Id, window.Handle);

		// When we execute the transform
		Result<bool>? result = null;
		CustomAssert.Layout(root, () => result = ctx.Store.Dispatch(sut), [workspace.Id]);

		// Then it succeeds
		Assert.True(result!.Value.IsSuccessful);

		engine1.Received().MinimizeWindowStart(window);
		engine2.Received().MinimizeWindowStart(window);
	}
}
