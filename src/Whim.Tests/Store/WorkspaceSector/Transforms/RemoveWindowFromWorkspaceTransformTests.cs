namespace Whim.Tests;

public class RemoveWindowFromWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotFound(IContext ctx, MutableRootSector root, IWindow window)
	{
		// Given the window is not found
		window.Handle.Returns((HWND)3);

		Workspace workspace = CreateWorkspace();
		workspace = PopulateWindowWorkspaceMap(root, CreateWindow((HWND)1), workspace);
		workspace = PopulateWindowWorkspaceMap(root, CreateWindow((HWND)2), workspace);

		RemoveWindowFromWorkspaceTransform sut = new(workspace.Id, window);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get false
		Assert.True(result.IsSuccessful);
		Assert.False(result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowIsNotLastFocused(IContext ctx, MutableRootSector root, IWindow window)
	{
		// Given the window is not the last focused window
		window.Handle.Returns((HWND)1);

		Workspace workspace = CreateWorkspace() with { LastFocusedWindowHandle = (HWND)2 };

		workspace = PopulateWindowWorkspaceMap(root, CreateWindow((HWND)1), workspace);
		workspace = PopulateWindowWorkspaceMap(root, CreateWindow((HWND)2), workspace);
		workspace = PopulateWindowWorkspaceMap(root, CreateWindow((HWND)3), workspace);

		RemoveWindowFromWorkspaceTransform sut = new(workspace.Id, window);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get true
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);

		Workspace resultingWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Equal(2, resultingWorkspace.WindowPositions.Count);
		Assert.DoesNotContain(window.Handle, resultingWorkspace.WindowPositions);
		Assert.Equal((HWND)2, resultingWorkspace.LastFocusedWindowHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LastFocusedWindowIsLastWindow(IContext ctx, MutableRootSector root, IWindow window)
	{
		// Given the window is the last focused window and the last window in the workspace
		window.Handle.Returns((HWND)1);
		Workspace workspace = CreateWorkspace() with { LastFocusedWindowHandle = window.Handle };

		workspace = PopulateWindowWorkspaceMap(root, CreateWindow(window.Handle), workspace);

		RemoveWindowFromWorkspaceTransform sut = new(workspace.Id, window);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get true
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);

		Workspace resultingWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Empty(resultingWorkspace.WindowPositions);
		Assert.Equal(default, resultingWorkspace.LastFocusedWindowHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void UpdateLastFocusedWindow(IContext ctx, MutableRootSector root, IWindow window)
	{
		// Given the window is the last focused window and the window is not the last window in the workspace
		window.Handle.Returns((HWND)1);
		Workspace workspace = CreateWorkspace() with
		{
			LastFocusedWindowHandle = window.Handle,
			WindowPositions = ImmutableDictionary<HWND, WindowPosition>.Empty.Add(
				(HWND)3,
				new WindowPosition { WindowSize = WindowSize.Normal }
			),
		};

		workspace = PopulateWindowWorkspaceMap(root, CreateWindow(window.Handle), workspace);
		workspace = PopulateWindowWorkspaceMap(root, CreateWindow((HWND)2), workspace);
		// HWND 3 is minimized and has already been added to the workspace
		AddWindowToSector(root, CreateWindow((HWND)3));

		RemoveWindowFromWorkspaceTransform sut = new(workspace.Id, window);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get true
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);

		Workspace resultingWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Equal(2, resultingWorkspace.WindowPositions.Count);
		Assert.DoesNotContain(window.Handle, resultingWorkspace.WindowPositions);
		Assert.Equal((HWND)3, resultingWorkspace.LastFocusedWindowHandle);
	}
}
