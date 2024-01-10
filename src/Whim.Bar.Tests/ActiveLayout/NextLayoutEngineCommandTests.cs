using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class NextLayoutEngineCommandTests
{
	private static ActiveLayoutWidgetViewModel CreateSut(IContext context, IMonitor monitor) => new(context, monitor);

	[Theory, AutoSubstituteData]
	public void Execute(IContext context, IMonitor monitor)
	{
		// Given
		ActiveLayoutWidgetViewModel viewModel = CreateSut(context, monitor);
		NextLayoutEngineCommand command = new(context, viewModel);

		// When
		command.Execute(null);

		// Then
		context.WorkspaceManager.Received(1).GetWorkspaceForMonitor(Arg.Any<IMonitor>());
		context.WorkspaceManager.GetWorkspaceForMonitor(monitor)!.Received(1).CycleLayoutEngine(false);
	}

	[Theory, AutoSubstituteData]
	public void CanExecute(IContext context, IMonitor monitor)
	{
		// Given
		ActiveLayoutWidgetViewModel viewModel = CreateSut(context, monitor);
		NextLayoutEngineCommand command = new(context, viewModel);

		// When
		bool canExecute = command.CanExecute(null);

		// Then
		Assert.True(canExecute);
	}
}
