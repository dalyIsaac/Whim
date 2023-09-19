using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ActiveLayoutWidgetViewModelTests
{
	private static ActiveLayoutWidgetViewModel CreateSut(IContext context, IMonitor monitor) => new(context, monitor);

	[Theory, AutoSubstituteData]
	public void WorkspaceManager_ActiveLayoutEngineChanged(IContext context, IMonitor monitor, IWorkspace workspace)
	{
		// Given
		ActiveLayoutWidgetViewModel viewModel = CreateSut(context, monitor);

		// When
		// Then
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.ActiveLayoutEngine),
			() =>
				context.WorkspaceManager.ActiveLayoutEngineChanged += Raise.Event<
					EventHandler<ActiveLayoutEngineChangedEventArgs>
				>(
					context.WorkspaceManager,
					new ActiveLayoutEngineChangedEventArgs()
					{
						Workspace = workspace,
						PreviousLayoutEngine = workspace.ActiveLayoutEngine,
						CurrentLayoutEngine = workspace.ActiveLayoutEngine
					}
				)
		);
	}

	[Theory, AutoSubstituteData]
	public void WorkspaceManager_MonitorWorkspaceChanged(IContext context, IMonitor monitor, IWorkspace workspace)
	{
		// Given
		ActiveLayoutWidgetViewModel viewModel = CreateSut(context, monitor);

		// When
		// Then
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.ActiveLayoutEngine),
			() =>
				context.WorkspaceManager.MonitorWorkspaceChanged += Raise.Event<
					EventHandler<MonitorWorkspaceChangedEventArgs>
				>(
					context.WorkspaceManager,
					new MonitorWorkspaceChangedEventArgs() { Monitor = monitor, CurrentWorkspace = workspace }
				)
		);
	}

	[Theory, AutoSubstituteData]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Usage",
		"NS5000:Received check.",
		Justification = "The analyzer is wrong"
	)]
	public void Dispose(IContext context, IMonitor monitor)
	{
		// Given
		ActiveLayoutWidgetViewModel viewModel = CreateSut(context, monitor);

		// When
		viewModel.Dispose();

		// Then
		context.WorkspaceManager.Received(1).ActiveLayoutEngineChanged -= Arg.Any<
			EventHandler<ActiveLayoutEngineChangedEventArgs>
		>();
		context.WorkspaceManager.Received(1).MonitorWorkspaceChanged -= Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
	}
}
