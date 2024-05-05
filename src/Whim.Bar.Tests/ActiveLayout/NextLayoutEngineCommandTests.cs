using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class NextLayoutEngineCommandTests
{
	private static ActiveLayoutWidgetViewModel CreateSut(IContext context, IMonitor monitor) => new(context, monitor);

	[Theory, AutoSubstituteData]
	public void Execute_CycleLayoutEngine(IContext context, IMonitor monitor, IWorkspace workspace)
	{
		// Given the picker returns a workspace.
		ActiveLayoutWidgetViewModel viewModel = CreateSut(context, monitor);
		NextLayoutEngineCommand command = new(context, viewModel);

		context.Store.Pick(Arg.Any<PurePicker<Result<IWorkspace>>>()).Returns(Result.FromValue(workspace));

		// When
		command.Execute(null);

		// Then
		workspace.Received(1).CycleLayoutEngine(false);
	}

	[Theory, AutoSubstituteData]
	public void Execute_NoWorkspaceForMonitor(IContext context, IMonitor monitor, IWorkspace workspace)
	{
		// Given the picker doesn't return a workspace
		ActiveLayoutWidgetViewModel viewModel = CreateSut(context, monitor);
		NextLayoutEngineCommand command = new(context, viewModel);

		context.Store.Pick(Arg.Any<PurePicker<Result<IWorkspace>>>()).Returns(Result.FromException<IWorkspace>(new Exception("welp")));

		// When
		command.Execute(null);

		// Then
		workspace.DidNotReceive().CycleLayoutEngine(false);
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
