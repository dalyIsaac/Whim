using Moq;
using Xunit;

namespace Whim.Tests;

public class WindowManagerTests
{
	[Fact]
	public void OnWindowFocused_MonitorManager_WindowFocused()
	{
		// Given
		Mock<IWorkspaceManager> workspaceManagerMock = new();
		Mock<IMonitor> monitorMock = new();
		workspaceManagerMock.Setup(w => w.GetMonitorForWindow(It.IsAny<IWindow>())).Returns(monitorMock.Object);

		Mock<IConfigContext> configContextMock = new();
		configContextMock.Setup(c => c.WorkspaceManager).Returns(workspaceManagerMock.Object);

#pragma warning disable CA2000 // Dispose objects before losing scope
		MonitorManager monitorManager = new(configContextMock.Object);
#pragma warning restore CA2000 // Dispose objects before losing scope
		configContextMock.Setup(c => c.MonitorManager).Returns(monitorManager);

#pragma warning disable CA2000 // Dispose objects before losing scope
		WindowManager windowManager = new(configContextMock.Object);
#pragma warning restore CA2000 // Dispose objects before losing scope

		// When the window manager calls TriggerWindowFocused
		windowManager.OnWindowFocused(new Mock<IWindow>().Object);

		// Then the monitor manager was called and updated the focused monitor.
		Assert.Equal(monitorMock.Object, monitorManager.FocusedMonitor);
	}
}
