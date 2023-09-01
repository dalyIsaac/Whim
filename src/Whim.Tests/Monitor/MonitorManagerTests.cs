using FluentAssertions;
using Microsoft.UI.Dispatching;
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
	private class Wrapper
	{
		private RECT[] _monitorRects;

		public Mock<IContext> Context { get; }
		public Mock<IWorkspaceManager> WorkspaceManager { get; }
		public Mock<ICoreNativeManager> CoreNativeManager { get; }
		public Mock<IWindowMessageMonitor> WindowMessageMonitor { get; }
		public Mock<IMouseHook> MouseHook { get; }

		public Wrapper()
		{
			Context = new();

			WorkspaceManager = new();
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);

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
			MouseHook = new();
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

			// The HMONITORs are non-zero
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
			else
			{
				CoreNativeManager
					.Setup(n => n.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY))
					.Returns(new HMONITOR(1));
			}
		}
	}

	[Fact]
	public void Create()
	{
		// Given
		Wrapper wrapper = new();

		// When
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);

		// Then
		Assert.Equal(new HMONITOR(2), (monitorManager.PrimaryMonitor as Monitor)!._hmonitor);
		Assert.Equal(new HMONITOR(2), (monitorManager.ActiveMonitor as Monitor)!._hmonitor);
	}

	[Fact]
	public void Create_NoPrimaryMonitorFound()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager
			.Setup(n => n.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY))
			.Returns(new HMONITOR(0));

		// When
		// Then
		var result = Assert.Throws<Exception>(
			() =>
				new MonitorManager(
					wrapper.Context.Object,
					wrapper.CoreNativeManager.Object,
					wrapper.MouseHook.Object,
					wrapper.WindowMessageMonitor.Object
				)
		);
	}

	[Fact]
	public void Initialize()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);

		// When
		monitorManager.Initialize();

		// Then
		wrapper.WindowMessageMonitor.VerifyAdd(
			m => m.DisplayChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
		wrapper.WindowMessageMonitor.VerifyAdd(
			m => m.WorkAreaChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
		wrapper.WindowMessageMonitor.VerifyAdd(
			m => m.DpiChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
		wrapper.WindowMessageMonitor.VerifyAdd(
			m => m.SessionChanged += It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
	}

	[Fact]
	public void WindowFocused_NullMonitor()
	{
		// Given
		Wrapper wrapper = new();

		wrapper.WorkspaceManager.Setup(w => w.GetMonitorForWindow(It.IsAny<IWindow>())).Returns((IMonitor?)null);
		wrapper.Context.SetupGet(c => c.WorkspaceManager).Returns(wrapper.WorkspaceManager.Object);

		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);

		// When
		monitorManager.WindowFocused(new Mock<IWindow>().Object);

		// Then
		wrapper.WorkspaceManager.Verify(w => w.GetMonitorForWindow(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void WindowFocused()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IMonitor> monitorMock = new();
		monitorMock.Setup(m => m.Equals(null)).Returns(false);
		monitorMock.Setup(m => m.Equals(It.IsAny<IMonitor>())).Returns(true);

		wrapper.WorkspaceManager.Setup(w => w.GetMonitorForWindow(It.IsAny<IWindow>())).Returns(monitorMock.Object);
		wrapper.Context.SetupGet(c => c.WorkspaceManager).Returns(wrapper.WorkspaceManager.Object);

		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);

		// When
		monitorManager.WindowFocused(new Mock<IWindow>().Object);

		// Then
		wrapper.WorkspaceManager.Verify(w => w.GetMonitorForWindow(It.IsAny<IWindow>()), Times.Once);
		Assert.Equal(monitorMock.Object, monitorManager.ActiveMonitor);
	}

	private static WindowMessageMonitorEventArgs WindowMessageMonitorEventArgs =>
		new()
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
		};

	[Fact]
	public void WindowMessageMonitor_DisplayChanged_AddMonitor_HasMultipleMonitors()
	{
		// Given
		Wrapper wrapper = new();

		// Populate the monitor manager with the default two monitors
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// Set up the monitor manager to be given a new monitor
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
		wrapper.UpdateGetCurrentMonitors(monitorRects);

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() => wrapper.WindowMessageMonitor.Raise(m => m.DisplayChanged += null, WindowMessageMonitorEventArgs)
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

		RECT[] expectedUnchangedRects = new[] { leftTop, right };
		raisedEvent.Arguments.UnchangedMonitors
			.Select(m => m.Bounds)
			.Should()
			.Equal(expectedUnchangedRects.Select(r => r.ToLocation()));

		RECT[] expectedAddedRects = new[] { leftBottom };
		raisedEvent.Arguments.AddedMonitors
			.Select(m => m.Bounds)
			.Should()
			.Equal(expectedAddedRects.Select(r => r.ToLocation()));
	}

	[Fact]
	public void WindowMessageMonitor_DisplayChanged_AddMonitor_HasSingleMonitor()
	{
		// Given
		Wrapper wrapper = new();

		// Populate the monitor manager with a single monitor
		RECT primaryRect =
			new()
			{
				left = 0,
				top = 0,
				right = 1920,
				bottom = 1080
			};
		wrapper.UpdateGetCurrentMonitors(new[] { primaryRect });

		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// Set up the monitor manager to be given a new monitor
		RECT right =
			new()
			{
				left = 1920,
				top = 0,
				right = 3840,
				bottom = 1080
			};
		wrapper.UpdateGetCurrentMonitors(new[] { primaryRect, right });

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() => wrapper.WindowMessageMonitor.Raise(m => m.DisplayChanged += null, WindowMessageMonitorEventArgs)
		);

		// Then
		List<IMonitor> monitors = monitorManager.ToList();
		Assert.Equal(2, monitors.Count);

		Assert.Equal(primaryRect.ToLocation(), monitors[0].WorkingArea);
		Assert.Equal(right.ToLocation(), monitors[1].WorkingArea);

		Assert.Single(raisedEvent.Arguments.UnchangedMonitors);
		Assert.Single(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);

		RECT[] expectedUnchangedRects = new[] { primaryRect };
		raisedEvent.Arguments.UnchangedMonitors
			.Select(m => m.Bounds)
			.Should()
			.Equal(expectedUnchangedRects.Select(r => r.ToLocation()));

		RECT[] expectedAddedRects = new[] { right };
		raisedEvent.Arguments.AddedMonitors
			.Select(m => m.Bounds)
			.Should()
			.Equal(expectedAddedRects.Select(r => r.ToLocation()));
	}

	[Fact]
	public void WindowMessageMonitor_DisplayChanged_RemoveMonitor_HasMultipleMonitors()
	{
		// Given
		Wrapper wrapper = new();

		// Populate the monitor manager with the default two monitors
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// Set up the monitor manager to have only one monitor
		RECT left =
			new()
			{
				left = 0,
				top = 0,
				right = 1920,
				bottom = 1080
			};
		wrapper.UpdateGetCurrentMonitors(new[] { left });

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() => wrapper.WindowMessageMonitor.Raise(m => m.DisplayChanged += null, WindowMessageMonitorEventArgs)
		);

		// Then
		List<IMonitor> monitors = monitorManager.ToList();
		Assert.Single(monitors);

		Assert.Equal(left.ToLocation(), monitors[0].WorkingArea);

		Assert.Single(raisedEvent.Arguments.UnchangedMonitors);
		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Single(raisedEvent.Arguments.RemovedMonitors);

		RECT[] expectedRemovedRects = new[] { left };
		raisedEvent.Arguments.RemovedMonitors
			.Select(m => m.Bounds)
			.Should()
			.Equal(expectedRemovedRects.Select(r => r.ToLocation()));
	}

	[Fact]
	public void WindowMessageMonitor_WorkAreaChanged()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() => wrapper.WindowMessageMonitor.Raise(m => m.WorkAreaChanged += null, WindowMessageMonitorEventArgs)
		);

		// Then
		Assert.Equal(raisedEvent.Arguments.UnchangedMonitors, monitorManager.ToList());
	}

	[Fact]
	public void WindowMessageMonitor_DpiChanged()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() => wrapper.WindowMessageMonitor.Raise(m => m.DpiChanged += null, WindowMessageMonitorEventArgs)
		);

		// Then
		Assert.Equal(raisedEvent.Arguments.UnchangedMonitors, monitorManager.ToList());
	}

	[Fact]
	public void WindowMessageMonitor_SessionChanged()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		wrapper.WindowMessageMonitor.Raise(m => m.SessionChanged += null, WindowMessageMonitorEventArgs);

		// Then
		wrapper.CoreNativeManager.Verify(cnm => cnm.TryEnqueue(It.IsAny<DispatcherQueueHandler>()), Times.Once);
	}

	[Fact]
	public void GetMonitorAtPoint_Error_ReturnsFirstMonitor()
	{
		// Given
		Wrapper wrapper = new();
		Point<int> point = new() { X = 10 * 1000, Y = 10 * 1000 };

		wrapper.CoreNativeManager
			.Setup(cnm => cnm.MonitorFromPoint(point.ToSystemPoint(), It.IsAny<MONITOR_FROM_FLAGS>()))
			.Returns((HMONITOR)0);

		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetMonitorAtPoint(point);

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Fact]
	public void GetMonitorAtPoint_MultipleMonitors_ReturnsCorrectMonitor()
	{
		// Given
		Wrapper wrapper = new();
		Point<int> point = new() { X = 1930, Y = 10 };

		wrapper.CoreNativeManager
			.Setup(cnm => cnm.MonitorFromPoint(point.ToSystemPoint(), It.IsAny<MONITOR_FROM_FLAGS>()))
			.Returns((HMONITOR)1);

		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetMonitorAtPoint(point);

		// Then
		Assert.Equal(monitorManager.ElementAt(1), monitor);
	}

	[Fact]
	public void GetPreviousMonitor_Error_ReturnsFirstMonitor()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetPreviousMonitor(new Mock<IMonitor>().Object);

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Fact]
	public void GetPreviousMonitor_MultipleMonitors_ReturnsCorrectMonitor()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetPreviousMonitor(monitorManager.ElementAt(1));

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Fact]
	public void GetPreviousMonitor_MultipleMonitors_Mod()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetPreviousMonitor(monitorManager.ElementAt(0));

		// Then
		Assert.Equal(monitorManager.ElementAt(1), monitor);
	}

	[Fact]
	public void GetNextMonitor_Error_ReturnsFirstMonitor()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetNextMonitor(new Mock<IMonitor>().Object);

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Fact]
	public void GetNextMonitor_MultipleMonitors_ReturnsCorrectMonitor()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetNextMonitor(monitorManager.First());

		// Then
		Assert.Equal(monitorManager.ElementAt(1), monitor);
	}

	[Fact]
	public void GetNextMonitor_MultipleMonitors_Mod()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetNextMonitor(monitorManager.ElementAt(1));

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		// When
		monitorManager.Dispose();

		// Then
		wrapper.WindowMessageMonitor.VerifyRemove(
			wmm => wmm.DisplayChanged -= It.IsAny<EventHandler<WindowMessageMonitorEventArgs>>(),
			Times.Once
		);
		wrapper.WindowMessageMonitor.Verify(wmm => wmm.Dispose(), Times.Once);
	}

	[Fact]
	public void Length()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);

		// When
		int length = monitorManager.Length;

		// Then
		Assert.Equal(2, length);
	}

	[Fact]
	public void MouseHook_MouseLeftButtonUp()
	{
		// Given
		Wrapper wrapper = new();
		MonitorManager monitorManager =
			new(
				wrapper.Context.Object,
				wrapper.CoreNativeManager.Object,
				wrapper.MouseHook.Object,
				wrapper.WindowMessageMonitor.Object
			);
		monitorManager.Initialize();

		IMonitor monitor = monitorManager.ActiveMonitor;
		IMonitor? monitor2 = monitorManager.ElementAt(1);

		wrapper.CoreNativeManager
			.Setup(cnm => cnm.MonitorFromPoint(It.IsAny<Point>(), It.IsAny<MONITOR_FROM_FLAGS>()))
			.Returns(((Monitor)monitor2)._hmonitor);

		// When
		Assert.Equal(monitor, monitorManager.ActiveMonitor);
		wrapper.MouseHook.Raise(m => m.MouseLeftButtonUp += null, new MouseEventArgs(new Point<int>()));

		// Then
		Assert.Equal(monitor2, monitorManager.ActiveMonitor);
	}
}
