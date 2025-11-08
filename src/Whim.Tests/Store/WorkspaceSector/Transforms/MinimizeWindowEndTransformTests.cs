namespace Whim.Tests;

public class MinimizeWindowEndTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowAlreadyFocused(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine engine1,
		ILayoutEngine engine2
	)
	{
		// Given the window is already focused in the workspace, as indicated by the active layout engine
		// not mutating.
		HWND handle = (HWND)1;
		IWindow window = CreateWindow(handle);
		IMonitor monitor = CreateMonitor((HMONITOR)1);

		// Configure engines to return empty layout
		engine1.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>()).Returns([]);
		engine2.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>()).Returns([]);

		Workspace workspace = PopulateThreeWayMap(
			root,
			monitor,
			CreateWorkspace() with
			{
				LayoutEngines = [engine1, engine2],
				ActiveLayoutEngineIndex = 1,
			},
			window
		);

		engine2.MinimizeWindowEnd(Arg.Any<IWindow>()).Returns(engine2);

		// When we execute the transform - since the layout engine doesn't change, no layout should occur
		Result<bool> result = ctx.Store.Dispatch(new MinimizeWindowEndTransform(workspace.Id, handle));

		// Then it succeeds
		Assert.True(result.IsSuccessful);

		engine1.DidNotReceive().MinimizeWindowEnd(Arg.Any<IWindow>());
		engine2.Received().MinimizeWindowEnd(window);

		Workspace workspaceResult = root.WorkspaceSector.Workspaces[workspace.Id];

		Assert.Same(workspaceResult.LayoutEngines[0], workspace.LayoutEngines[0]);
		Assert.Same(workspaceResult.LayoutEngines[1], workspace.LayoutEngines[1]);

		// Verify no layout was triggered since workspace didn't change
		Assert.Empty(root.WorkspaceSector.WorkspacesToLayout);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotAlreadyFocused(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine engine1,
		ILayoutEngine engine2
	)
	{
		// Given the window is not already focused in the workspace, as indicated by the active layout engine
		// mutating.
		HWND handle = (HWND)1;
		IWindow window = CreateWindow(handle);
		IMonitor monitor = CreateMonitor((HMONITOR)1);

		// Configure engines to return empty layout
		engine1.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>()).Returns([]);
		engine2.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>()).Returns([]);

		Workspace workspace = PopulateThreeWayMap(
			root,
			monitor,
			CreateWorkspace() with
			{
				LayoutEngines = [engine1, engine2],
				ActiveLayoutEngineIndex = 1,
			},
			window
		);

		// When we execute the transform
		Result<bool>? result = null;
		CustomAssert.Layout(
			root,
			() => result = ctx.Store.Dispatch(new MinimizeWindowEndTransform(workspace.Id, handle)),
			[workspace.Id]
		);

		// Then it succeeds
		Assert.True(result!.Value.IsSuccessful);

		engine1.DidNotReceive().MinimizeWindowEnd(Arg.Any<IWindow>());
		engine2.Received().MinimizeWindowEnd(window);

		Workspace workspaceResult = root.WorkspaceSector.Workspaces[workspace.Id];

		Assert.Same(workspaceResult.LayoutEngines[0], workspace.LayoutEngines[0]);
		Assert.NotSame(workspaceResult.LayoutEngines[1], workspace.LayoutEngines[1]);

		// Verify WindowPositions was updated
		Assert.True(workspaceResult.WindowPositions.ContainsKey(handle));
		Assert.Equal(new WindowPosition(), workspaceResult.WindowPositions[handle]);
	}
}
