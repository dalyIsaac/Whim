using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspaceUtilsTests
{
	[Theory, AutoSubstituteData]
	public void OrActiveWorkspace_ReturnsActiveWorkspace(IContext ctx)
	{
		// Given the workspace id is default
		Guid workspaceId = default;
		Guid activeWorkspaceId = Guid.NewGuid();
		ctx.WorkspaceManager.ActiveWorkspace.Returns(CreateWorkspace(ctx) with { Id = activeWorkspaceId });

		// When
		Guid result = WorkspaceUtils.OrActiveWorkspace(workspaceId, ctx);

		// Then
		Assert.Equal(activeWorkspaceId, result);
	}

	[Theory, AutoSubstituteData]
	public void OrActiveWorkspace_ReturnsProvidedWorkspace(IContext ctx)
	{
		// Given the workspace id is not default
		Guid workspaceId = Guid.NewGuid();
		Guid activeWorkspaceId = Guid.NewGuid();
		ctx.WorkspaceManager.ActiveWorkspace.Returns(CreateWorkspace(ctx) with { Id = activeWorkspaceId });

		// When
		Guid result = WorkspaceUtils.OrActiveWorkspace(workspaceId, ctx);

		// Then
		Assert.Equal(workspaceId, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SetActiveLayoutEngine(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine engine1,
		ILayoutEngine engine2,
		ILayoutEngine engine3
	)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = ImmutableList.Create(engine1, engine2, engine3),
			ActiveLayoutEngineIndex = 1
		};
		int newActiveLayoutEngineIndex = 2;

		// When
		Workspace result = WorkspaceUtils.SetActiveLayoutEngine(
			root.WorkspaceSector,
			workspace,
			newActiveLayoutEngineIndex
		);

		Assert.Raises<ActiveLayoutEngineChangedEventArgs>(
			h => ctx.Store.WorkspaceEvents.ActiveLayoutEngineChanged += h,
			h => ctx.Store.WorkspaceEvents.ActiveLayoutEngineChanged -= h,
			root.WorkspaceSector.DispatchEvents
		);

		// Then
		Assert.NotSame(workspace, result);
		Assert.Equal(2, result.ActiveLayoutEngineIndex);
		Assert.Equal(workspace.Id, result.Id);
	}

	[Theory, AutoSubstituteData]
	internal void GetValidWorkspaceWindow_NoWindowProvided(IContext ctx)
	{
		// Given the handle is null and we don't default to the last focused window
		HWND windowHandle = default;
		bool defaultToLastFocusedWindow = false;

		// When
		Result<IWindow> result = WorkspaceUtils.GetValidWorkspaceWindow(
			ctx,
			CreateWorkspace(ctx),
			windowHandle,
			defaultToLastFocusedWindow
		);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData]
	internal void GetValidWorkspaceWindow_NoWindowsInWorkspace(IContext ctx)
	{
		// Given the handle is null and we do default to the last focused window
		HWND windowHandle = default;
		bool defaultToLastFocusedWindow = true;

		// When there are no windows
		Result<IWindow> result = WorkspaceUtils.GetValidWorkspaceWindow(
			ctx,
			CreateWorkspace(ctx),
			windowHandle,
			defaultToLastFocusedWindow
		);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData]
	internal void GetValidWorkspaceWindow_WindowNotInWorkspace(IContext ctx)
	{
		// Given the handle is not null, but the window is not in the workspace
		HWND windowHandle = new(1);
		Workspace workspace = CreateWorkspace(ctx);

		// When the window is not in the workspace
		Result<IWindow> result = WorkspaceUtils.GetValidWorkspaceWindow(
			ctx,
			workspace,
			windowHandle,
			defaultToLastFocusedWindow: false
		);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetValidWorkspaceWindow_WindowInWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given the handle is provided and the window is in the window sector
		IWindow window = CreateWindow((HWND)1);
		AddWindowToSector(root, window);
		Workspace workspace = CreateWorkspace(ctx) with
		{
			WindowPositions = ImmutableDictionary<HWND, WindowPosition>.Empty.Add(window.Handle, new())
		};

		// When the window is in the workspace
		Result<IWindow> result = WorkspaceUtils.GetValidWorkspaceWindow(
			ctx,
			workspace,
			window.Handle,
			defaultToLastFocusedWindow: false
		);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Same(window, result.Value);
	}
}
