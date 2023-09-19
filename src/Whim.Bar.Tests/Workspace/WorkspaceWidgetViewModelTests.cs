using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using NSubstitute;
using Xunit;

namespace Whim.Bar.Tests;

public class WorkspaceWidgetViewModelDataAttribute : AutoDataAttribute
{
	public WorkspaceWidgetViewModelDataAttribute()
		: base(CreateFixture) { }

	private static IFixture CreateFixture()
	{
		IFixture fixture = new Fixture();
		fixture.Customize(new AutoNSubstituteCustomization());
		fixture.Customize(new WorkspaceWidgetViewModelCustomization());
		return fixture;
	}
}

public class WorkspaceWidgetViewModelCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext context = fixture.Freeze<IContext>();

		// The workspace manager should have a single workspace
		using IWorkspace workspace = fixture.Create<IWorkspace>();
		context.WorkspaceManager.GetEnumerator().Returns((_) => new List<IWorkspace>() { workspace }.GetEnumerator());

		fixture.Inject(context);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspaceWidgetViewModelTests
{
	[Theory, WorkspaceWidgetViewModelData]
	public void WorkspaceManager_WorkspaceAdded_AlreadyExists(IContext context, IMonitor monitor)
	{
		// Given
		IWorkspace workspace = context.WorkspaceManager.First();
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// When
		context.WorkspaceManager.WorkspaceAdded += Raise.Event<EventHandler<WorkspaceEventArgs>>(
			context.WorkspaceManager,
			new WorkspaceEventArgs() { Workspace = workspace }
		);

		// Then
		Assert.Single(viewModel.Workspaces);
		Assert.Equal(workspace, viewModel.Workspaces[0].Workspace);
		context.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
	}

	[Theory, WorkspaceWidgetViewModelData]
	public void WorkspaceManager_WorkspaceAdded(IContext context, IMonitor monitor, IWorkspace addedWorkspace)
	{
		// Given
		IWorkspace workspace = context.WorkspaceManager.First();
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// When
		context.WorkspaceManager.WorkspaceAdded += Raise.Event<EventHandler<WorkspaceEventArgs>>(
			context.WorkspaceManager,
			new WorkspaceEventArgs() { Workspace = addedWorkspace }
		);

		// Then
		Assert.Equal(2, viewModel.Workspaces.Count);
		context.WorkspaceManager.Received(1).GetMonitorForWorkspace(workspace);
		context.WorkspaceManager.Received(1).GetMonitorForWorkspace(addedWorkspace);
	}

	[Theory, WorkspaceWidgetViewModelData]
	public void WorkspaceManager_WorkspaceRemoved(IContext context, IMonitor monitor)
	{
		// Given
		IWorkspace workspace = context.WorkspaceManager.First();
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// When
		context.WorkspaceManager.WorkspaceRemoved += Raise.Event<EventHandler<WorkspaceEventArgs>>(
			context.WorkspaceManager,
			new WorkspaceEventArgs() { Workspace = workspace }
		);

		// Then
		Assert.Empty(viewModel.Workspaces);
	}

	[Theory, WorkspaceWidgetViewModelData]
	public void WorkspaceManager_WorkspaceRemoved_DoesNotExist(
		IContext context,
		IMonitor monitor,
		IWorkspace removedWorkspace
	)
	{
		// Given
		IWorkspace workspace = context.WorkspaceManager.First();
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// When
		context.WorkspaceManager.WorkspaceRemoved += Raise.Event<EventHandler<WorkspaceEventArgs>>(
			context.WorkspaceManager,
			new WorkspaceEventArgs() { Workspace = removedWorkspace }
		);

		// Then
		Assert.Single(viewModel.Workspaces);
		Assert.Equal(workspace, viewModel.Workspaces[0].Workspace);
	}

	#region WorkspaceManager_MonitorWorkspaceChanged
	[Theory, WorkspaceWidgetViewModelData]
	public void WorkspaceManager_MonitorWorkspaceChanged_Deactivate(
		IContext context,
		IMonitor monitor,
		IWorkspace currentWorkspace
	)
	{
		// Given
		IWorkspace previousWorkspace = context.WorkspaceManager.First();
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// When
		context.WorkspaceManager.MonitorWorkspaceChanged += Raise.Event<EventHandler<MonitorWorkspaceChangedEventArgs>>(
			context.WorkspaceManager,
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = monitor,
				PreviousWorkspace = previousWorkspace,
				CurrentWorkspace = currentWorkspace
			}
		);

		// Then
		WorkspaceModel model = viewModel.Workspaces[0];
		Assert.False(model.ActiveOnMonitor);
	}

	[Theory, WorkspaceWidgetViewModelData]
	public void WorkspaceManager_MonitorWorkspaceChanged_Activate(
		IContext context,
		IMonitor monitor,
		IWorkspace addedWorkspace
	)
	{
		// Given
		IWorkspace workspace = context.WorkspaceManager.First();
		context.WorkspaceManager.GetMonitorForWorkspace(workspace).Returns(monitor);
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// Add workspace
		context.WorkspaceManager.WorkspaceAdded += Raise.Event<EventHandler<WorkspaceEventArgs>>(
			context.WorkspaceManager,
			new WorkspaceEventArgs() { Workspace = addedWorkspace }
		);

		// Verify that the correct workspace is active on the monitor
		WorkspaceModel existingModel = viewModel.Workspaces[0];
		WorkspaceModel addedWorkspaceModel = viewModel.Workspaces[1];
		Assert.True(existingModel.ActiveOnMonitor);
		Assert.False(addedWorkspaceModel.ActiveOnMonitor);

		// When
		context.WorkspaceManager.MonitorWorkspaceChanged += Raise.Event<EventHandler<MonitorWorkspaceChangedEventArgs>>(
			context.WorkspaceManager,
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = monitor,
				PreviousWorkspace = existingModel.Workspace,
				CurrentWorkspace = addedWorkspaceModel.Workspace
			}
		);

		// Then
		Assert.False(existingModel.ActiveOnMonitor);
		Assert.True(addedWorkspaceModel.ActiveOnMonitor);
	}

	[Theory, WorkspaceWidgetViewModelData]
	public void WorkspaceManager_MonitorWorkspaceChanged_DifferentMonitor(
		IContext context,
		IMonitor monitor,
		IWorkspace addedWorkspace,
		IMonitor otherMonitor
	)
	{
		// Given
		IWorkspace workspace = context.WorkspaceManager.First();
		context.WorkspaceManager.GetMonitorForWorkspace(workspace).Returns(monitor);
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// Add workspace
		context.WorkspaceManager.WorkspaceAdded += Raise.Event<EventHandler<WorkspaceEventArgs>>(
			context.WorkspaceManager,
			new WorkspaceEventArgs() { Workspace = addedWorkspace }
		);

		// Verify that the correct workspace is active on the monitor
		WorkspaceModel existingModel = viewModel.Workspaces[0];
		WorkspaceModel addedWorkspaceModel = viewModel.Workspaces[1];
		Assert.True(existingModel.ActiveOnMonitor);
		Assert.False(addedWorkspaceModel.ActiveOnMonitor);

		// When
		context.WorkspaceManager.MonitorWorkspaceChanged += Raise.Event<EventHandler<MonitorWorkspaceChangedEventArgs>>(
			context.WorkspaceManager,
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = otherMonitor,
				PreviousWorkspace = existingModel.Workspace,
				CurrentWorkspace = addedWorkspaceModel.Workspace
			}
		);

		// Then
		Assert.True(existingModel.ActiveOnMonitor);
		Assert.False(addedWorkspaceModel.ActiveOnMonitor);
	}
	#endregion

	[Theory, WorkspaceWidgetViewModelData]
	public void WorkspaceManager_WorkspaceRenamed_ExistingWorkspace(IContext context, IMonitor monitor)
	{
		// Given
		IWorkspace workspace = context.WorkspaceManager.First();
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// When
		// Then
		Assert.PropertyChanged(
			viewModel.Workspaces[0],
			nameof(WorkspaceModel.Name),
			() =>
			{
				context.WorkspaceManager.WorkspaceRenamed += Raise.Event<EventHandler<WorkspaceRenamedEventArgs>>(
					context.WorkspaceManager,
					new WorkspaceRenamedEventArgs()
					{
						Workspace = workspace,
						PreviousName = "Old Name",
						CurrentName = "New Name"
					}
				);
			}
		);
	}

	[Theory, WorkspaceWidgetViewModelData]
	public void WorkspaceManager_WorkspaceRenamed_NonExistingWorkspace(
		IContext context,
		IMonitor monitor,
		IWorkspace renamedWorkspace
	)
	{
		// Given
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// Verify that property changed is not raised

		bool propertyChangedRaised = false;
		WorkspaceModel model = viewModel.Workspaces[0];

		// When
		model.PropertyChanged += (sender, args) => propertyChangedRaised = true;

		context.WorkspaceManager.WorkspaceRenamed += Raise.Event<EventHandler<WorkspaceRenamedEventArgs>>(
			context.WorkspaceManager,
			new WorkspaceRenamedEventArgs()
			{
				Workspace = renamedWorkspace,
				PreviousName = "Old Name",
				CurrentName = "New Name"
			}
		);

		// Then
		Assert.False(propertyChangedRaised);
	}

	[Theory, WorkspaceWidgetViewModelData]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Usage",
		"NS5000:Received check.",
		Justification = "The analyzer is wrong"
	)]
	public void Dispose(IContext context, IMonitor monitor)
	{
		// Given
		WorkspaceWidgetViewModel viewModel = new(context, monitor);

		// When
		viewModel.Dispose();

		// Then
		context.WorkspaceManager.Received(1).WorkspaceAdded -= Arg.Any<EventHandler<WorkspaceEventArgs>>();
		context.WorkspaceManager.Received(1).WorkspaceRemoved -= Arg.Any<EventHandler<WorkspaceEventArgs>>();
		context.WorkspaceManager.Received(1).MonitorWorkspaceChanged -= Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
		context.WorkspaceManager.Received(1).WorkspaceRenamed -= Arg.Any<EventHandler<WorkspaceRenamedEventArgs>>();
	}
}
