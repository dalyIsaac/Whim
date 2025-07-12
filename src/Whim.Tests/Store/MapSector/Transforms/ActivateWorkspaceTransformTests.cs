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

		CustomAssert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => rootSector.MapSector.MonitorWorkspaceChanged += h,
			h => rootSector.MapSector.MonitorWorkspaceChanged -= h,
			() => result = ctx.Store.Dispatch(sut),
			(sender, args) => evs.Add(args)
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
		Workspace workspace = CreateWorkspace();
		AddWorkspacesToStore(rootSector, workspace);

		ActivateWorkspaceTransform sut = new(workspace.Id);

		// When we activate the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceAlreadyActivatedOnMonitor(
		IContext ctx,
		MutableRootSector rootSector,
		List<object> executedTransforms
	)
	{
		// Given the workspace is already activated on the monitor
		Workspace workspace = CreateWorkspace();
		IMonitor monitor = CreateMonitor((HMONITOR)10);
		PopulateMonitorWorkspaceMap(rootSector, monitor, workspace);

		ActivateWorkspaceTransform sut = new(workspace.Id, monitor.Handle);

		// When we activate the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then nothing happens
		Assert.True(result.IsSuccessful);
		Assert.DoesNotContain(executedTransforms, t => t.Equals(new DoWorkspaceLayoutTransform(workspace.Id)));
		Assert.DoesNotContain(executedTransforms, t => t is FocusWindowTransform);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LayoutOldWorkspace(IContext ctx, MutableRootSector rootSector, List<object> executedTransforms)
	{
		// Given the target monitor has an old workspace
		Workspace workspace1 = CreateWorkspace();
		Workspace workspace2 = CreateWorkspace();
		Workspace workspace3 = CreateWorkspace();

		IMonitor monitor1 = CreateMonitor((HMONITOR)1);
		IMonitor monitor2 = CreateMonitor((HMONITOR)2);
		IMonitor monitor3 = CreateMonitor((HMONITOR)3);

		PopulateMonitorWorkspaceMap(rootSector, monitor1, workspace1);
		PopulateMonitorWorkspaceMap(rootSector, monitor2, workspace2);
		PopulateMonitorWorkspaceMap(rootSector, monitor3, workspace3);

		ActivateWorkspaceTransform sut = new(workspace3.Id, monitor1.Handle);

		// When we activate the workspace on the target monitor
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then the old workspace is deactivated on monitor 1 and the new workspace is activated
		Assert.True(result.IsSuccessful);

		Assert.Equal(2, evs.Count);

		// The event for the first monitor.
		Assert.Same(workspace3, evs[0].PreviousWorkspace);
		Assert.Same(workspace1, evs[0].CurrentWorkspace);
		Assert.Same(monitor3, evs[0].Monitor);

		// The event for the second monitor.
		Assert.Same(workspace1, evs[1].PreviousWorkspace);
		Assert.Same(workspace3, evs[1].CurrentWorkspace);
		Assert.Same(monitor1, evs[1].Monitor);

		Assert.Equal(workspace3.Id, rootSector.MapSector.MonitorWorkspaceMap[monitor1.Handle]);
		Assert.Equal(workspace1.Id, rootSector.MapSector.MonitorWorkspaceMap[monitor3.Handle]);

		Assert.Contains(executedTransforms, t => t.Equals(new DoWorkspaceLayoutTransform(workspace1.Id)));
		Assert.Contains(executedTransforms, t => t.Equals(new DoWorkspaceLayoutTransform(workspace3.Id)));
		Assert.Contains(executedTransforms, t => t.Equals(new FocusWorkspaceTransform(workspace3.Id)));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DeactivateOldWorkspace(IContext ctx, MutableRootSector rootSector, List<object> executedTransforms)
	{
		// Given the target monitor has an old workspace, and the new workspace wasn't previously activated
		Workspace oldWorkspace = CreateWorkspace();
		Workspace workspace2 = CreateWorkspace();
		Workspace newWorkspace = CreateWorkspace();

		IMonitor monitor1 = CreateMonitor((HMONITOR)1);
		IMonitor monitor2 = CreateMonitor((HMONITOR)2);

		PopulateMonitorWorkspaceMap(rootSector, monitor1, oldWorkspace);
		PopulateMonitorWorkspaceMap(rootSector, monitor2, workspace2);
		AddWorkspacesToStore(rootSector, newWorkspace);

		ActivateWorkspaceTransform sut = new(newWorkspace.Id, monitor1.Handle);

		// When we activate the workspace on the target monitor
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then the old workspace is deactivated
		Assert.True(result.IsSuccessful);

		Assert.Single(evs);

		Assert.Same(oldWorkspace, evs[0].PreviousWorkspace);
		Assert.Same(newWorkspace, evs[0].CurrentWorkspace);
		Assert.Same(monitor1, evs[0].Monitor);

		Assert.DoesNotContain(executedTransforms, t => t.Equals(new DoWorkspaceLayoutTransform(oldWorkspace.Id)));
		Assert.Contains(executedTransforms, t => t.Equals(new DeactivateWorkspaceTransform(oldWorkspace.Id)));

		Assert.Contains(executedTransforms, t => t.Equals(new DoWorkspaceLayoutTransform(newWorkspace.Id)));
		Assert.Contains(executedTransforms, t => t.Equals(new FocusWorkspaceTransform(newWorkspace.Id)));
		Assert.DoesNotContain(executedTransforms, t => t.Equals(new DeactivateWorkspaceTransform(newWorkspace.Id)));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FocusWorkspaceWindow_ActiveIsOldWorkspace(
		IContext ctx,
		MutableRootSector rootSector,
		List<object> executedTransforms
	)
	{
		// Given the FocusWorkspaceWindow flag is false and the active workspace is the old workspace...
		Workspace workspace1 = CreateWorkspace();
		Workspace workspace2 = CreateWorkspace();

		IMonitor monitor1 = CreateMonitor((HMONITOR)1);

		PopulateMonitorWorkspaceMap(rootSector, monitor1, workspace1);
		AddWorkspacesToStore(rootSector, workspace2);

		ActivateWorkspaceTransform sut = new(workspace2.Id, monitor1.Handle, FocusWorkspaceWindow: false);

		// When we activate the workspace
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then the window on the new workspace is focused
		Assert.True(result.IsSuccessful);
		Assert.Contains(executedTransforms, t => t.Equals(new FocusWorkspaceTransform(workspace2.Id)));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FocusWorkspaceWindow_ActiveIsStillVisible(
		IContext ctx,
		MutableRootSector rootSector,
		List<object> executedTransforms
	)
	{
		// Given the FocusWorkspaceWindow flag is false and the active workspace is still visible...
		Workspace workspace1 = CreateWorkspace();
		Workspace workspace2 = CreateWorkspace();
		Workspace workspace3 = CreateWorkspace();

		IMonitor monitor1 = CreateMonitor((HMONITOR)1);
		IMonitor monitor2 = CreateMonitor((HMONITOR)2);

		PopulateMonitorWorkspaceMap(rootSector, monitor1, workspace1);
		PopulateMonitorWorkspaceMap(rootSector, monitor2, workspace2);
		AddWorkspacesToStore(rootSector, workspace3);
		AddActiveWorkspaceToStore(rootSector, workspace1);

		ActivateWorkspaceTransform sut = new(workspace3.Id, monitor2.Handle, FocusWorkspaceWindow: false);

		// When we activate the workspace
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then the window on the previously active workspace is focused
		Assert.True(result.IsSuccessful);

		Assert.Contains(executedTransforms, t => t.Equals(new FocusWorkspaceTransform(workspace1.Id)));
		Assert.DoesNotContain(executedTransforms, t => t.Equals(new FocusWorkspaceTransform(workspace3.Id)));
	}
}
