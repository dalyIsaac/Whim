using Moq;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class NextLayoutEngineCommandTests
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
			WorkspaceManager.Setup(wm => wm.GetWorkspaceForMonitor(It.IsAny<IMonitor>())).Returns(Workspace.Object);
		}
	}

	[Fact]
	public void Execute()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);
		NextLayoutEngineCommand command = new(wrapper.Context.Object, viewModel);

		// When
		command.Execute(null);

		// Then
		wrapper.WorkspaceManager.Verify(wm => wm.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Once);
		wrapper.Workspace.Verify(w => w.NextLayoutEngine(), Times.Once);
	}

	[Fact]
	public void CanExecute()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);
		NextLayoutEngineCommand command = new(wrapper.Context.Object, viewModel);

		// When
		bool canExecute = command.CanExecute(null);

		// Then
		Assert.True(canExecute);
	}
}
