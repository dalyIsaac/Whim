using Moq;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class MonitorManagerTests
{
	private static (Mock<IConfigContext>, Mock<ICoreNativeManager>, Mock<IWindowMessageMonitor>) CreateStubs()
	{
		Mock<IConfigContext> configContextMock = new();

		Mock<ICoreNativeManager> coreNativeManagerMock = new();
		coreNativeManagerMock.Setup(m => m.HasMultipleMonitors()).Returns(true);
		coreNativeManagerMock
			.Setup(
				m =>
					m.EnumDisplayMonitors(
						It.IsAny<SafeHandle>(),
						It.IsAny<RECT?>(),
						It.IsAny<MONITORENUMPROC>(),
						It.IsAny<LPARAM>()
					)
			)
			.Callback<SafeHandle, RECT?, MONITORENUMPROC, LPARAM>(
				(hdc, rect, callback, param) =>
				{
					RECT monitor1Rect =
						new()
						{
							left = 1920,
							top = 0,
							right = 3840,
							bottom = 1080
						};

					RECT monitor2Rect =
						new()
						{
							left = 0,
							top = 0,
							right = 1920,
							bottom = 1080
						};
					unsafe
					{
						callback.Invoke(new HMONITOR(1), (HDC)0, &monitor1Rect, (LPARAM)0);
						callback.Invoke(new HMONITOR(2), (HDC)0, &monitor2Rect, (LPARAM)0);
					}
				}
			);
		coreNativeManagerMock
			.Setup(n => n.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY))
			.Returns(new HMONITOR(1));

		Mock<IWindowMessageMonitor> windowMessageMonitorMock = new();

		return (configContextMock, coreNativeManagerMock, windowMessageMonitorMock);
	}

	[Fact]
	public void Create()
	{
		// Given
		(
			Mock<IConfigContext> configContextMock,
			Mock<ICoreNativeManager> coreNativeManagerMock,
			Mock<IWindowMessageMonitor> windowMessageMonitorMock
		) = CreateStubs();

		// When
		MonitorManager monitorManager =
			new(configContextMock.Object, coreNativeManagerMock.Object, windowMessageMonitorMock.Object);

		// Then
		Assert.Equal(new HMONITOR(1), (monitorManager.PrimaryMonitor as Monitor)!._hmonitor);
		Assert.Equal(new HMONITOR(1), (monitorManager.FocusedMonitor as Monitor)!._hmonitor);
	}

	[Fact]
	public void Create_NoPrimaryMonitorFound()
	{
		// Given
		(
			Mock<IConfigContext> configContextMock,
			Mock<ICoreNativeManager> coreNativeManagerMock,
			Mock<IWindowMessageMonitor> windowMessageMonitorMock
		) = CreateStubs();
		coreNativeManagerMock
			.Setup(n => n.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY))
			.Returns(new HMONITOR(0));

		// When
		// Then
		var result = Assert.Throws<Exception>(
			() =>
				new MonitorManager(
					configContextMock.Object,
					coreNativeManagerMock.Object,
					windowMessageMonitorMock.Object
				)
		);
	}

	[Fact]
	public void Initialize()
	{
		// Given
		(
			Mock<IConfigContext> configContextMock,
			Mock<ICoreNativeManager> coreNativeManagerMock,
			Mock<IWindowMessageMonitor> windowMessageMonitorMock
		) = CreateStubs();
		MonitorManager monitorManager =
			new(configContextMock.Object, coreNativeManagerMock.Object, windowMessageMonitorMock.Object);

		// When
		monitorManager.Initialize();

		// Then
		windowMessageMonitorMock.VerifyAdd(
			m => m.DisplayChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
		windowMessageMonitorMock.VerifyAdd(
			m => m.WorkAreaChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
		windowMessageMonitorMock.VerifyAdd(
			m => m.DpiChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
	}

	[Fact]
	public void WindowFocused_NullMonitor()
	{
		// Given
		(
			Mock<IConfigContext> configContextMock,
			Mock<ICoreNativeManager> coreNativeManagerMock,
			Mock<IWindowMessageMonitor> windowMessageMonitorMock
		) = CreateStubs();
		Mock<IWorkspaceManager> workspaceManagerMock = new();
		workspaceManagerMock.Setup(w => w.GetMonitorForWindow(It.IsAny<IWindow>())).Returns((IMonitor?)null);
		configContextMock.SetupGet(c => c.WorkspaceManager).Returns(workspaceManagerMock.Object);

		MonitorManager monitorManager =
			new(configContextMock.Object, coreNativeManagerMock.Object, windowMessageMonitorMock.Object);

		// When
		monitorManager.WindowFocused(new Mock<IWindow>().Object);

		// Then
		workspaceManagerMock.Verify(w => w.GetMonitorForWindow(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void WindowFocused()
	{
		// Given
		(
			Mock<IConfigContext> configContextMock,
			Mock<ICoreNativeManager> coreNativeManagerMock,
			Mock<IWindowMessageMonitor> windowMessageMonitorMock
		) = CreateStubs();
		Mock<IWorkspaceManager> workspaceManagerMock = new();
		Mock<IMonitor> monitorMock = new();
		monitorMock.Setup(m => m.Equals(null)).Returns(false);
		monitorMock.Setup(m => m.Equals(It.IsAny<IMonitor>())).Returns(true);
		workspaceManagerMock.Setup(w => w.GetMonitorForWindow(It.IsAny<IWindow>())).Returns(monitorMock.Object);
		configContextMock.SetupGet(c => c.WorkspaceManager).Returns(workspaceManagerMock.Object);

		MonitorManager monitorManager =
			new(configContextMock.Object, coreNativeManagerMock.Object, windowMessageMonitorMock.Object);

		// When
		monitorManager.WindowFocused(new Mock<IWindow>().Object);

		// Then
		workspaceManagerMock.Verify(w => w.GetMonitorForWindow(It.IsAny<IWindow>()), Times.Once);
		Assert.Equal(monitorMock.Object, monitorManager.FocusedMonitor);
	}
}
