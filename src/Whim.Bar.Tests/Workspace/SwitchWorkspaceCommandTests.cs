using Moq;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class SwitchWorkspaceCommandTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public WorkspaceWidgetViewModel ViewModel { get; }
		public WorkspaceModel Model { get; }

		public Wrapper()
		{
			Context.SetupGet(c => c.WorkspaceManager).Returns(WorkspaceManager.Object);
			WorkspaceManager
				.Setup(wm => wm.GetEnumerator())
				.Returns(new List<IWorkspace> { Workspace.Object }.GetEnumerator());
			ViewModel = new WorkspaceWidgetViewModel(Context.Object, Monitor.Object);
			Model = new WorkspaceModel(Context.Object, ViewModel, Workspace.Object, true);
		}
	}

	[Fact]
	public void Workspace_PropertyChanged()
	{
		// Given
		Wrapper wrapper = new();
		SwitchWorkspaceCommand command = new(wrapper.Context.Object, wrapper.ViewModel, wrapper.Model);

		// When
		// Then
		Assert.Raises<EventArgs>(
			h => command.CanExecuteChanged += new EventHandler(h),
			h => command.CanExecuteChanged -= new EventHandler(h),
			() => wrapper.Model.ActiveOnMonitor = true
		);
	}

	[Fact]
	public void Execute_InvalidObject()
	{
		// Given
		Wrapper wrapper = new();
		SwitchWorkspaceCommand command = new(wrapper.Context.Object, wrapper.ViewModel, wrapper.Model);

		// When
		command.Execute(null);

		// Then
		wrapper.WorkspaceManager.Verify(
			wm => wm.Activate(wrapper.Workspace.Object, wrapper.ViewModel.Monitor),
			Times.Never
		);
	}

	[Fact]
	public void Execute_ValidObject()
	{
		// Given
		Wrapper wrapper = new();
		SwitchWorkspaceCommand command = new(wrapper.Context.Object, wrapper.ViewModel, wrapper.Model);

		// When
		command.Execute(wrapper.Model);

		// Then
		wrapper.WorkspaceManager.Verify(
			wm => wm.Activate(wrapper.Workspace.Object, wrapper.ViewModel.Monitor),
			Times.Once
		);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();
		SwitchWorkspaceCommand command = new(wrapper.Context.Object, wrapper.ViewModel, wrapper.Model);

		// When
		command.Dispose();

		// Then
		wrapper.WorkspaceManager.Verify(
			wm => wm.Activate(wrapper.Workspace.Object, wrapper.ViewModel.Monitor),
			Times.Never
		);
	}
}
