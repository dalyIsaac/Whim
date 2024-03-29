using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using AutoFixture;
using FluentAssertions;
using Microsoft.UI.Dispatching;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.ReturnsExtensions;
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
				(
					new RECT()
					{
						left = 1920,
						top = 0,
						right = 3840,
						bottom = 1080
					},
					(HMONITOR)1
				),
				(
					new RECT()
					{
						left = 0,
						top = 0,
						right = 1920,
						bottom = 1080
					},
					(HMONITOR)2
				)
			}
		);
	}

	public static void UpdateGetCurrentMonitors(IInternalContext internalCtx, (RECT Rect, HMONITOR HMonitor)[] monitors)
	{
		internalCtx.CoreNativeManager.ClearSubstitute();
		UpdateMultipleMonitors(internalCtx, monitors);

		RECT[] potentialPrimaryMonitors = monitors.Select(m => m.Rect).Where(r => r.left == 0 && r.top == 0).ToArray();
		if (potentialPrimaryMonitors.Length != 1)
		{
			throw new Exception("No primary monitor found");
		}
		RECT primaryRect = potentialPrimaryMonitors[0];

		// The HMONITORs are non-zero
		foreach ((RECT rect, HMONITOR hMonitor) in monitors)
		{
			if (primaryRect.Equals(rect))
			{
				internalCtx
					.CoreNativeManager.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
					.Returns(hMonitor);
			}

			internalCtx
				.CoreNativeManager.GetMonitorInfoEx(hMonitor)
				.Returns(
					new MONITORINFOEXW()
					{
						monitorInfo = new MONITORINFO()
						{
							cbSize = (uint)Marshal.SizeOf<MONITORINFO>(),
							rcMonitor = rect,
							rcWork = rect,
							dwFlags = 0
						},
						szDevice = $"DISPLAY {(int)hMonitor}"
					}
				);
		}
	}

	public static void UpdateMultipleMonitors(IInternalContext internalCtx, (RECT Rect, HMONITOR HMonitor)[] monitors)
	{
		internalCtx.CoreNativeManager.HasMultipleMonitors().Returns(monitors.Length > 1);
		internalCtx
			.CoreNativeManager.EnumDisplayMonitors(
				Arg.Any<SafeHandle>(),
				Arg.Any<RECT?>(),
				Arg.Any<MONITORENUMPROC>(),
				Arg.Any<LPARAM>()
			)
			.Returns(
				(callInfo) =>
				{
					foreach ((RECT rect, HMONITOR hMonitor) in monitors)
					{
						unsafe
						{
							callInfo.ArgAt<MONITORENUMPROC>(2).Invoke(hMonitor, (HDC)0, &rect, (LPARAM)0);
						}
					}

					return (BOOL)true;
				}
			);
	}
}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unnecessary for tests")]
public class MonitorManagerTests
{
	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void Create(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		// When
		MonitorManager monitorManager = new(ctx, internalCtx);

		// Then
		Assert.Equal(new HMONITOR(2), (monitorManager.PrimaryMonitor as Monitor)!._hmonitor);
		Assert.Equal(new HMONITOR(2), (monitorManager.ActiveMonitor as Monitor)!._hmonitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void Create_NoPrimaryMonitorFound(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx
			.CoreNativeManager.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
			.Returns(new HMONITOR(0));

		// When
		// Then
		var result = Assert.Throws<Exception>(() => new MonitorManager(ctx, internalCtx));
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	[SuppressMessage("Usage", "NS5000:Received check.")]
	internal void Initialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);

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

	#region OnWindowFocused
	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void OnWindowFocused_DefinedWindow_MonitorDefined_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow window,
		IMonitor monitor
	)
	{
		// Given the window is defined, and GetMonitorForWindow returns a monitor
		MonitorManager monitorManager = new(ctx, internalCtx);
		IMonitor originalMonitor = monitorManager.ActiveMonitor;
		ctx.Butler.Pantry.GetMonitorForWindow(window).Returns(monitor);

		// When
		monitorManager.OnWindowFocused(window);

		// Then the active monitors are both updated
		Assert.NotEqual(originalMonitor, monitorManager.ActiveMonitor);
		Assert.Equal(monitor, monitorManager.ActiveMonitor);
		Assert.Equal(monitor, monitorManager.LastWhimActiveMonitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void OnWindowFocused_DefinedWindow_MonitorNotDefined(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given the window is defined, and GetMonitorForWindow does not return a monitor
		MonitorManager monitorManager = new(ctx, internalCtx);
		IMonitor originalMonitor = monitorManager.ActiveMonitor;

		ctx.Butler.Pantry.GetMonitorForWindow(window).ReturnsNull();
		window.Handle.Returns((HWND)1);
		internalCtx
			.CoreNativeManager.MonitorFromWindow(window.Handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns((HMONITOR)1);

		// When
		monitorManager.OnWindowFocused(window);

		// Then the active monitor is updated, but not the LastWhimActiveMonitor
		Assert.NotEqual(originalMonitor, monitorManager.ActiveMonitor);
		Assert.NotEqual(originalMonitor, monitorManager.LastWhimActiveMonitor);
		Assert.Equal((HMONITOR)1, ((Monitor)monitorManager.ActiveMonitor)._hmonitor);
		Assert.Equal((HMONITOR)1, ((Monitor)monitorManager.LastWhimActiveMonitor)._hmonitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void OnWindowFocused_NullWindow_InvalidHandle(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window is null, and Windows returns an invalid handle
		MonitorManager monitorManager = new(ctx, internalCtx);
		IMonitor originalMonitor = monitorManager.ActiveMonitor;
		internalCtx.CoreNativeManager.GetForegroundWindow().Returns((HWND)0);

		// When
		monitorManager.OnWindowFocused(null);

		// Then neither active monitors are updated
		Assert.Equal(originalMonitor, monitorManager.ActiveMonitor);
		Assert.Equal(originalMonitor, monitorManager.LastWhimActiveMonitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void OnWindowFocused_NullWindow_CannotFindHMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window is null, we get a valid HWND, but we can't find the monitor
		MonitorManager monitorManager = new(ctx, internalCtx);
		IMonitor originalMonitor = monitorManager.ActiveMonitor;

		HWND hwnd = (HWND)100;
		HMONITOR hmonitor = (HMONITOR)100;
		internalCtx.CoreNativeManager.GetForegroundWindow().Returns(hwnd);
		internalCtx
			.CoreNativeManager.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns(hmonitor);

		// When
		monitorManager.OnWindowFocused(null);

		// Then neither active monitors are updated
		Assert.Equal(originalMonitor, monitorManager.ActiveMonitor);
		Assert.Equal(originalMonitor, monitorManager.LastWhimActiveMonitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void OnWindowFocused_NullWindow_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window is null, we get a valid HWND, but we can't find the monitor
		MonitorManager monitorManager = new(ctx, internalCtx);
		IMonitor originalMonitor = monitorManager.ActiveMonitor;

		HWND hwnd = (HWND)100;
		HMONITOR hmonitor = (HMONITOR)1;
		internalCtx.CoreNativeManager.GetForegroundWindow().Returns(hwnd);
		internalCtx
			.CoreNativeManager.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns(hmonitor);

		// When
		monitorManager.OnWindowFocused(null);

		// Then the active monitor is updated, but not the LastWhimActiveMonitor
		Assert.NotEqual(originalMonitor, monitorManager.ActiveMonitor);
		Assert.Equal((HMONITOR)1, ((Monitor)monitorManager.ActiveMonitor)._hmonitor);
		Assert.Equal(originalMonitor, monitorManager.LastWhimActiveMonitor);
	}
	#endregion

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
	internal void WindowMessageMonitor_DisplayChanged_AddMonitor_HasMultipleMonitors(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given
		// Populate the monitor manager with the default two monitors
		MonitorManager monitorManager = new(ctx, internalCtx);
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
		MonitorManagerCustomization.UpdateGetCurrentMonitors(
			internalCtx,
			new[] { (right, (HMONITOR)1), (leftTop, (HMONITOR)2), (leftBottom, (HMONITOR)3) }
		);

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

		Assert.Equal(leftTop.ToRectangle(), monitors[0].WorkingArea);
		Assert.Equal(leftBottom.ToRectangle(), monitors[1].WorkingArea);
		Assert.Equal(right.ToRectangle(), monitors[2].WorkingArea);

		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());
		Assert.Single(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		internalCtx.ButlerEventHandlers.Received().OnMonitorsChanged(Arg.Any<MonitorsChangedEventArgs>());

		RECT[] expectedUnchangedRects = new[] { leftTop, right };
		raisedEvent
			.Arguments.UnchangedMonitors.Select(m => m.Bounds)
			.Should()
			.Equal(expectedUnchangedRects.Select(r => r.ToRectangle()));

		RECT[] expectedAddedRects = new[] { leftBottom };
		raisedEvent
			.Arguments.AddedMonitors.Select(m => m.Bounds)
			.Should()
			.Equal(expectedAddedRects.Select(r => r.ToRectangle()));
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_DisplayChanged_AddMonitor_HasSingleMonitor(
		IContext ctx,
		IInternalContext internalCtx
	)
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
		MonitorManagerCustomization.UpdateGetCurrentMonitors(internalCtx, new[] { (primaryRect, (HMONITOR)1) });

		MonitorManager monitorManager = new(ctx, internalCtx);
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
		MonitorManagerCustomization.UpdateGetCurrentMonitors(
			internalCtx,
			new[] { (primaryRect, (HMONITOR)1), (right, (HMONITOR)2) }
		);

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

		Assert.Equal(primaryRect.ToRectangle(), monitors[0].WorkingArea);
		Assert.Equal(right.ToRectangle(), monitors[1].WorkingArea);

		Assert.Single(raisedEvent.Arguments.UnchangedMonitors);
		Assert.Single(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		internalCtx.ButlerEventHandlers.Received().OnMonitorsChanged(Arg.Any<MonitorsChangedEventArgs>());

		RECT[] expectedUnchangedRects = new[] { primaryRect };
		raisedEvent
			.Arguments.UnchangedMonitors.Select(m => m.Bounds)
			.Should()
			.Equal(expectedUnchangedRects.Select(r => r.ToRectangle()));

		RECT[] expectedAddedRects = new[] { right };
		raisedEvent
			.Arguments.AddedMonitors.Select(m => m.Bounds)
			.Should()
			.Equal(expectedAddedRects.Select(r => r.ToRectangle()));
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_DisplayChanged_RemoveMonitor_HasMultipleMonitors(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given
		// Populate the monitor manager with the default two monitors
		MonitorManager monitorManager = new(ctx, internalCtx);
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
		MonitorManagerCustomization.UpdateMultipleMonitors(internalCtx, new[] { (left, (HMONITOR)2) });

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

		Assert.Equal(left.ToRectangle(), monitors[0].WorkingArea);

		Assert.Single(raisedEvent.Arguments.UnchangedMonitors);
		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Single(raisedEvent.Arguments.RemovedMonitors);
		internalCtx.ButlerEventHandlers.Received().OnMonitorsChanged(Arg.Any<MonitorsChangedEventArgs>());

		RECT[] expectedRemovedRects = new[]
		{
			new RECT()
			{
				left = 1920,
				top = 0,
				right = 3840,
				bottom = 1080
			}
		};
		raisedEvent
			.Arguments.RemovedMonitors.Select(m => m.Bounds)
			.Should()
			.Equal(expectedRemovedRects.Select(r => r.ToRectangle()));
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_DisplayChanged_NoChange(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		// Populate the monitor manager with the default two monitors
		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// Set up the monitor manager to have the two monitors
		MonitorManagerCustomization.UpdateMultipleMonitors(
			internalCtx,
			new[]
			{
				(
					new RECT()
					{
						left = 1920,
						top = 0,
						right = 3840,
						bottom = 1080
					},
					(HMONITOR)1
				),
				(
					new RECT()
					{
						left = 0,
						top = 0,
						right = 1920,
						bottom = 1080
					},
					(HMONITOR)2
				)
			}
		);

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

		Assert.Equal(2, raisedEvent.Arguments.UnchangedMonitors.Count());
		Assert.Empty(raisedEvent.Arguments.AddedMonitors);
		Assert.Empty(raisedEvent.Arguments.RemovedMonitors);
		internalCtx.ButlerEventHandlers.DidNotReceive().OnMonitorsChanged(Arg.Any<MonitorsChangedEventArgs>());
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void WindowMessageMonitor_WorkAreaChanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
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
	internal void WindowMessageMonitor_DpiChanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
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
	internal void WindowMessageMonitor_SessionChanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// When
		internalCtx.WindowMessageMonitor.SessionChanged += Raise.Event<EventHandler<WindowMessageMonitorEventArgs>>(
			internalCtx.WindowMessageMonitor,
			WindowMessageMonitorEventArgs
		);

		// Then
		ctx.NativeManager.Received(1).TryEnqueue(Arg.Any<DispatcherQueueHandler>());
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetMonitorAtPoint_Error_ReturnsFirstMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		Point<int> point = new() { X = 10 * 1000, Y = 10 * 1000 };

		internalCtx
			.CoreNativeManager.MonitorFromPoint(point.ToSystemPoint(), Arg.Any<MONITOR_FROM_FLAGS>())
			.Returns((HMONITOR)0);

		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetMonitorAtPoint(point);

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetMonitorAtPoint_MultipleMonitors_ReturnsCorrectMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		Point<int> point = new() { X = 1930, Y = 10 };

		internalCtx
			.CoreNativeManager.MonitorFromPoint(point.ToSystemPoint(), Arg.Any<MONITOR_FROM_FLAGS>())
			.Returns((HMONITOR)1);

		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetMonitorAtPoint(point);

		// Then
		Assert.Equal(monitorManager.ElementAt(1), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetPreviousMonitor_Error_ReturnsFirstMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetPreviousMonitor(Substitute.For<IMonitor>());

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetPreviousMonitor_MultipleMonitors_ReturnsCorrectMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetPreviousMonitor(monitorManager.ElementAt(1));

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetPreviousMonitor_MultipleMonitors_Mod(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetPreviousMonitor(monitorManager.ElementAt(0));

		// Then
		Assert.Equal(monitorManager.ElementAt(1), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetNextMonitor_Error_ReturnsFirstMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetNextMonitor(Substitute.For<IMonitor>());

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetNextMonitor_MultipleMonitors_ReturnsCorrectMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetNextMonitor(monitorManager.First());

		// Then
		Assert.Equal(monitorManager.ElementAt(1), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetNextMonitor_MultipleMonitors_Mod(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		// When
		IMonitor monitor = monitorManager.GetNextMonitor(monitorManager.ElementAt(1));

		// Then
		Assert.Equal(monitorManager.First(), monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	[SuppressMessage("Usage", "NS5000:Received check.")]
	internal void Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
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
	internal void Length(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);

		// When
		int length = monitorManager.Length;

		// Then
		Assert.Equal(2, length);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void MouseHook_MouseLeftButtonUp(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		monitorManager.Initialize();

		IMonitor monitor = monitorManager.ActiveMonitor;
		IMonitor? monitor2 = monitorManager.ElementAt(1);

		internalCtx
			.CoreNativeManager.MonitorFromPoint(Arg.Any<Point>(), Arg.Any<MONITOR_FROM_FLAGS>())
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
	internal void GetEnumerator(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);

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
	internal void GetEnumerator_Explicit(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);

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

	#region ActivateEmptyMonitor
	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void ActivateEmptyMonitor_MonitorNotInManager(IContext ctx, IInternalContext internalCtx, IMonitor monitor)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);

		// When
		monitorManager.ActivateEmptyMonitor(monitor);

		// Then
		Assert.Equal(monitorManager.ActiveMonitor, monitorManager.First());
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void ActivateEmptyMonitor_Success(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		IMonitor monitor = monitorManager.ElementAt(1);

		ctx.Butler.Pantry.GetWorkspaceForMonitor(monitor).Returns(workspace);
		workspace.Windows.GetEnumerator().Returns(new List<IWindow>().GetEnumerator());

		// When
		monitorManager.ActivateEmptyMonitor(monitor);

		// Then
		Assert.Equal(monitorManager.ActiveMonitor, monitor);
	}
	#endregion

	#region GetMonitorByHandle
	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetMonitorByHandle_ReturnsNull(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);

		// When
		IMonitor? monitor = monitorManager.GetMonitorByHandle((HMONITOR)5);

		// Then
		Assert.Null(monitor);
	}

	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
	internal void GetMonitorByHandle_ReturnsMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorManager monitorManager = new(ctx, internalCtx);
		IMonitor monitor = monitorManager.ElementAt(1);

		// When
		IMonitor? result = monitorManager.GetMonitorByHandle((HMONITOR)1);

		// Then
		Assert.Equal(monitor, result);
	}
	#endregion
}
