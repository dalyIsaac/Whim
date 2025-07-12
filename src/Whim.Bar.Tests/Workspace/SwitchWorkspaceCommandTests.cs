using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

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
		context.Store.DidNotReceive().Dispatch(Arg.Any<ActivateWorkspaceTransform>());
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
		context
			.Store.Received(1)
			.Dispatch(
				Arg.Is<ActivateWorkspaceTransform>(t =>
					t.WorkspaceId == workspaceModel.Workspace.Id && t.MonitorHandle == viewModel.Monitor.Handle
				)
			);
	}

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext context, WorkspaceWidgetViewModel viewModel, WorkspaceModel workspaceModel)
	{
		// Given
		SwitchWorkspaceCommand command = CreateSut(context, viewModel, workspaceModel);

		// When
		command.Dispose();

		// Then
		CustomAssert.DoesNotRaise(
			h => command.CanExecuteChanged += h,
			h => command.CanExecuteChanged -= h,
			() => workspaceModel.ActiveOnMonitor = true
		);
	}
}
