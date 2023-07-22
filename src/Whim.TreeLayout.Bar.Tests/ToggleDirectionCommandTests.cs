using Moq;
using Xunit;

namespace Whim.TreeLayout.Bar.Tests;

public class ToggleDirectionCommandTests
{
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<ITreeLayoutPlugin> Plugin { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IImmutableLayoutEngine> TreeLayoutEngine { get; } = new();
		public TreeLayoutEngineWidgetViewModel ViewModel { get; }

		public MocksBuilder()
		{
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			WorkspaceManager.Setup(x => x.GetWorkspaceForMonitor(Monitor.Object)).Returns(Workspace.Object);
			Workspace.SetupGet(x => x.ActiveLayoutEngine).Returns(TreeLayoutEngine.Object);

			ViewModel = new TreeLayoutEngineWidgetViewModel(Context.Object, Plugin.Object, Monitor.Object);
		}
	}

	[Fact]
	public void CanExecute_ShouldReturnTrue()
	{
		// Given
		MocksBuilder mocks = new();
		ToggleDirectionCommand command = new(mocks.ViewModel);

		// When
		bool actual = command.CanExecute(null);

		// Then
		Assert.True(actual);
	}

	[Fact]
	public void Execute_ShouldToggleDirection()
	{
		// Given
		MocksBuilder mocks = new();
		mocks.Plugin.Setup(p => p.GetAddWindowDirection(mocks.Monitor.Object)).Returns(Direction.Up);
		ToggleDirectionCommand command = new(mocks.ViewModel);

		// When
		command.Execute(null);

		// Then
		mocks.Plugin.Verify(p => p.SetAddWindowDirection(mocks.Monitor.Object, Direction.Right));
	}
}
