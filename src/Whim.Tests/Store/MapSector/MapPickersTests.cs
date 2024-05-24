using System;
using System.Linq;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MapPickersTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickAllActiveWorkspaces(IContext ctx, MutableRootSector root)
	{
		// Given there are three active workspaces
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), CreateWorkspace());
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)2), CreateWorkspace());
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)3), CreateWorkspace());

		// When we get the workspaces
		var result = ctx.Store.Pick(Pickers.PickAllActiveWorkspaces());

		// Then we get the workspaces
		Assert.Equal(3, result.Count());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceById_Failure(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), CreateWorkspace());
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)2), CreateWorkspace());
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)3), CreateWorkspace());

		Guid workspaceId = Guid.NewGuid();

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceById(workspaceId));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceById_Success(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		var workspace1 = CreateWorkspace();
		var workspace2 = CreateWorkspace();
		var workspace3 = CreateWorkspace();

		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), workspace1);
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)2), workspace2);
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)3), workspace3);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceById(workspace1.Id));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(workspace1, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceByMonitor(IContext ctx, MutableRootSector root)
	{
		// Given there is a workspace
		HMONITOR handle = (HMONITOR)1;

		var workspace = CreateWorkspace();
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor(handle), workspace);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceByMonitor(handle));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(workspace, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceByWindow(IContext ctx, MutableRootSector root)
	{
		// Given there is a workspace
		HWND handle = (HWND)1;

		var workspace = CreateWorkspace();
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow(handle), workspace);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceByWindow(handle));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(workspace, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickMonitorByWorkspace_Failure(IContext ctx, MutableRootSector root)
	{
		// Given we have an untracked workspace
		var untrackedWorkspace = CreateWorkspace();
		var workspace = CreateWorkspace();
		root.MapSector.MonitorWorkspaceMap = root.MapSector.MonitorWorkspaceMap.SetItem((HMONITOR)1, workspace.Id);

		// When we get the monitor
		var result = ctx.Store.Pick(Pickers.PickMonitorByWorkspace(untrackedWorkspace.Id));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickMonitorByWorkspace_Success(IContext ctx, MutableRootSector root)
	{
		// Given we have a tracked workspace
		var workspace = CreateWorkspace();
		root.MapSector.MonitorWorkspaceMap = root.MapSector.MonitorWorkspaceMap.SetItem((HMONITOR)1, workspace.Id);

		IMonitor monitor = CreateMonitor((HMONITOR)1);
		root.MonitorSector.Monitors = root.MonitorSector.Monitors.Add(monitor);

		// When we get the monitor
		var result = ctx.Store.Pick(Pickers.PickMonitorByWorkspace(workspace.Id));

		// Then we get the monitor
		Assert.True(result.IsSuccessful);
		Assert.Same(monitor, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickMonitorByWindow_Failure(IContext ctx, MutableRootSector root)
	{
		// Given we have an untracked window handle
		HWND hwnd = (HWND)10;
		HWND untrackedHwnd = (HWND)20;

		PopulateThreeWayMap(ctx, root, CreateMonitor((HMONITOR)1), CreateWorkspace(), CreateWindow(untrackedHwnd));

		// When we get the monitor
		var result = ctx.Store.Pick(Pickers.PickMonitorByWindow(hwnd));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickMonitorByWindow_Success(IContext ctx, MutableRootSector root)
	{
		// Given we have a tracked monitor handle
		HWND hwnd1 = (HWND)1;
		HWND hwnd2 = (HWND)2;

		IMonitor monitor1 = CreateMonitor((HMONITOR)1);
		IMonitor monitor2 = CreateMonitor((HMONITOR)2);

		PopulateThreeWayMap(ctx, root, monitor1, CreateWorkspace(), CreateWindow(hwnd1));
		PopulateThreeWayMap(ctx, root, monitor2, CreateWorkspace(), CreateWindow(hwnd2));

		// When we get the monitor
		var result = ctx.Store.Pick(Pickers.PickMonitorByWindow(hwnd1));

		// Then we get the monitor
		Assert.True(result.IsSuccessful);
		Assert.Same(monitor1, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickAdjacentWorkspace_CouldNotFindWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given we have an untracked workspace
		var untrackedWorkspace = CreateWorkspace();
		var workspace = CreateWorkspace();
		root.MapSector.MonitorWorkspaceMap = root.MapSector.MonitorWorkspaceMap.SetItem((HMONITOR)1, workspace.Id);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickAdjacentWorkspace(untrackedWorkspace.Id));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickAdjacentWorkspace_NoAdjacentWorkspaces(IContext ctx, MutableRootSector root)
	{
		// Given we have a tracked workspace
		var workspace = CreateWorkspace();
		root.MapSector.MonitorWorkspaceMap = root.MapSector.MonitorWorkspaceMap.SetItem((HMONITOR)1, workspace.Id);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickAdjacentWorkspace(workspace.Id));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>(0, 0, false, false, 1)]
	[InlineAutoSubstituteData<StoreCustomization>(0, 0, true, false, 3)]
	[InlineAutoSubstituteData<StoreCustomization>(3, 3, false, false, 0)]
	[InlineAutoSubstituteData<StoreCustomization>(3, 3, true, false, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(0, 1, false, true, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(1, 0, true, true, 3)]
	[InlineAutoSubstituteData<StoreCustomization>(3, 0, false, true, 1)]
	[InlineAutoSubstituteData<StoreCustomization>(3, 2, true, true, 1)]
	internal void PickAdjacentWorkspace_Success(
		int startIdx,
		int activeIdx,
		bool reverse,
		bool skipActive,
		int expected,
		IContext ctx,
		MutableRootSector root
	)
	{
		// Given we have four workspaces
		PopulateThreeWayMap(ctx, root, CreateMonitor((HMONITOR)1), CreateWorkspace(), CreateWindow((HWND)1));
		PopulateThreeWayMap(ctx, root, CreateMonitor((HMONITOR)2), CreateWorkspace(), CreateWindow((HWND)2));
		PopulateThreeWayMap(ctx, root, CreateMonitor((HMONITOR)3), CreateWorkspace(), CreateWindow((HWND)3));
		PopulateThreeWayMap(ctx, root, CreateMonitor((HMONITOR)4), CreateWorkspace(), CreateWindow((HWND)4));

		IWorkspace[] workspaces = ctx.WorkspaceManager.ToArray();
		ctx.WorkspaceManager.ActiveWorkspace.Returns(workspaces[activeIdx]);

		Guid workspaceId = workspaces[startIdx].Id;

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickAdjacentWorkspace(workspaceId, reverse, skipActive));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(workspaces[expected], result.Value);
	}
}
