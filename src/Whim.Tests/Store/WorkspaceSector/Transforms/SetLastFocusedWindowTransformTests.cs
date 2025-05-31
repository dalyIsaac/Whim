using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class SetLastFocusedWindowTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoChanges(IContext ctx, MutableRootSector root)
	{
		// Given the last focused window is the same as the window we're setting.
		IWindow window = CreateWindow((HWND)123);
		Workspace workspace = CreateWorkspace(ctx) with { LastFocusedWindowHandle = window.Handle };

		workspace = PopulateWindowWorkspaceMap(ctx, root, window, workspace);

		SetLastFocusedWindowTransform sut = new(workspace.Id, window.Handle);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then there are no changes
		Assert.True(result.IsSuccessful);
		Assert.False(result.Value);

		Workspace resultWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Equal(window.Handle, resultWorkspace.LastFocusedWindowHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector root)
	{
		// Given the last focused window is not the same as the window we're setting.
		IWindow window = CreateWindow((HWND)123);
		Workspace workspace = CreateWorkspace(ctx) with { LastFocusedWindowHandle = (HWND)456 };

		workspace = PopulateWindowWorkspaceMap(ctx, root, window, workspace);

		SetLastFocusedWindowTransform sut = new(workspace.Id, window.Handle);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then there are changes
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);

		Workspace resultWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Equal(window.Handle, resultWorkspace.LastFocusedWindowHandle);
	}
}
