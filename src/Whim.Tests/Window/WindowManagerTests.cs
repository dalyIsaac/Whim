using Moq;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class WindowManagerTests
{
	[Fact]
	public void OnWindowAdded()
	{
		// Given
		Mock<IConfigContext> configContext = new();

		Mock<WorkspaceManager> workspaceManager = new(configContext.Object);
		workspaceManager.Setup(m => m.WindowAdded(It.IsAny<IWindow>()));

		configContext.SetupGet(m => m.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ICoreNativeManager> coreNativeManager = new();

		WindowManager windowManager = new(configContext.Object, coreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowAdded += eh,
			eh => windowManager.WindowAdded -= eh,
			() => windowManager.OnWindowAdded(window.Object)
		);

		// Then
		workspaceManager.Verify(m => m.WindowAdded(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowFocused()
	{
		// Given
		Mock<IConfigContext> configContext = new();
		Mock<ICoreNativeManager> coreNativeManager = new();
		Mock<IWindowMessageMonitor> windowMessageMonitor = new();

		Mock<WorkspaceManager> workspaceManager = new(configContext.Object);
		workspaceManager.Setup(m => m.WindowFocused(It.IsAny<IWindow>()));

		Mock<MonitorManager> monitorManager =
			new(configContext.Object, coreNativeManager.Object, windowMessageMonitor.Object);
		monitorManager.Setup(m => m.WindowFocused(It.IsAny<IWindow>()));

		configContext.SetupGet(m => m.WorkspaceManager).Returns(workspaceManager.Object);
		configContext.SetupGet(m => m.MonitorManager).Returns(monitorManager.Object);

		WindowManager windowManager = new(configContext.Object, coreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowFocused += eh,
			eh => windowManager.WindowFocused -= eh,
			() => windowManager.OnWindowFocused(window.Object)
		);

		// Then
		workspaceManager.Verify(m => m.WindowFocused(window.Object), Times.Once);
		monitorManager.Verify(m => m.WindowFocused(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowRemoved()
	{
		// Given
		Mock<IConfigContext> configContext = new();

		Mock<WorkspaceManager> workspaceManager = new(configContext.Object);
		workspaceManager.Setup(m => m.WindowRemoved(It.IsAny<IWindow>()));

		configContext.SetupGet(m => m.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ICoreNativeManager> coreNativeManager = new();

		WindowManager windowManager = new(configContext.Object, coreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowRemoved += eh,
			eh => windowManager.WindowRemoved -= eh,
			() => windowManager.OnWindowRemoved(window.Object)
		);

		// Then
		workspaceManager.Verify(m => m.WindowRemoved(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}
}
