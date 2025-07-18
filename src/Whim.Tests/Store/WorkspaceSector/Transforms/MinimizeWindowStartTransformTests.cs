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

		Workspace workspace = CreateWorkspace() with { LayoutEngines = [engine1, engine2] };
		workspace = PopulateWindowWorkspaceMap(root, window, workspace);

		MinimizeWindowStartTransform sut = new(workspace.Id, window.Handle);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then it succeeds
		Assert.True(result.IsSuccessful);

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

		Workspace workspace = CreateWorkspace() with { LayoutEngines = [engine1, engine2] };
		AddWorkspaceToStore(root, workspace);

		MinimizeWindowStartTransform sut = new(workspace.Id, window.Handle);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then it succeeds
		Assert.True(result.IsSuccessful);

		engine1.Received().MinimizeWindowStart(window);
		engine2.Received().MinimizeWindowStart(window);
	}
}
