using Moq;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspaceWidgetViewModelTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();

		public Wrapper()
		{
			Context.SetupGet(c => c.WorkspaceManager).Returns(WorkspaceManager.Object);
			WorkspaceManager
				.Setup(wm => wm.GetEnumerator())
				.Returns(new List<IWorkspace> { Workspace.Object }.GetEnumerator());
			WorkspaceManager.Setup(wm => wm.GetMonitorForWorkspace(Workspace.Object)).Returns(Monitor.Object);
		}
	}

	[Fact]
	public void WorkspaceManager_WorkspaceAdded_AlreadyExists()
	{
		// Given
		Wrapper wrapper = new();
		WorkspaceWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);

		// When
		wrapper.WorkspaceManager.Raise(
			wm => wm.WorkspaceAdded += null,
			new WorkspaceEventArgs() { Workspace = wrapper.Workspace.Object }
		);

		// Then
		Assert.Single(viewModel.Workspaces);
		Assert.Equal(wrapper.Workspace.Object, viewModel.Workspaces[0].Workspace);
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(wrapper.Workspace.Object), Times.Once);
	}

	[Fact]
	public void WorkspaceManager_WorkspaceAdded()
	{
		// Given
		Wrapper wrapper = new();
		WorkspaceWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);
		Mock<IWorkspace> addedWorkspace = new();

		// When
		wrapper.WorkspaceManager.Raise(
			wm => wm.WorkspaceAdded += null,
			new WorkspaceEventArgs() { Workspace = addedWorkspace.Object }
		);

		// Then
		Assert.Equal(2, viewModel.Workspaces.Count);
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(wrapper.Workspace.Object), Times.Once);
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(addedWorkspace.Object), Times.Once);
	}

	[Fact]
	public void WorkspaceManager_WorkspaceRemoved()
	{
		// Given
		Wrapper wrapper = new();
		WorkspaceWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);

		// When
		wrapper.WorkspaceManager.Raise(
			wm => wm.WorkspaceRemoved += null,
			new WorkspaceEventArgs() { Workspace = wrapper.Workspace.Object }
		);

		// Then
		Assert.Empty(viewModel.Workspaces);
	}

	[Fact]
	public void WorkspaceManager_WorkspaceRemoved_DoesNotExist()
	{
		// Given
		Wrapper wrapper = new();
		WorkspaceWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);
		Mock<IWorkspace> removedWorkspace = new();

		// When
		wrapper.WorkspaceManager.Raise(
			wm => wm.WorkspaceRemoved += null,
			new WorkspaceEventArgs() { Workspace = removedWorkspace.Object }
		);

		// Then
		Assert.Single(viewModel.Workspaces);
		Assert.Equal(wrapper.Workspace.Object, viewModel.Workspaces[0].Workspace);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged_Deactivate()
	{
		// Given
		Wrapper wrapper = new();
		WorkspaceWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);

		// When
		wrapper.WorkspaceManager.Raise(
			wm => wm.MonitorWorkspaceChanged += null,
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = wrapper.Monitor.Object,
				OldWorkspace = wrapper.Workspace.Object,
				NewWorkspace = new Mock<IWorkspace>().Object
			}
		);

		// Then
		WorkspaceModel model = viewModel.Workspaces[0];
		Assert.False(model.ActiveOnMonitor);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged_Activate()
	{
		// Given
		Wrapper wrapper = new();
		WorkspaceWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);

		// Add workspace
		Mock<IWorkspace> addedWorkspace = new();
		wrapper.WorkspaceManager.Raise(
			wm => wm.WorkspaceAdded += null,
			new WorkspaceEventArgs() { Workspace = addedWorkspace.Object }
		);

		// Verify that the correct workspace is active on the monitor
		WorkspaceModel existingModel = viewModel.Workspaces[0];
		WorkspaceModel addedWorkspaceModel = viewModel.Workspaces[1];
		Assert.True(existingModel.ActiveOnMonitor);
		Assert.False(addedWorkspaceModel.ActiveOnMonitor);

		// When
		wrapper.WorkspaceManager.Raise(
			wm => wm.MonitorWorkspaceChanged += null,
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = wrapper.Monitor.Object,
				OldWorkspace = existingModel.Workspace,
				NewWorkspace = addedWorkspaceModel.Workspace
			}
		);

		// Then
		Assert.False(existingModel.ActiveOnMonitor);
		Assert.True(addedWorkspaceModel.ActiveOnMonitor);
	}

	[Fact]
	public void WorkspaceManager_WorkspaceRenamed_ExistingWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		WorkspaceWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);

		// When
		// Then
		Assert.PropertyChanged(
			viewModel.Workspaces[0],
			nameof(WorkspaceModel.Name),
			() =>
			{
				wrapper.WorkspaceManager.Raise(
					wm => wm.WorkspaceRenamed += null,
					new WorkspaceRenamedEventArgs()
					{
						Workspace = wrapper.Workspace.Object,
						PreviousName = "Old Name",
						CurrentName = "New Name"
					}
				);
			}
		);
	}
}
