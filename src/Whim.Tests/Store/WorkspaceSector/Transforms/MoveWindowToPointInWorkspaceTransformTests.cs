using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MoveWindowToPointInWorkspaceTransformTests
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
		HWND handle = (HWND)1;
		IWindow window = CreateWindow(handle);

		Workspace workspace = PopulateWindowWorkspaceMap(
			ctx,
			root,
			window,
			CreateWorkspace(ctx) with
			{
				LayoutEngines = ImmutableList.Create(engine1, engine2)
			}
		);
		Point<double> point = new(0.5, 0.5);

		MoveWindowToPointInWorkspaceTransform sut = new(workspace.Id, handle, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then it succeeds
		Assert.True(result.IsSuccessful);

		Workspace workspaceResult = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Contains(window.Handle, workspaceResult.WindowPositions.Keys);

		engine1.Received(1).MoveWindowToPoint(window, point);
		engine2.DidNotReceive().MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<double>>());
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
		HWND handle = (HWND)1;
		IWindow window = CreateWindow(handle);
		AddWindowToSector(root, window);

		Workspace workspace = CreateWorkspace(ctx) with { LayoutEngines = ImmutableList.Create(engine1, engine2) };
		AddWorkspaceToManager(ctx, root, workspace);
		Point<double> point = new(0.5, 0.5);

		MoveWindowToPointInWorkspaceTransform sut = new(workspace.Id, handle, point);

		// When we execute the transform
		var result = ctx.Store.Dispatch(sut);

		// Then it succeeds
		Assert.True(result.IsSuccessful);

		Workspace workspaceResult = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Contains(window.Handle, workspaceResult.WindowPositions.Keys);

		engine1.Received(1).MoveWindowToPoint(window, point);
		engine2.Received(1).MoveWindowToPoint(window, point);
	}
}
