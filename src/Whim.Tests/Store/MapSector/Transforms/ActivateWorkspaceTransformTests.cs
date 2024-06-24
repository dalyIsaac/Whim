using System;
using System.Collections.Generic;
using DotNext;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ActivateWorkspaceTransformTests
{
	private static Result<Unit> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector rootSector,
		ActivateWorkspaceTransform sut
	)
	{
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => rootSector.MapSector.MonitorWorkspaceChanged += h,
			h => rootSector.MapSector.MonitorWorkspaceChanged -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return result!.Value;
	}

	private static (Result<Unit>, List<MonitorWorkspaceChangedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector rootSector,
		ActivateWorkspaceTransform sut
	)
	{
		Result<Unit>? result = null;
		List<MonitorWorkspaceChangedEventArgs> evs = new();

		Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h =>
				rootSector.MapSector.MonitorWorkspaceChanged += (sender, args) =>
				{
					evs.Add(args);
					h.Invoke(sender, args);
				},
			h => rootSector.MapSector.MonitorWorkspaceChanged -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, evs);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given the workspace doesn't exist
		ActivateWorkspaceTransform sut = new(Guid.NewGuid());

		// When we activate the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given the monitor doesn't exist
		IWorkspace workspace = CreateWorkspace();
		AddWorkspacesToManager(ctx, workspace);

		ActivateWorkspaceTransform sut = new(workspace.Id);

		// When we activate the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceAlreadyActivatedOnMonitor(IContext ctx, MutableRootSector rootSector)
	{
		// Given the workspace is already activated on the monitor
		IWorkspace workspace = CreateWorkspace();
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, workspace);

		ActivateWorkspaceTransform sut = new(workspace.Id, monitor.Handle);

		// When we activate the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then nothing happens
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LayoutOldWorkspace(IContext ctx, MutableRootSector rootSector)
	{
		// Given the target monitor has an old workspace
		IWorkspace workspace1 = CreateWorkspace();
		IWorkspace workspace2 = CreateWorkspace();
		IWorkspace workspace3 = CreateWorkspace();

		IMonitor monitor1 = CreateMonitor((HMONITOR)1);
		IMonitor monitor2 = CreateMonitor((HMONITOR)2);
		IMonitor monitor3 = CreateMonitor((HMONITOR)3);

		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor1, workspace1);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor2, workspace2);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor3, workspace3);

		ActivateWorkspaceTransform sut = new(workspace3.Id, monitor1.Handle);

		// When we activate the workspace on the target monitor
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then the old workspace is deactivated
		Assert.True(result.IsSuccessful);

		Assert.Equal(2, evs.Count);

		Assert.Same(workspace3, evs[0].PreviousWorkspace);
		Assert.Same(workspace1, evs[0].CurrentWorkspace);
		Assert.Same(monitor3, evs[0].Monitor);

		Assert.Same(workspace1, evs[1].PreviousWorkspace);
		Assert.Same(workspace3, evs[1].CurrentWorkspace);
		Assert.Same(monitor1, evs[1].Monitor);

		Assert.Equal(workspace3.Id, rootSector.MapSector.MonitorWorkspaceMap[monitor1.Handle]);
		Assert.Equal(workspace1.Id, rootSector.MapSector.MonitorWorkspaceMap[monitor3.Handle]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DeactivateOldWorkspace(IContext ctx, MutableRootSector rootSector)
	{
		// Given the target monitor has an old workspace, and the new workspace wasn't previously activated
		IWorkspace workspace1 = CreateWorkspace();
		IWorkspace workspace2 = CreateWorkspace();
		IWorkspace workspace3 = CreateWorkspace();

		IMonitor monitor1 = CreateMonitor((HMONITOR)1);
		IMonitor monitor2 = CreateMonitor((HMONITOR)2);

		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor1, workspace1);
		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor2, workspace2);
		AddWorkspacesToManager(ctx, workspace3);

		ActivateWorkspaceTransform sut = new(workspace3.Id, monitor1.Handle);

		// When we activate the workspace on the target monitor
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then the old workspace is deactivated
		Assert.True(result.IsSuccessful);

		Assert.Single(evs);

		Assert.Same(workspace1, evs[0].PreviousWorkspace);
		Assert.Same(workspace3, evs[0].CurrentWorkspace);
		Assert.Same(monitor1, evs[0].Monitor);
	}
}
