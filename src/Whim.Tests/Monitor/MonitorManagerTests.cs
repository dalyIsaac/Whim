using AutoFixture;
using FluentAssertions;
using Microsoft.UI.Dispatching;
using NSubstitute;
using NSubstitute.ClearExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Tests;

internal class MonitorManagerCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();
		UpdateGetCurrentMonitors(
			internalCtx,
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
	}

	public static void UpdateGetCurrentMonitors(IInternalContext internalCtx, RECT[] monitorRects)
	{
		internalCtx.CoreNativeManager.ClearSubstitute();
		internalCtx.CoreNativeManager.HasMultipleMonitors().Returns(monitorRects.Length > 1);
		internalCtx.CoreNativeManager
			.EnumDisplayMonitors(Arg.Any<SafeHandle>(), Arg.Any<RECT?>(), Arg.Any<MONITORENUMPROC>(), Arg.Any<LPARAM>())
			.Returns(
				(callInfo) =>
				{
					unsafe
					{
						fixed (RECT* monitorRectsPtr = monitorRects)
						{
							for (int i = 0; i < monitorRects.Length; i++)
							{
								callInfo
									.ArgAt<MONITORENUMPROC>(2)
									.Invoke(new HMONITOR(i + 1), (HDC)0, &monitorRectsPtr[i], (LPARAM)0);
							}
						}
					}

					return (BOOL)true;
				}
			);

		var potentialPrimaryMonitors = monitorRects.Where(r => r.left == 0 && r.top == 0);
		if (potentialPrimaryMonitors.Count() != 1)
		{
			throw new Exception("No primary monitor found");
		}
		RECT primaryRect = potentialPrimaryMonitors.First();

		internalCtx.CoreNativeManager.GetVirtualScreenLeft().Returns(primaryRect.left);
		internalCtx.CoreNativeManager.GetVirtualScreenTop().Returns(primaryRect.top);
		internalCtx.CoreNativeManager.GetVirtualScreenWidth().Returns(primaryRect.right - primaryRect.left);
		internalCtx.CoreNativeManager.GetVirtualScreenHeight().Returns(primaryRect.bottom - primaryRect.top);
		internalCtx.CoreNativeManager
			.GetPrimaryDisplayWorkArea(out RECT _)
			.Returns(
				(callInfo) =>
				{
					callInfo[0] = primaryRect;
					return (BOOL)true;
				}
			);

		// The HMONITORs are non-zero
		if (monitorRects.Length > 1)
		{
			for (int i = 0; i < monitorRects.Length; i++)
			{
				if (primaryRect.Equals(monitorRects[i]))
				{
					internalCtx.CoreNativeManager
						.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
						.Returns(new HMONITOR(i + 1));
					continue;
				}

				internalCtx.CoreNativeManager
					.GetMonitorInfoEx(new HMONITOR(i + 1))
					.Returns(
						new MONITORINFOEXW()
						{
							monitorInfo = new MONITORINFO()
							{
								rcMonitor = monitorRects[i],
								rcWork = monitorRects[i],
								dwFlags = 0
							},
							szDevice = $"DISPLAY {i + 1}"
						}
					);
			}
		}
		else
		{
			internalCtx.CoreNativeManager
				.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
				.Returns(new HMONITOR(1));
		}
	}
}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unnecessary for tests")]
public class MonitorManagerTests
{
	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void Create(IInternalContext internalCtx)
	{
		// Given
		// When
		MonitorManager monitorManager = new(internalCtx);

		// Then
		Assert.Equal(new HMONITOR(2), (monitorManager.PrimaryMonitor as Monitor)!._hmonitor);
		Assert.Equal(new HMONITOR(2), (monitorManager.ActiveMonitor as Monitor)!._hmonitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void Create_NoPrimaryMonitorFound(IInternalContext internalCtx)
	{
		// Given
		internalCtx.CoreNativeManager
			.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
			.Returns(new HMONITOR(0));

		// When
		// Then
		var result = Assert.Throws<Exception>(() => new MonitorManager(internalCtx));
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	[SuppressMessage("Usage", "NS5000:Received check.")]
	internal void Initialize(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);

		// When
		monitorManager.Initialize();

		// Then
		internalCtx.WindowMessageMonitor.Received(1).DisplayChanged += Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).WorkAreaChanged += Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).DpiChanged += Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).SessionChanged += Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowFocused_NullMonitor(IInternalContext internalCtx)
	{
		// Given
		internalCtx.CoreNativeManager
			.MonitorFromWindow(Arg.Any<HWND>(), Arg.Any<MONITOR_FROM_FLAGS>())
			.Returns((HMONITOR)1);

		MonitorManager monitorManager = new(internalCtx);

		// When
		monitorManager.WindowFocused(Substitute.For<IWindow>());

		// Then
		internalCtx.CoreNativeManager.DidNotReceive().GetForegroundWindow();
		internalCtx.CoreNativeManager.Received(1).MonitorFromWindow(Arg.Any<HWND>(), Arg.Any<MONITOR_FROM_FLAGS>());
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowFocused(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);

		// When
		monitorManager.WindowFocused(Substitute.For<IWindow>());

		// Then
		internalCtx.CoreNativeManager.DidNotReceive().GetForegroundWindow();
		internalCtx.CoreNativeManager.Received(1).MonitorFromWindow(Arg.Any<HWND>(), Arg.Any<MONITOR_FROM_FLAGS>());
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowFocused_NullWindow(IInternalContext internalCtx)
	{
		// Given
		internalCtx.CoreNativeManager
			.MonitorFromWindow(Arg.Any<HWND>(), Arg.Any<MONITOR_FROM_FLAGS>())
			.Returns((HMONITOR)1);

		MonitorManager monitorManager = new(internalCtx);

		// When
		monitorManager.WindowFocused(null);

		// Then
		internalCtx.CoreNativeManager.Received(1).GetForegroundWindow();
		internalCtx.CoreNativeManager.Received(1).MonitorFromWindow(Arg.Any<HWND>(), Arg.Any<MONITOR_FROM_FLAGS>());
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

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_DisplayChanged_AddMonitor_HasMultipleMonitors(IInternalContext internalCtx)
	{
		// Given
		// Populate the monitor manager with the default two monitors
		MonitorManager monitorManager = new(internalCtx);
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
		MonitorManagerCustomization.UpdateGetCurrentMonitors(internalCtx, monitorRects);

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() =>
				internalCtx.WindowMessageMonitor.DisplayChanged += Raise.Event<
					EventHandler<WindowMessageMonitorEventArgs>
				>(internalCtx.WindowMessageMonitor, WindowMessageMonitorEventArgs)
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

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_DisplayChanged_AddMonitor_HasSingleMonitor(IInternalContext internalCtx)
	{
		// Given
		// Populate the monitor manager with a single monitor
		RECT primaryRect =
			new()
			{
				left = 0,
				top = 0,
				right = 1920,
				bottom = 1080
			};
		MonitorManagerCustomization.UpdateGetCurrentMonitors(internalCtx, new[] { primaryRect });

		MonitorManager monitorManager = new(internalCtx);
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
		MonitorManagerCustomization.UpdateGetCurrentMonitors(internalCtx, new[] { primaryRect, right });

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() =>
				internalCtx.WindowMessageMonitor.DisplayChanged += Raise.Event<
					EventHandler<WindowMessageMonitorEventArgs>
				>(internalCtx.WindowMessageMonitor, WindowMessageMonitorEventArgs)
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

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_DisplayChanged_RemoveMonitor_HasMultipleMonitors(IInternalContext internalCtx)
	{
		// Given
		// Populate the monitor manager with the default two monitors
		MonitorManager monitorManager = new(internalCtx);
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
		MonitorManagerCustomization.UpdateGetCurrentMonitors(internalCtx, new[] { left });

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() =>
				internalCtx.WindowMessageMonitor.DisplayChanged += Raise.Event<
					EventHandler<WindowMessageMonitorEventArgs>
				>(internalCtx.WindowMessageMonitor, WindowMessageMonitorEventArgs)
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

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_WorkAreaChanged(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() =>
				internalCtx.WindowMessageMonitor.WorkAreaChanged += Raise.Event<
					EventHandler<WindowMessageMonitorEventArgs>
				>(internalCtx.WindowMessageMonitor, WindowMessageMonitorEventArgs)
		);

		// Then
		Assert.Equal(raisedEvent.Arguments.UnchangedMonitors, monitorManager.ToList());
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_DpiChanged(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		var raisedEvent = Assert.Raises<MonitorsChangedEventArgs>(
			h => monitorManager.MonitorsChanged += h,
			h => monitorManager.MonitorsChanged -= h,
			() =>
				internalCtx.WindowMessageMonitor.DpiChanged += Raise.Event<EventHandler<WindowMessageMonitorEventArgs>>(
					internalCtx.WindowMessageMonitor,
					WindowMessageMonitorEventArgs
				)
		);

		// Then
		Assert.Equal(raisedEvent.Arguments.UnchangedMonitors, monitorManager.ToList());
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_SessionChanged(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		internalCtx.WindowMessageMonitor.SessionChanged += Raise.Event<EventHandler<WindowMessageMonitorEventArgs>>(
			internalCtx.WindowMessageMonitor,
			WindowMessageMonitorEventArgs
		);

		// Then
		internalCtx.CoreNativeManager.Received(1).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetMonitorAtPoint_Error_ReturnsFirstMonitor(IInternalContext internalCtx)
	{
		// Given
		Point<int> point = new() { X = 10 * 1000, Y = 10 * 1000 };

		internalCtx.CoreNativeManager
			.MonitorFromPoint(point.ToSystemPoint(), Arg.Any<MONITOR_FROM_FLAGS>())
			.Returns((HMONITOR)0);

		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetMonitorAtPoint(point);

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetMonitorAtPoint_MultipleMonitors_ReturnsCorrectMonitor(IInternalContext internalCtx)
	{
		// Given
		Point<int> point = new() { X = 1930, Y = 10 };

		internalCtx.CoreNativeManager
			.MonitorFromPoint(point.ToSystemPoint(), Arg.Any<MONITOR_FROM_FLAGS>())
			.Returns((HMONITOR)1);

		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetMonitorAtPoint(point);

		// Then
		Assert.Equal(monitorManager.ElementAt(1), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetPreviousMonitor_Error_ReturnsFirstMonitor(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetPreviousMonitor(Substitute.For<IMonitor>());

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetPreviousMonitor_MultipleMonitors_ReturnsCorrectMonitor(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetPreviousMonitor(monitorManager.ElementAt(1));

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetPreviousMonitor_MultipleMonitors_Mod(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetPreviousMonitor(monitorManager.ElementAt(0));

		// Then
		Assert.Equal(monitorManager.ElementAt(1), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetNextMonitor_Error_ReturnsFirstMonitor(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetNextMonitor(Substitute.For<IMonitor>());

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetNextMonitor_MultipleMonitors_ReturnsCorrectMonitor(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetNextMonitor(monitorManager.First());

		// Then
		Assert.Equal(monitorManager.ElementAt(1), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetNextMonitor_MultipleMonitors_Mod(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetNextMonitor(monitorManager.ElementAt(1));

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	[SuppressMessage("Usage", "NS5000:Received check.")]
	internal void Dispose(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		// When
		monitorManager.Dispose();

		// Then
		internalCtx.WindowMessageMonitor.Received(1).DisplayChanged -= Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).WorkAreaChanged -= Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).DpiChanged -= Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
		internalCtx.WindowMessageMonitor.Received(1).SessionChanged -= Arg.Any<
			EventHandler<WindowMessageMonitorEventArgs>
		>();
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void Length(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);

		// When
		int length = monitorManager.Length;

		// Then
		Assert.Equal(2, length);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void MouseHook_MouseLeftButtonUp(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);
		monitorManager.Initialize();

		IMonitor monitor = monitorManager.ActiveMonitor;
		IMonitor? monitor2 = monitorManager.ElementAt(1);

		internalCtx.CoreNativeManager
			.MonitorFromPoint(Arg.Any<Point>(), Arg.Any<MONITOR_FROM_FLAGS>())
			.Returns(((Monitor)monitor2)._hmonitor);

		// When
		Assert.Equal(monitor, monitorManager.ActiveMonitor);
		internalCtx.MouseHook.MouseLeftButtonUp += Raise.Event<EventHandler<MouseEventArgs>>(
			internalCtx.MouseHook,
			new MouseEventArgs(new Point<int>())
		);

		// Then
		Assert.Equal(monitor2, monitorManager.ActiveMonitor);
	}

	#region GetEnumerator
	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetEnumerator(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);

		// When
		IEnumerator<IMonitor> enumerator = monitorManager.GetEnumerator();
		List<IMonitor> monitors = new();

		while (enumerator.MoveNext())
		{
			monitors.Add(enumerator.Current);
		}

		// Then
		Assert.Equal(monitorManager.ToList(), monitors);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetEnumerator_Explicit(IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(internalCtx);

		// When
		IEnumerator enumerator = ((IEnumerable)monitorManager).GetEnumerator();
		List<IMonitor> monitors = new();

		while (enumerator.MoveNext())
		{
			monitors.Add((IMonitor)enumerator.Current);
		}

		// Then
		Assert.Equal(monitorManager.ToList(), monitors);
	}
	#endregion
}
