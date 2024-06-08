using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DotNext;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class RemoveWindowFromWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotFound(IContext ctx, MutableRootSector root)
	{
		// Given the window is not found
		Workspace workspace = CreateWorkspace(ctx);
		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)1), workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)2), workspace);

		RemoveWindowFromWorkspaceTransform sut = new(workspace.Id, (HWND)3);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get false
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowIsNotLastFocused(IContext ctx, MutableRootSector root)
	{
		// Given the window is not the last focused window
		HWND handleToRemove = (HWND)1;
		Workspace workspace = CreateWorkspace(ctx) with { LastFocusedWindowHandle = (HWND)2 };

		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)1), workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)2), workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)3), workspace);

		RemoveWindowFromWorkspaceTransform sut = new(workspace.Id, handleToRemove);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get true
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);

		Workspace resultingWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Equal(2, resultingWorkspace.WindowPositions.Count);
		Assert.DoesNotContain(handleToRemove, resultingWorkspace.WindowPositions);
		Assert.Equal((HWND)2, resultingWorkspace.LastFocusedWindowHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LastFocusedWindowIsLastWindow(IContext ctx, MutableRootSector root)
	{
		// Given the window is the last focused window and the last window in the workspace
		HWND handleToRemove = (HWND)1;
		Workspace workspace = CreateWorkspace(ctx) with { LastFocusedWindowHandle = handleToRemove };

		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow(handleToRemove), workspace);

		RemoveWindowFromWorkspaceTransform sut = new(workspace.Id, (HWND)1);

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
	internal void UpdateLastFocusedWindow(IContext ctx, MutableRootSector root)
	{
		// Given the window is the last focused window and the window is not the last window in the workspace
		HWND handleToRemove = (HWND)1;
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LastFocusedWindowHandle = handleToRemove,
			WindowPositions = ImmutableDictionary<HWND, WindowPosition>.Empty.Add(
				(HWND)3,
				new WindowPosition { WindowSize = WindowSize.Normal }
			)
		};

		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow(handleToRemove), workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)2), workspace);
		// HWND 3 is minimized and has already been added to the workspace
		AddWindowToSector(root, CreateWindow((HWND)3));

		RemoveWindowFromWorkspaceTransform sut = new(workspace.Id, handleToRemove);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get true
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);

		Workspace resultingWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Equal(2, resultingWorkspace.WindowPositions.Count);
		Assert.DoesNotContain(handleToRemove, resultingWorkspace.WindowPositions);
		Assert.Equal((HWND)3, resultingWorkspace.LastFocusedWindowHandle);
	}
}
