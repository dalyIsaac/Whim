using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class SwitchWorkspaceCommandTests
{
	private static SwitchWorkspaceCommand CreateSut(
		IContext context,
		WorkspaceWidgetViewModel viewModel,
		WorkspaceModel workspaceModel
	) => new(context, viewModel, workspaceModel);

	[Theory, AutoSubstituteData]
	internal void Workspace_PropertyChanged(
		IContext context,
		WorkspaceWidgetViewModel viewModel,
		WorkspaceModel workspaceModel
	)
	{
		// Given
		SwitchWorkspaceCommand command = CreateSut(context, viewModel, workspaceModel);

		// When
		// Then
		Assert.Raises<EventArgs>(
			h => command.CanExecuteChanged += new EventHandler(h),
			h => command.CanExecuteChanged -= new EventHandler(h),
			() => workspaceModel.ActiveOnMonitor = true
		);
	}

	[Theory, AutoSubstituteData]
	internal void Execute_InvalidObject(
		IContext context,
		WorkspaceWidgetViewModel viewModel,
		WorkspaceModel workspaceModel
	)
	{
		// Given
		SwitchWorkspaceCommand command = CreateSut(context, viewModel, workspaceModel);

		// When
		command.Execute(null);

		// Then
		context.Butler.DidNotReceive().Activate(workspaceModel.Workspace, viewModel.Monitor);
	}

	[Theory, AutoSubstituteData]
	internal void Execute_ValidObject(
		IContext context,
		WorkspaceWidgetViewModel viewModel,
		WorkspaceModel workspaceModel
	)
	{
		// Given
		SwitchWorkspaceCommand command = CreateSut(context, viewModel, workspaceModel);

		// When
		command.Execute(workspaceModel);

		// Then
		context.Butler.Received(1).Activate(workspaceModel.Workspace, viewModel.Monitor);
	}

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext context, WorkspaceWidgetViewModel viewModel, WorkspaceModel workspaceModel)
	{
		// Given
		SwitchWorkspaceCommand command = CreateSut(context, viewModel, workspaceModel);

		// When
		command.Dispose();

		// Then
		context.Butler.DidNotReceive().Activate(workspaceModel.Workspace, viewModel.Monitor);
	}
}
