namespace Whim.Tests;

public class FocusWindowInDirectionTransformTests
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

		Workspace workspace = PopulateWindowWorkspaceMap(
			root,
			window,
			CreateWorkspace() with
			{
				LayoutEngines = [engine1, engine2],
				ActiveLayoutEngineIndex = 1,
			}
		);

		engine2.FocusWindowInDirection(Arg.Any<Direction>(), Arg.Any<IWindow>()).Returns(engine2);

		// When we execute the transform
		var result = ctx.Store.Dispatch(new FocusWindowInDirectionTransform(workspace.Id, Direction.Down, handle));

		// Then it succeeds
		Assert.True(result.IsSuccessful);

		engine1.DidNotReceive().FocusWindowInDirection(Arg.Any<Direction>(), Arg.Any<IWindow>());
		engine2.Received().FocusWindowInDirection(Direction.Down, window);

		Workspace workspaceResult = root.WorkspaceSector.Workspaces[workspace.Id];

		Assert.Same(workspaceResult.LayoutEngines[0], workspace.LayoutEngines[0]);
		Assert.Same(workspaceResult.LayoutEngines[1], workspace.LayoutEngines[1]);
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

		Workspace workspace = PopulateWindowWorkspaceMap(
			root,
			window,
			CreateWorkspace() with
			{
				LayoutEngines = [engine1, engine2],
				ActiveLayoutEngineIndex = 1,
			}
		);

		// When we execute the transform
		var result = ctx.Store.Dispatch(new FocusWindowInDirectionTransform(workspace.Id, Direction.Down, handle));

		// Then it succeeds
		Assert.True(result.IsSuccessful);

		engine1.DidNotReceive().FocusWindowInDirection(Arg.Any<Direction>(), Arg.Any<IWindow>());
		engine2.Received().FocusWindowInDirection(Direction.Down, window);

		Workspace workspaceResult = root.WorkspaceSector.Workspaces[workspace.Id];

		Assert.Same(workspaceResult.LayoutEngines[0], workspace.LayoutEngines[0]);
		Assert.NotSame(workspaceResult.LayoutEngines[1], workspace.LayoutEngines[1]);
	}
}
