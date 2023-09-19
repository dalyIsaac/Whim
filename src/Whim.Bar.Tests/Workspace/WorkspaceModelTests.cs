using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

public class WorkspaceModelTests
{
	private static WorkspaceModel CreateSut(
		IContext context,
		WorkspaceWidgetViewModel viewModel,
		IWorkspace workspace,
		bool activeOnMonitor
	) => new(context, viewModel, workspace, activeOnMonitor);

	[InlineAutoSubstituteData(true)]
	[InlineAutoSubstituteData(false)]
	[Theory]
	internal void ActiveOnMonitor(
		bool activeOnMonitor,
		IContext context,
		WorkspaceWidgetViewModel viewModel,
		IWorkspace workspace
	)
	{
		// Given
		WorkspaceModel model = CreateSut(context, viewModel, workspace, activeOnMonitor);

		// When
		Assert.PropertyChanged(model, nameof(model.ActiveOnMonitor), () => model.ActiveOnMonitor = !activeOnMonitor);

		// Then
		Assert.Equal(!activeOnMonitor, model.ActiveOnMonitor);
	}

	[Theory, AutoSubstituteData]
	internal void Workspace_Renamed(IContext context, WorkspaceWidgetViewModel viewModel, IWorkspace workspace)
	{
		// Given
		WorkspaceModel model = CreateSut(context, viewModel, workspace, true);
		string newName = "new name";

		// When
		// Then
		Assert.Equal(model.Name, workspace.Name);
		Assert.PropertyChanged(
			model,
			nameof(model.Name),
			() =>
				model.Workspace_Renamed(
					workspace,
					new WorkspaceRenamedEventArgs()
					{
						PreviousName = workspace.Name,
						CurrentName = newName,
						Workspace = workspace
					}
				)
		);
	}
}
