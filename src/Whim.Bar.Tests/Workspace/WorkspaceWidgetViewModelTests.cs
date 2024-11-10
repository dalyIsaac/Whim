using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspaceWidgetViewModelTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceWidgetViewModel_Ctor(IContext ctx, MutableRootSector root)
	{
		// Given
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)100);
		Workspace workspace1 = StoreTestUtils.CreateWorkspace(ctx);
		Workspace workspace2 = StoreTestUtils.CreateWorkspace(ctx);

		StoreTestUtils.AddWorkspacesToManager(ctx, root, workspace1, workspace2);
		StoreTestUtils.PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace2);

		// When
		WorkspaceWidgetViewModel sut = new(ctx, monitor);

		// Then
		Assert.Equal(2, sut.Workspaces.Count);
		Assert.Same(workspace1, sut.Workspaces[0].Workspace);
		Assert.Same(workspace2, sut.Workspaces[1].Workspace);
		Assert.False(sut.Workspaces[0].ActiveOnMonitor);
		Assert.True(sut.Workspaces[1].ActiveOnMonitor);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceAdded(IContext ctx, MutableRootSector root)
	{
		// Given
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)100);
		Workspace workspace1 = StoreTestUtils.CreateWorkspace(ctx);
		Workspace workspace2 = StoreTestUtils.CreateWorkspace(ctx);

		StoreTestUtils.AddWorkspacesToManager(ctx, root, workspace1, workspace2);
		StoreTestUtils.PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace2);

		WorkspaceWidgetViewModel sut = new(ctx, monitor);

		Workspace workspace3 = StoreTestUtils.CreateWorkspace(ctx);
		StoreTestUtils.AddWorkspacesToManager(ctx, root, workspace3);

		// When
		root.WorkspaceSector.QueueEvent(new WorkspaceAddedEventArgs() { Workspace = workspace3 });
		root.DispatchEvents();

		// Then
		Assert.Equal(3, sut.Workspaces.Count);
		Assert.Same(workspace1, sut.Workspaces[0].Workspace);
		Assert.Same(workspace2, sut.Workspaces[1].Workspace);
		Assert.Same(workspace3, sut.Workspaces[2].Workspace);
		Assert.False(sut.Workspaces[0].ActiveOnMonitor);
		Assert.True(sut.Workspaces[1].ActiveOnMonitor);
		Assert.False(sut.Workspaces[2].ActiveOnMonitor);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceRemoved(IContext ctx, MutableRootSector root)
	{
		// Given
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)100);
		Workspace workspace1 = StoreTestUtils.CreateWorkspace(ctx);
		Workspace workspace2 = StoreTestUtils.CreateWorkspace(ctx);
		Workspace workspace3 = StoreTestUtils.CreateWorkspace(ctx);

		StoreTestUtils.AddWorkspacesToManager(ctx, root, workspace1, workspace2, workspace3);
		StoreTestUtils.PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace2);

		WorkspaceWidgetViewModel sut = new(ctx, monitor);

		// When
		root.WorkspaceSector.Workspaces = root.WorkspaceSector.Workspaces.Remove(workspace1.Id);
		root.WorkspaceSector.QueueEvent(new WorkspaceRemovedEventArgs() { Workspace = workspace1 });
		root.DispatchEvents();

		// Then
		Assert.Equal(2, sut.Workspaces.Count);
		Assert.Same(workspace2, sut.Workspaces[0].Workspace);
		Assert.Same(workspace3, sut.Workspaces[1].Workspace);
		Assert.True(sut.Workspaces[0].ActiveOnMonitor);
		Assert.False(sut.Workspaces[1].ActiveOnMonitor);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorWorkspaceChanged_WrongMonitor(IContext ctx, MutableRootSector root)
	{
		// Given
		IMonitor monitor1 = StoreTestUtils.CreateMonitor((HMONITOR)100);
		IMonitor monitor2 = StoreTestUtils.CreateMonitor((HMONITOR)200);
		Workspace workspace1 = StoreTestUtils.CreateWorkspace(ctx);
		Workspace workspace2 = StoreTestUtils.CreateWorkspace(ctx);

		StoreTestUtils.AddWorkspacesToManager(ctx, root, workspace1, workspace2);
		StoreTestUtils.PopulateMonitorWorkspaceMap(ctx, root, monitor1, workspace1);
		StoreTestUtils.PopulateMonitorWorkspaceMap(ctx, root, monitor2, workspace2);

		WorkspaceWidgetViewModel sut = new(ctx, monitor1);

		// When
		root.MapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs() { Monitor = monitor2, CurrentWorkspace = workspace2 }
		);
		root.DispatchEvents();

		// Then
		Assert.Equal(2, sut.Workspaces.Count);
		Assert.Same(workspace1, sut.Workspaces[0].Workspace);
		Assert.Same(workspace2, sut.Workspaces[1].Workspace);
		Assert.True(sut.Workspaces[0].ActiveOnMonitor);
		Assert.False(sut.Workspaces[1].ActiveOnMonitor);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorWorkspaceChanged_CorrectMonitor(IContext ctx, MutableRootSector root)
	{
		// Given
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)100);
		Workspace workspace1 = StoreTestUtils.CreateWorkspace(ctx);
		Workspace workspace2 = StoreTestUtils.CreateWorkspace(ctx);

		StoreTestUtils.AddWorkspacesToManager(ctx, root, workspace1, workspace2);
		StoreTestUtils.PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace1);

		WorkspaceWidgetViewModel sut = new(ctx, monitor);

		// When
		root.MapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs() { Monitor = monitor, CurrentWorkspace = workspace2 }
		);
		root.DispatchEvents();

		// Then
		Assert.Equal(2, sut.Workspaces.Count);
		Assert.Same(workspace1, sut.Workspaces[0].Workspace);
		Assert.Same(workspace2, sut.Workspaces[1].Workspace);
		Assert.False(sut.Workspaces[0].ActiveOnMonitor);
		Assert.True(sut.Workspaces[1].ActiveOnMonitor);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceRenamed_WrongMonitor(IContext ctx, MutableRootSector root)
	{
		// Given
		IMonitor monitor1 = StoreTestUtils.CreateMonitor((HMONITOR)100);
		IMonitor monitor2 = StoreTestUtils.CreateMonitor((HMONITOR)200);
		Workspace workspace1 = StoreTestUtils.CreateWorkspace(ctx);
		Workspace workspace2 = StoreTestUtils.CreateWorkspace(ctx);

		StoreTestUtils.AddWorkspacesToManager(ctx, root, workspace1, workspace2);
		StoreTestUtils.PopulateMonitorWorkspaceMap(ctx, root, monitor1, workspace1);
		StoreTestUtils.PopulateMonitorWorkspaceMap(ctx, root, monitor2, workspace2);

		WorkspaceWidgetViewModel sut = new(ctx, monitor1);
		WorkspaceModel workspaceModel = sut.Workspaces[0];

		// When
		CustomAssert.DoesNotPropertyChange(
			h => workspaceModel.PropertyChanged += h,
			h => workspaceModel.PropertyChanged -= h,
			() =>
			{
				root.WorkspaceSector.QueueEvent(
					new WorkspaceRenamedEventArgs() { Workspace = workspace2, PreviousName = "Old Name" }
				);
				root.DispatchEvents();
			}
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceRenamed_CorrectMonitor(IContext ctx, MutableRootSector root)
	{
		// Given
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)100);
		Workspace workspace1 = StoreTestUtils.CreateWorkspace(ctx);
		Workspace workspace2 = StoreTestUtils.CreateWorkspace(ctx);

		StoreTestUtils.AddWorkspacesToManager(ctx, root, workspace1, workspace2);
		StoreTestUtils.PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace1);

		WorkspaceWidgetViewModel sut = new(ctx, monitor);
		WorkspaceModel workspaceModel = sut.Workspaces[0];

		// When
		Assert.PropertyChanged(
			sut.Workspaces[0],
			nameof(workspaceModel.Name),
			() =>
			{
				root.WorkspaceSector.QueueEvent(
					new WorkspaceRenamedEventArgs() { Workspace = workspace1, PreviousName = "Old Name" }
				);
				root.DispatchEvents();
			}
		);
	}

	[Theory, AutoSubstituteData]
	public void Dispose(IContext ctx, IMonitor monitor)
	{
		// Given
		WorkspaceWidgetViewModel sut = new(ctx, monitor);

		// When
		sut.Dispose();

		// Then
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceAdded += Arg.Any<EventHandler<WorkspaceAddedEventArgs>>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceRemoved += Arg.Any<EventHandler<WorkspaceRemovedEventArgs>>();
		ctx.Store.MapEvents.Received(1).MonitorWorkspaceChanged += Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceRenamed += Arg.Any<EventHandler<WorkspaceRenamedEventArgs>>();

		ctx.Store.WorkspaceEvents.Received(1).WorkspaceAdded -= Arg.Any<EventHandler<WorkspaceAddedEventArgs>>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceRemoved -= Arg.Any<EventHandler<WorkspaceRemovedEventArgs>>();
		ctx.Store.MapEvents.Received(1).MonitorWorkspaceChanged -= Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceRenamed -= Arg.Any<EventHandler<WorkspaceRenamedEventArgs>>();
	}
}
