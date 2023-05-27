using Moq;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ActiveLayoutWidgetViewModelTests
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
		}
	}

	[Fact]
	public void WorkspaceManager_ActiveLayoutEngineChanged()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);

		// When
		// Then
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.ActiveLayoutEngine),
			() =>
				wrapper.WorkspaceManager.Raise(
					wm => wm.ActiveLayoutEngineChanged += null,
					new ActiveLayoutEngineChangedEventArgs()
					{
						Workspace = wrapper.Workspace.Object,
						PreviousLayoutEngine = wrapper.Workspace.Object.ActiveLayoutEngine,
						CurrentLayoutEngine = wrapper.Workspace.Object.ActiveLayoutEngine
					}
				)
		);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);

		// When
		// Then
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.ActiveLayoutEngine),
			() =>
				wrapper.WorkspaceManager.Raise(
					wm => wm.MonitorWorkspaceChanged += null,
					new MonitorWorkspaceChangedEventArgs()
					{
						Monitor = wrapper.Monitor.Object,
						NewWorkspace = wrapper.Workspace.Object
					}
				)
		);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context.Object, wrapper.Monitor.Object);

		// When
		viewModel.Dispose();

		// Then
		wrapper.WorkspaceManager.VerifyRemove(
			wm => wm.ActiveLayoutEngineChanged -= It.IsAny<EventHandler<ActiveLayoutEngineChangedEventArgs>>(),
			Times.Once
		);
		wrapper.WorkspaceManager.VerifyRemove(
			wm => wm.MonitorWorkspaceChanged -= It.IsAny<EventHandler<MonitorWorkspaceChangedEventArgs>>(),
			Times.Once
		);
	}
}
