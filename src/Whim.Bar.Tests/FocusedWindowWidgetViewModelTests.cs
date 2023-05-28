using Moq;
using Xunit;

namespace Whim.Bar.Tests;

public class FocusedWindowWidgetViewModelTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWindowManager> WindowManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public FocusedWindowWidgetViewModel ViewModel { get; }

		public Wrapper()
		{
			Context.SetupGet(c => c.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(c => c.WindowManager).Returns(WindowManager.Object);
			WorkspaceManager
				.Setup(wm => wm.GetEnumerator())
				.Returns(new List<IWorkspace> { Workspace.Object }.GetEnumerator());
			ViewModel = new FocusedWindowWidgetViewModel(Context.Object);
		}
	}

	[Fact]
	public void Value_PropertyChanged()
	{
		// Given
		Wrapper wrapper = new();
		FocusedWindowWidgetViewModel viewModel = wrapper.ViewModel;

		// When
		// Then
		Assert.PropertyChanged(
			viewModel,
			nameof(viewModel.Title),
			() =>
			{
				wrapper.WindowManager.Raise(
					wm => wm.WindowFocused += null,
					new WindowEventArgs() { Window = new Mock<IWindow>().Object }
				);
			}
		);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();
		FocusedWindowWidgetViewModel viewModel = wrapper.ViewModel;

		// When
		viewModel.Dispose();

		// Then
		wrapper.WindowManager.VerifyRemove(wm => wm.WindowFocused -= It.IsAny<EventHandler<WindowEventArgs>>());
	}
}
