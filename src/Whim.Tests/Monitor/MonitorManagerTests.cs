using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
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

			var potentialPrimaryMonitors = monitorRects.Where(r => r.left == 0 && r.top == 0);
			if (potentialPrimaryMonitors.Count() != 1)
			{
				throw new Exception("No primary monitor found");
			}
			RECT primaryRect = potentialPrimaryMonitors.First();

			CoreNativeManager.Setup(c => c.GetVirtualScreenLeft()).Returns(primaryRect.left);
			CoreNativeManager.Setup(c => c.GetVirtualScreenTop()).Returns(primaryRect.top);
			CoreNativeManager.Setup(c => c.GetVirtualScreenWidth()).Returns(primaryRect.right - primaryRect.left);
			CoreNativeManager
				.Setup(expression: c => c.GetVirtualScreenHeight())
				.Returns(primaryRect.bottom - primaryRect.top);
			CoreNativeManager.Setup(c => c.GetPrimaryDisplayWorkArea(out primaryRect));

			if (_monitorRects.Length > 1)
			{
				for (int i = 0; i < _monitorRects.Length; i++)
				{
					if (primaryRect.Equals(_monitorRects[i]))
					{
						CoreNativeManager
							.Setup(
								n => n.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
							)
							.Returns(new HMONITOR(i + 1));
						continue;
					}

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

		MonitorManager monitorManager =
			new(
				mocksBuilder.ConfigContext.Object,
				mocksBuilder.CoreNativeManager.Object,
				mocksBuilder.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		RECT right =
			new()
			{
				left = 1920,
				top = 0,
				right = 3840,
				bottom = 1080
			};
		RECT leftTop =
			new()
			{
				left = 0,
				top = 0,
				right = 1920,
				bottom = 1080
			};
		RECT leftBottom =
			new()
			{
				left = 0,
				top = 1080,
				right = 1920,
				bottom = 2160
			};
		RECT[] monitorRects = new[] { right, leftTop, leftBottom };
		mocksBuilder.UpdateGetCurrentMonitors(monitorRects);

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() =>
				mocksBuilder.WindowMessageMonitor.Raise(
					m => m.DisplayChanged += null,
					new WindowMessageMonitorEventArgs()
					{
						MessagePayload = new WindowMessageMonitorEventArgsPayload()
						{
							HWnd = new HWND(1),
							UMsg = 0,
							WParam = 0,
							LParam = 0
						},
						Handled = false,
						Result = IntPtr.Zero
					}
				)
		);

		// Then
		List<IMonitor> monitors = monitorManager.ToList();
		Assert.Equal(3, monitors.Count);

		Assert.Equal(leftTop.ToLocation(), monitors[0].WorkingArea);
		Assert.Equal(leftBottom.ToLocation(), monitors[1].WorkingArea);
		Assert.Equal(right.ToLocation(), monitors[2].WorkingArea);

		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());
		Assert.Single(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);

		IMonitor first = raisedEvent.Arguments.UnchangedMonitors.First();
		Assert.Equal(leftTop.ToLocation(), first.WorkingArea);
	}
}
