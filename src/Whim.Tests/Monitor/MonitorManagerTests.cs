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
	private class MocksBuilder
	{
		public Mock<IConfigContext> ConfigContext { get; }
		public Mock<IWorkspaceManager> WorkspaceManager { get; }
		public Mock<ICoreNativeManager> CoreNativeManager { get; }
		public Mock<IWindowMessageMonitor> WindowMessageMonitor { get; }

		public MocksBuilder()
		{
			ConfigContext = new();

			WorkspaceManager = new();
			ConfigContext.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);

			CoreNativeManager = new();
			CoreNativeManager.Setup(m => m.HasMultipleMonitors()).Returns(true);
			CoreNativeManager
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
			CoreNativeManager
				.Setup(n => n.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY))
				.Returns(new HMONITOR(1));

			WindowMessageMonitor = new();
		}
	}

	[Fact]
	public void Create()
	{
		// Given
		MocksBuilder mocksBuilder = new();

		// When
		MonitorManager monitorManager =
			new(
				mocksBuilder.ConfigContext.Object,
				mocksBuilder.CoreNativeManager.Object,
				mocksBuilder.WindowMessageMonitor.Object
			);

		// Then
		Assert.Equal(new HMONITOR(1), (monitorManager.PrimaryMonitor as Monitor)!._hmonitor);
		Assert.Equal(new HMONITOR(1), (monitorManager.FocusedMonitor as Monitor)!._hmonitor);
	}

	[Fact]
	public void Create_NoPrimaryMonitorFound()
	{
		// Given
		MocksBuilder mocksBuilder = new();
		mocksBuilder.CoreNativeManager
			.Setup(n => n.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY))
			.Returns(new HMONITOR(0));

		// When
		// Then
		var result = Assert.Throws<Exception>(
			() =>
				new MonitorManager(
					mocksBuilder.ConfigContext.Object,
					mocksBuilder.CoreNativeManager.Object,
					mocksBuilder.WindowMessageMonitor.Object
				)
		);
	}

	[Fact]
	public void Initialize()
	{
		// Given
		MocksBuilder mocksBuilder = new();
		MonitorManager monitorManager =
			new(
				mocksBuilder.ConfigContext.Object,
				mocksBuilder.CoreNativeManager.Object,
				mocksBuilder.WindowMessageMonitor.Object
			);

		// When
		monitorManager.Initialize();

		// Then
		mocksBuilder.WindowMessageMonitor.VerifyAdd(
			m => m.DisplayChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
		mocksBuilder.WindowMessageMonitor.VerifyAdd(
			m => m.WorkAreaChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
		mocksBuilder.WindowMessageMonitor.VerifyAdd(
			m => m.DpiChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
	}

	[Fact]
	public void WindowFocused_NullMonitor()
	{
		// Given
		MocksBuilder mocksBuilder = new();

		mocksBuilder.WorkspaceManager.Setup(w => w.GetMonitorForWindow(It.IsAny<IWindow>())).Returns((IMonitor?)null);
		mocksBuilder.ConfigContext.SetupGet(c => c.WorkspaceManager).Returns(mocksBuilder.WorkspaceManager.Object);

		MonitorManager monitorManager =
			new(
				mocksBuilder.ConfigContext.Object,
				mocksBuilder.CoreNativeManager.Object,
				mocksBuilder.WindowMessageMonitor.Object
			);

		// When
		monitorManager.WindowFocused(new Mock<IWindow>().Object);

		// Then
		mocksBuilder.WorkspaceManager.Verify(w => w.GetMonitorForWindow(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void WindowFocused()
	{
		// Given
		MocksBuilder mocksBuilder = new();

		Mock<IMonitor> monitorMock = new();
		monitorMock.Setup(m => m.Equals(null)).Returns(false);
		monitorMock.Setup(m => m.Equals(It.IsAny<IMonitor>())).Returns(true);

		mocksBuilder.WorkspaceManager
			.Setup(w => w.GetMonitorForWindow(It.IsAny<IWindow>()))
			.Returns(monitorMock.Object);
		mocksBuilder.ConfigContext.SetupGet(c => c.WorkspaceManager).Returns(mocksBuilder.WorkspaceManager.Object);

		MonitorManager monitorManager =
			new(
				mocksBuilder.ConfigContext.Object,
				mocksBuilder.CoreNativeManager.Object,
				mocksBuilder.WindowMessageMonitor.Object
			);

		// When
		monitorManager.WindowFocused(new Mock<IWindow>().Object);

		// Then
		mocksBuilder.WorkspaceManager.Verify(w => w.GetMonitorForWindow(It.IsAny<IWindow>()), Times.Once);
		Assert.Equal(monitorMock.Object, monitorManager.FocusedMonitor);
	}
}
