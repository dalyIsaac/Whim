using Moq;
using Xunit;

namespace Whim.Bar.Tests;

public class WorkspaceModelTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public WorkspaceWidgetViewModel ViewModel { get; }

		public Wrapper()
		{
			Context.SetupGet(c => c.WorkspaceManager).Returns(WorkspaceManager.Object);
			WorkspaceManager
				.Setup(wm => wm.GetEnumerator())
				.Returns(new List<IWorkspace> { Workspace.Object }.GetEnumerator());
			ViewModel = new WorkspaceWidgetViewModel(Context.Object, new Mock<IMonitor>().Object);
		}
	}

	[InlineData(true)]
	[InlineData(false)]
	[Theory]
	public void ActiveOnMonitor(bool activeOnMonitor)
	{
		// Given
		Wrapper wrapper = new();
		WorkspaceModel model =
			new(wrapper.Context.Object, wrapper.ViewModel, wrapper.Workspace.Object, activeOnMonitor);

		// When
		Assert.PropertyChanged(model, nameof(model.ActiveOnMonitor), () => model.ActiveOnMonitor = !activeOnMonitor);

		// Then
		Assert.Equal(!activeOnMonitor, model.ActiveOnMonitor);
	}

	[Fact]
	public void Workspace_Renamed()
	{
		// Given
		Wrapper wrapper = new();
		WorkspaceModel model = new(wrapper.Context.Object, wrapper.ViewModel, wrapper.Workspace.Object, true);
		string newName = "new name";

		// When
		// Then
		Assert.PropertyChanged(
			model,
			nameof(model.Name),
			() =>
				model.Workspace_Renamed(
					wrapper.Workspace.Object,
					new WorkspaceRenamedEventArgs()
					{
						PreviousName = wrapper.Workspace.Object.Name,
						CurrentName = newName,
						Workspace = wrapper.Workspace.Object
					}
				)
		);
	}
}
