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
		monitorMock.Setup(m => m.Equals(It.Is((IMonitor m) => m == monitorMock.Object))).Returns(true);
		workspaceManagerMock.Setup(w => w.GetMonitorForWindow(It.IsAny<IWindow>())).Returns(monitorMock.Object);

		Mock<IConfigContext> configContextMock = new();
		configContextMock.Setup(c => c.WorkspaceManager).Returns(workspaceManagerMock.Object);

		Mock<ICoreNativeManager> coreNativeManagerMock = new();

		using MonitorManager monitorManager =
			new(configContextMock.Object, coreNativeManagerMock.Object, new Mock<IWindowMessageMonitor>().Object);
		configContextMock.Setup(c => c.MonitorManager).Returns(monitorManager);

		using WindowManager windowManager = new(configContextMock.Object, coreNativeManagerMock.Object);

		// When the window manager calls TriggerWindowFocused
		windowManager.OnWindowFocused(new Mock<IWindow>().Object);

		// Then the monitor manager was called and updated the focused monitor.
		Assert.Equal(monitorMock.Object, monitorManager.FocusedMonitor);
	}
}
