using System.Linq;
using FluentAssertions;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MapPickersTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickAllActiveWorkspaces(IContext ctx, MutableRootSector root)
	{
		// Given there are three active workspaces
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), CreateWorkspace(ctx));
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)2), CreateWorkspace(ctx));
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)3), CreateWorkspace(ctx));

		// When we get the workspaces
		var result = ctx.Store.Pick(Pickers.PickAllActiveWorkspaces());

		// Then we get the workspaces
		Assert.Equal(3, result.Count());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceById_Failure(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), CreateWorkspace(ctx));
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)2), CreateWorkspace(ctx));
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)3), CreateWorkspace(ctx));

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
		var workspace1 = CreateWorkspace(ctx);
		var workspace2 = CreateWorkspace(ctx);
		var workspace3 = CreateWorkspace(ctx);

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

		var workspace = CreateWorkspace(ctx);
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
		Workspace workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow(handle), CreateWorkspace(ctx));

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
		var untrackedWorkspace = CreateWorkspace(ctx);
		var workspace = CreateWorkspace(ctx);
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
		var workspace = CreateWorkspace(ctx);
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

		PopulateThreeWayMap(ctx, root, CreateMonitor((HMONITOR)1), CreateWorkspace(ctx), CreateWindow(untrackedHwnd));

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

		PopulateThreeWayMap(ctx, root, monitor1, CreateWorkspace(ctx), CreateWindow(hwnd1));
		PopulateThreeWayMap(ctx, root, monitor2, CreateWorkspace(ctx), CreateWindow(hwnd2));

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
		var untrackedWorkspace = CreateWorkspace(ctx);
		var workspace = CreateWorkspace(ctx);
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
		var workspace = CreateWorkspace(ctx);
		root.MapSector.MonitorWorkspaceMap = root.MapSector.MonitorWorkspaceMap.SetItem((HMONITOR)1, workspace.Id);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickAdjacentWorkspace(workspace.Id));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	public static TheoryData<int, int[], bool, bool, int> PickAdjacentWorkspaceData =>
		new()
		{
			{ 0, new[] { 0 }, false, false, 1 },
			{ 0, new[] { 0 }, true, false, 3 },
			{ 3, new[] { 3 }, false, false, 0 },
			{ 3, new[] { 3 }, true, false, 2 },
			{ 0, new[] { 1 }, false, true, 2 },
			{ 1, new[] { 0 }, true, true, 3 },
			{ 3, new[] { 0 }, false, true, 1 },
			{ 3, new[] { 2 }, true, true, 1 },
			// Multiple active, skip active.
			{ 0, new[] { 0, 1 }, false, true, 2 },
			{ 1, new[] { 0, 1 }, true, true, 3 },
			{ 3, new[] { 0, 2 }, false, true, 1 },
			{ 3, new[] { 2, 3 }, true, true, 1 },
		};

	[Theory]
	[MemberAutoSubstituteData<StoreCustomization>(nameof(PickAdjacentWorkspaceData))]
	internal void PickAdjacentWorkspace_Success(
		int startIdx,
		int[] activeIdx,
		bool reverse,
		bool skipActive,
		int expected,
		IContext ctx,
		MutableRootSector root
	)
	{
		// Given we have four workspaces
		AddWorkspacesToManager(
			ctx,
			root,
			CreateWorkspace(ctx),
			CreateWorkspace(ctx),
			CreateWorkspace(ctx),
			CreateWorkspace(ctx)
		);

		ImmutableArray<Guid> workspaceOrder = root.WorkspaceSector.WorkspaceOrder;
		Guid startId = workspaceOrder[startIdx];
		Random gen = new();

		for (int idx = 0; idx < activeIdx.Length; idx++)
		{
			root.MapSector.MonitorWorkspaceMap = root.MapSector.MonitorWorkspaceMap.SetItem(
				(HMONITOR)idx,
				workspaceOrder[activeIdx[idx]]
			);
		}

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickAdjacentWorkspace(startId, reverse, skipActive));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Equal(workspaceOrder[expected], result.Value.Id);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveLayoutEngineByMonitor_Failure(IContext ctx)
	{
		// Given we have an untracked monitor
		IMonitor monitor = CreateMonitor((HMONITOR)1);

		// When we get the layout engine
		var result = ctx.Store.Pick(Pickers.PickActiveLayoutEngineByMonitor(monitor.Handle));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveLayoutEngineByMonitor_NoWorkspaceForMonitor(IContext ctx, MutableRootSector root)
	{
		// Given we have a monitor with no workspace
		IMonitor monitor = CreateMonitor((HMONITOR)1);
		AddMonitorsToManager(ctx, root, monitor);

		// When we get the layout engine
		var result = ctx.Store.Pick(Pickers.PickActiveLayoutEngineByMonitor(monitor.Handle));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveLayoutEngineByMonitor_Success(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine layoutEngine
	)
	{
		// Given we have a monitor with an active layout engine
		IMonitor monitor = CreateMonitor((HMONITOR)1);
		Workspace workspace = CreateWorkspace(ctx) with { LayoutEngines = [layoutEngine] };

		PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace);

		// When we get the layout engine
		var result = ctx.Store.Pick(Pickers.PickActiveLayoutEngineByMonitor(monitor.Handle));

		// Then we get the layout engine
		Assert.True(result.IsSuccessful);
		Assert.Same(layoutEngine, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickStickyMonitorsByWorkspace_Failure(IContext ctx)
	{
		// Given we have an untracked workspace
		var untrackedWorkspace = CreateWorkspace(ctx);

		// When we get the monitors
		var result = ctx.Store.Pick(Pickers.PickStickyMonitorsByWorkspace(untrackedWorkspace.Id));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickStickyMonitorsByWorkspace_StickyWorkspaceMonitorMapIsEmpty(IContext ctx, MutableRootSector root)
	{
		// Given we have a sticky workspace
		var workspace = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, root, workspace);
		AddMonitorsToManager(
			ctx,
			root,
			CreateMonitor((HMONITOR)0),
			CreateMonitor((HMONITOR)1),
			CreateMonitor((HMONITOR)2)
		);

		root.MapSector.StickyWorkspaceMonitorIndexMap = root.MapSector.StickyWorkspaceMonitorIndexMap.SetItem(
			workspace.Id,
			[0, 1]
		);

		// When we get the monitors
		var result = ctx.Store.Pick(Pickers.PickStickyMonitorsByWorkspace(workspace.Id));

		// Then we get the monitors
		Assert.True(result.IsSuccessful);
		Assert.Equal(2, result.Value.Count);
		result.Value.Should().BeEquivalentTo([(HMONITOR)0, (HMONITOR)1]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickStickyMonitorsByWorkspace_Success(IContext ctx, MutableRootSector root)
	{
		// Given we have workspace which isn't sticky
		var workspace = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, root, workspace);
		AddMonitorsToManager(
			ctx,
			root,
			CreateMonitor((HMONITOR)0),
			CreateMonitor((HMONITOR)1),
			CreateMonitor((HMONITOR)2)
		);

		// When we get the monitors
		var result = ctx.Store.Pick(Pickers.PickStickyMonitorsByWorkspace(workspace.Id));

		// Then we get all the monitors
		Assert.True(result.IsSuccessful);
		Assert.Equal(3, result.Value.Count);
		result.Value.Should().BeEquivalentTo([(HMONITOR)0, (HMONITOR)1, (HMONITOR)2]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickStickyMonitorsByWorkspace_NoMonitors(IContext ctx, MutableRootSector root)
	{
		// Given we have workspace which is sticky, but is stuck to non-existent monitors
		var workspace = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, root, workspace);
		AddMonitorsToManager(
			ctx,
			root,
			CreateMonitor((HMONITOR)0),
			CreateMonitor((HMONITOR)1),
			CreateMonitor((HMONITOR)2)
		);

		root.MapSector.StickyWorkspaceMonitorIndexMap = root.MapSector.StickyWorkspaceMonitorIndexMap.SetItem(
			workspace.Id,
			[3, 4]
		);

		// When we get the monitors
		var result = ctx.Store.Pick(Pickers.PickStickyMonitorsByWorkspace(workspace.Id));

		// Then we get all the monitors
		Assert.True(result.IsSuccessful);
		Assert.Equal(3, result.Value.Count);
		result.Value.Should().BeEquivalentTo([(HMONITOR)0, (HMONITOR)1, (HMONITOR)2]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickExplicitStickyMonitorIndicesByWorkspace_Failure(IContext ctx)
	{
		// Given we have an untracked workspace
		var untrackedWorkspace = CreateWorkspace(ctx);

		// When we get the monitor indices
		var result = ctx.Store.Pick(Pickers.PickExplicitStickyMonitorIndicesByWorkspace(untrackedWorkspace.Id));

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickExplicitStickyMonitorIndicesByWorkspace_Success(IContext ctx, MutableRootSector root)
	{
		// Given we have workspace which is sticky
		var workspace = CreateWorkspace(ctx);
		root.MapSector.StickyWorkspaceMonitorIndexMap = root.MapSector.StickyWorkspaceMonitorIndexMap.SetItem(
			workspace.Id,
			[0, 1]
		);

		// When we get the monitor indices
		var result = ctx.Store.Pick(Pickers.PickExplicitStickyMonitorIndicesByWorkspace(workspace.Id));

		// Then we get the monitor indices
		Assert.True(result.IsSuccessful);
		Assert.Equal(2, result.Value.Count);
		result.Value.Should().BeEquivalentTo([0, 1]);
	}
}

public class PickValidMonitorByWorkspaceTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void TargetMonitorIsValid(IContext ctx, MutableRootSector root)
	{
		// Given we have a workspace and target monitor that is valid for it
		var workspace = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, root, workspace);
		AddMonitorsToManager(ctx, root, CreateMonitor((HMONITOR)1), CreateMonitor((HMONITOR)2));

		root.MapSector.StickyWorkspaceMonitorIndexMap = root.MapSector.StickyWorkspaceMonitorIndexMap.SetItem(
			workspace.Id,
			[0, 1]
		);

		// When we get the monitor
		var result = ctx.Store.Pick(Pickers.PickValidMonitorByWorkspace(workspace.Id, (HMONITOR)1));

		// Then we get the target monitor
		Assert.True(result.IsSuccessful);
		Assert.Equal((HMONITOR)1, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FallbackToLastMonitor(IContext ctx, MutableRootSector root)
	{
		// Given we have a workspace with an invalid target monitor but valid last monitor
		var workspace = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, root, workspace);
		AddMonitorsToManager(ctx, root, CreateMonitor((HMONITOR)1), CreateMonitor((HMONITOR)2));

		root.MapSector.StickyWorkspaceMonitorIndexMap = root.MapSector.StickyWorkspaceMonitorIndexMap.SetItem(
			workspace.Id,
			[1] // Only monitor 2 is valid
		);

		root.MapSector.WorkspaceLastMonitorMap = root.MapSector.WorkspaceLastMonitorMap.SetItem(
			workspace.Id,
			(HMONITOR)2
		);

		// When we try to use monitor 1
		var result = ctx.Store.Pick(Pickers.PickValidMonitorByWorkspace(workspace.Id, (HMONITOR)1));

		// Then we get monitor 2 since it's the last valid monitor used
		Assert.True(result.IsSuccessful);
		Assert.Equal((HMONITOR)2, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FallbackToFirstAvailableMonitor(IContext ctx, MutableRootSector root)
	{
		// Given we have a workspace with invalid target and last monitors
		var workspace = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, root, workspace);
		AddMonitorsToManager(ctx, root, CreateMonitor((HMONITOR)1), CreateMonitor((HMONITOR)2));

		root.MapSector.StickyWorkspaceMonitorIndexMap = root.MapSector.StickyWorkspaceMonitorIndexMap.SetItem(
			workspace.Id,
			[0] // Only monitor 1 is valid
		);

		root.MapSector.WorkspaceLastMonitorMap = root.MapSector.WorkspaceLastMonitorMap.SetItem(
			workspace.Id,
			(HMONITOR)2 // Last monitor was 2 (invalid)
		);

		// When we try to use monitor 2
		var result = ctx.Store.Pick(Pickers.PickValidMonitorByWorkspace(workspace.Id, (HMONITOR)2));

		// Then we get monitor 1 as it's the first valid monitor
		Assert.True(result.IsSuccessful);
		Assert.Equal((HMONITOR)1, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoValidMonitors(IContext ctx, MutableRootSector root)
	{
		// Given we have a workspace with no valid monitors and no fallback monitors
		var workspace = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, root, workspace);

		// We don't add any monitors to the system
		// This ensures there are no fallback monitors available

		root.MapSector.StickyWorkspaceMonitorIndexMap = root.MapSector.StickyWorkspaceMonitorIndexMap.SetItem(
			workspace.Id,
			[1] // Only monitor index 1 is valid, but no monitors exist
		);

		// When we try to get a valid monitor
		var result = ctx.Store.Pick(Pickers.PickValidMonitorByWorkspace(workspace.Id));

		// Then we get an error since there are no valid monitors and no fallback monitors
		Assert.False(result.IsSuccessful);
		Assert.Contains("No valid monitor found", result.Error!.Message);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void UseActiveMonitorWhenNoTargetSpecified(IContext ctx, MutableRootSector root)
	{
		// Given we have a workspace and an active monitor
		var workspace = CreateWorkspace(ctx);
		AddWorkspacesToManager(ctx, root, workspace);
		AddMonitorsToManager(ctx, root, CreateMonitor((HMONITOR)1), CreateMonitor((HMONITOR)2));

		root.MonitorSector.ActiveMonitorHandle = (HMONITOR)2;

		// When we don't specify a target monitor
		var result = ctx.Store.Pick(Pickers.PickValidMonitorByWorkspace(workspace.Id));

		// Then we get the active monitor
		Assert.True(result.IsSuccessful);
		Assert.Equal((HMONITOR)2, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceDoesNotExist(IContext ctx)
	{
		// Given we have a non-existent workspace ID
		Guid nonExistentWorkspaceId = Guid.NewGuid();

		// When we try to get a valid monitor
		var result = ctx.Store.Pick(Pickers.PickValidMonitorByWorkspace(nonExistentWorkspaceId));

		// Then we get an error
		Assert.False(result.IsSuccessful);
		Assert.Contains("not found", result.Error!.Message);
	}
}
