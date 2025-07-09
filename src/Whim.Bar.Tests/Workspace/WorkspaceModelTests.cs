using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
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

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Workspace_Renamed(IContext ctx, MutableRootSector root, WorkspaceWidgetViewModel viewModel)
	{
		// Given
		Workspace workspace = StoreTestUtils.CreateWorkspace(ctx) with
		{
			BackingName = "This is the workspace name",
		};
		StoreTestUtils.AddWorkspaceToManager(ctx, root, workspace);

		// When
		WorkspaceModel model = CreateSut(ctx, viewModel, workspace, true);

		// Then
		Assert.Equal(model.Name, workspace.BackingName);
		Assert.PropertyChanged(
			model,
			nameof(model.Name),
			() =>
				model.Workspace_Renamed(
					workspace,
					new WorkspaceRenamedEventArgs() { PreviousName = workspace.Name, Workspace = workspace }
				)
		);
	}
}
