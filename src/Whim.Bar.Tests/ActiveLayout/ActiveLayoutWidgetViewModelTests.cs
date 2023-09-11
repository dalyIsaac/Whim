using NSubstitute;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ActiveLayoutWidgetViewModelTests
{
	private class Wrapper
	{
		public IContext Context { get; } = Substitute.For<IContext>();
		public IWorkspaceManager WorkspaceManager { get; } = Substitute.For<IWorkspaceManager>();
		public IWorkspace Workspace { get; } = Substitute.For<IWorkspace>();
		public IMonitor Monitor { get; } = Substitute.For<IMonitor>();

		public Wrapper()
		{
			Context.WorkspaceManager.Returns(WorkspaceManager);
		}
	}

	[Fact]
	public void WorkspaceManager_ActiveLayoutEngineChanged()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context, wrapper.Monitor);

		// When
		// Then
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.ActiveLayoutEngine),
			() =>
				wrapper.WorkspaceManager.ActiveLayoutEngineChanged += Raise.Event<
					EventHandler<ActiveLayoutEngineChangedEventArgs>
				>(
					wrapper.WorkspaceManager,
					new ActiveLayoutEngineChangedEventArgs()
					{
						Workspace = wrapper.Workspace,
						PreviousLayoutEngine = wrapper.Workspace.ActiveLayoutEngine,
						CurrentLayoutEngine = wrapper.Workspace.ActiveLayoutEngine
					}
				)
		);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context, wrapper.Monitor);

		// When
		// Then
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.ActiveLayoutEngine),
			() =>
				wrapper.WorkspaceManager.MonitorWorkspaceChanged += Raise.Event<
					EventHandler<MonitorWorkspaceChangedEventArgs>
				>(
					wrapper.WorkspaceManager,
					new MonitorWorkspaceChangedEventArgs()
					{
						Monitor = wrapper.Monitor,
						CurrentWorkspace = wrapper.Workspace
					}
				)
		);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged_DifferentMonitorButEquals()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context, wrapper.Monitor);
		IMonitor monitor = Substitute.For<IMonitor>();
		monitor.Equals(wrapper.Monitor).Returns(true);
		wrapper.Monitor.Equals(monitor).Returns(true);

		// When
		// Then
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.ActiveLayoutEngine),
			() =>
				wrapper.WorkspaceManager.MonitorWorkspaceChanged += Raise.Event<
					EventHandler<MonitorWorkspaceChangedEventArgs>
				>(
					wrapper.WorkspaceManager,
					new MonitorWorkspaceChangedEventArgs() { Monitor = monitor, CurrentWorkspace = wrapper.Workspace }
				)
		);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged_DifferentMonitor()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context, wrapper.Monitor);
		IMonitor monitor = Substitute.For<IMonitor>();
		monitor.Equals(wrapper.Monitor).Returns(false);
		wrapper.Monitor.Equals(monitor).Returns(false);

		// When
		// Then
		TestUtils.Assert.PropertyNotChanged(
			viewModel,
			nameof(viewModel.ActiveLayoutEngine),
			() =>
				wrapper.WorkspaceManager.MonitorWorkspaceChanged += Raise.Event<
					EventHandler<MonitorWorkspaceChangedEventArgs>
				>(
					wrapper.WorkspaceManager,
					new MonitorWorkspaceChangedEventArgs() { Monitor = monitor, CurrentWorkspace = wrapper.Workspace }
				)
		);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();
		ActiveLayoutWidgetViewModel viewModel = new(wrapper.Context, wrapper.Monitor);

		// When
		viewModel.Dispose();

		// Then
		wrapper.WorkspaceManager.Received(1).ActiveLayoutEngineChanged -= Arg.Any<
			EventHandler<ActiveLayoutEngineChangedEventArgs>
		>();
		wrapper.WorkspaceManager.Received(1).MonitorWorkspaceChanged -= Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
	}
}
