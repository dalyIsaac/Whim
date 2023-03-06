using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unnecessary for tests")]
public class MonitorManagerTests
{
	private class MocksBuilder
	{
		private RECT[] _monitorRects;

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
			UpdateGetCurrentMonitors(
				new[]
				{
					new RECT()
					{
						left = 1920,
						top = 0,
						right = 3840,
						bottom = 1080
					},
					new RECT()
					{
						left = 0,
						top = 0,
						right = 1920,
						bottom = 1080
					}
				}
			);
			CoreNativeManager
				.Setup(n => n.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY))
				.Returns(new HMONITOR(1));

			WindowMessageMonitor = new();
		}

		[MemberNotNull(nameof(_monitorRects))]
		public void UpdateGetCurrentMonitors(RECT[] monitorRects)
		{
			_monitorRects = monitorRects;
			CoreNativeManager.Setup(m => m.HasMultipleMonitors()).Returns(monitorRects.Length > 1);
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
						unsafe
						{
							fixed (RECT* monitorRectsPtr = _monitorRects)
							{
								for (int i = 0; i < _monitorRects.Length; i++)
								{
									callback.Invoke(new HMONITOR(i + 1), (HDC)0, &monitorRectsPtr[i], (LPARAM)0);
								}
							}
						}
					}
				);

			if (_monitorRects.Length > 1)
			{
				for (int i = 0; i < _monitorRects.Length; i++)
				{
					CoreNativeManager
						.Setup(m => m.GetMonitorInfo(new HMONITOR(i + 1), ref It.Ref<MONITORINFOEXW>.IsAny))
						.Callback(
							(HMONITOR hmonitor, ref MONITORINFOEXW infoEx) =>
							{
								infoEx.monitorInfo.rcMonitor = _monitorRects[(int)hmonitor - 1];
								infoEx.monitorInfo.rcWork = _monitorRects[(int)hmonitor - 1];
								infoEx.monitorInfo.dwFlags = 0;
								infoEx.szDevice = $"DISPLAY {i + 1}";
							}
						);
				}
			}
			else
			{
				RECT monitorRect = _monitorRects[0];
				CoreNativeManager.Setup(c => c.GetVirtualScreenLeft()).Returns(monitorRect.left);
				CoreNativeManager.Setup(c => c.GetVirtualScreenTop()).Returns(monitorRect.top);
				CoreNativeManager.Setup(c => c.GetVirtualScreenWidth()).Returns(monitorRect.right - monitorRect.left);
				CoreNativeManager.Setup(c => c.GetVirtualScreenHeight()).Returns(monitorRect.bottom - monitorRect.top);
				CoreNativeManager
					.Setup(c => c.GetPrimaryDisplayWorkArea(out It.Ref<RECT>.IsAny))
					.Callback<RECT>(
						(rect) =>
						{
							rect = monitorRect;
						}
					);
			}
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

	[Fact]
	public void WindowMessageMonitor_DisplayChanged_AddMonitor_HasMultipleMonitors()
	{
		// Given
		MocksBuilder mocksBuilder = new();

		// TODO
	}
}
