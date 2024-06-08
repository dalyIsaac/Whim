using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNext;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspacePickersTests
{
	private static void CreateNamedWorkspaces(IContext ctx, MutableRootSector root)
	{
		AddWorkspacesToManager(
			ctx,
			root,
			CreateWorkspace(ctx) with
			{
				BackingName = "Test"
			},
			CreateWorkspace(ctx) with
			{
				BackingName = "Test2"
			},
			CreateWorkspace(ctx) with
			{
				BackingName = "Test3"
			}
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceById_Success(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		CreateNamedWorkspaces(ctx, root);

		Guid workspaceId = root.WorkspaceSector.WorkspaceOrder[0];

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceById(workspaceId));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(root.WorkspaceSector.Workspaces[workspaceId], result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceById_Failure(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		CreateNamedWorkspaces(ctx, root);

		Guid workspaceId = Guid.NewGuid();

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceById(workspaceId));

		// Then we don't succeed
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickAllWorkspaces(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		CreateNamedWorkspaces(ctx, root);

		// When we get the workspaces
		var result = ctx.Store.Pick(Pickers.PickAllWorkspaces()).ToArray();

		// Then we get the workspaces
		Assert.Equal(3, result.Length);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceByName_Success(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		CreateNamedWorkspaces(ctx, root);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceByName("Test"));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(root.WorkspaceSector.Workspaces[root.WorkspaceSector.WorkspaceOrder[0]], result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceByName_Failure(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		CreateNamedWorkspaces(ctx, root);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceByName("Bob the Builder"));

		// Then we don't succeed
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		AddWorkspacesToManager(ctx, root, CreateWorkspace(ctx), CreateWorkspace(ctx));

		Workspace activeWorkspace = CreateWorkspace(ctx);
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), activeWorkspace);

		// When we get the workspace
		IWorkspace result = ctx.Store.Pick(Pickers.PickActiveWorkspace());

		// Then we get the workspace
		Assert.Same(activeWorkspace, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveWorkspaceId(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		AddWorkspacesToManager(ctx, root, CreateWorkspace(ctx), CreateWorkspace(ctx));

		Workspace activeWorkspace = CreateWorkspace(ctx);
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), activeWorkspace);

		// When we get the workspace id
		Guid result = ctx.Store.Pick(Pickers.PickActiveWorkspaceId());

		// Then we get the workspace id
		Assert.Equal(activeWorkspace.Id, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveLayoutEngine(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine layoutEngine1,
		ILayoutEngine layoutEngine2
	)
	{
		// Given the workspaces
		AddWorkspacesToManager(ctx, root, CreateWorkspace(ctx), CreateWorkspace(ctx));

		Workspace activeWorkspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = ImmutableList.Create(layoutEngine1, layoutEngine2),
			ActiveLayoutEngineIndex = 1
		};
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), activeWorkspace);

		// When we get the layout engine
		Result<ILayoutEngine> result = ctx.Store.Pick(Pickers.PickActiveLayoutEngine(activeWorkspace.Id));

		// Then we get the layout engine
		Assert.True(result.IsSuccessful);
		Assert.Same(layoutEngine2, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickAllWindowsInWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows
		IMonitor monitor = CreateMonitor((HMONITOR)1);
		ImmutableDictionary<HWND, WindowPosition> windowPositions = ImmutableDictionary<HWND, WindowPosition>
			.Empty.Add((HWND)1, new())
			.Add((HWND)2, new())
			.Add((HWND)3, new());

		Workspace workspace = CreateWorkspace(ctx) with { WindowPositions = windowPositions };

		PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace);
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)1), workspace);
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)2), workspace);
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)3), workspace);

		// When we get the windows
		Result<IEnumerable<IWindow>> result = ctx.Store.Pick(Pickers.PickAllWindowsInWorkspace(workspace.Id));

		// Then we get the windows
		Assert.True(result.IsSuccessful);
		Assert.Equal(3, result.Value.Count());
	}

	private static IWindow Setup_LastFocusedWindow(IContext ctx, MutableRootSector root, Workspace workspace)
	{
		IMonitor monitor = CreateMonitor((HMONITOR)1);
		IWindow lastFocusedWindow = CreateWindow((HWND)2);

		PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)1), workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, lastFocusedWindow, workspace);
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)3), workspace);

		return lastFocusedWindow;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastFocusedWindow_Success(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows
		Workspace workspace = CreateWorkspace(ctx);
		IWindow lastFocusedWindow = Setup_LastFocusedWindow(ctx, root, workspace);

		root.WorkspaceSector.Workspaces = root.WorkspaceSector.Workspaces.SetItem(
			workspace.Id,
			workspace with
			{
				LastFocusedWindowHandle = lastFocusedWindow.Handle
			}
		);

		// When we get the last focused window
		Result<IWindow> result = ctx.Store.Pick(Pickers.PickLastFocusedWindow(workspace.Id));

		// Then we get the last focused window
		Assert.True(result.IsSuccessful);
		Assert.Same(lastFocusedWindow, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastFocusedWindow_WorkspaceNotFound(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows exist, but the workspace to search for doesn't exist
		Workspace workspace = CreateWorkspace(ctx);
		IWindow lastFocusedWindow = Setup_LastFocusedWindow(ctx, root, workspace);

		root.WorkspaceSector.Workspaces = root.WorkspaceSector.Workspaces.SetItem(
			workspace.Id,
			workspace with
			{
				LastFocusedWindowHandle = lastFocusedWindow.Handle
			}
		);

		Guid workspaceToSearchFor = Guid.NewGuid();

		// When we get the last focused window
		Result<IWindow> result = ctx.Store.Pick(Pickers.PickLastFocusedWindow(workspaceToSearchFor));

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastFocusedWindow_NoLastFocusedWindow(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows, but the last focused window isn't set
		Workspace workspace = CreateWorkspace(ctx);
		IWindow lastFocusedWindow = Setup_LastFocusedWindow(ctx, root, workspace);

		// When we get the last focused window
		Result<IWindow> result = ctx.Store.Pick(Pickers.PickLastFocusedWindow(workspace.Id));

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}
}
