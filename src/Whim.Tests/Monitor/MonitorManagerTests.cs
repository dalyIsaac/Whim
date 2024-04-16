// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Diagnostics.CodeAnalysis;
// using System.Drawing;
// using System.Linq;
// using System.Runtime.InteropServices;
// using AutoFixture;
// using FluentAssertions;
// using Microsoft.UI.Dispatching;
// using NSubstitute;
// using NSubstitute.ClearExtensions;
// using NSubstitute.ReturnsExtensions;
// using Whim.TestUtils;
// using Windows.Win32.Foundation;
// using Windows.Win32.Graphics.Gdi;
// using Xunit;

// namespace Whim.Tests;

// internal class MonitorManagerCustomization : ICustomization
// {
// 	public void Customize(IFixture fixture)
// 	{
// 		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();
// 		UpdateGetCurrentMonitors(
// 			internalCtx,
// 			new[]
// 			{
// 				(
// 					new RECT()
// 					{
// 						left = 1920,
// 						top = 0,
// 						right = 3840,
// 						bottom = 1080
// 					},
// 					(HMONITOR)1
// 				),
// 				(
// 					new RECT()
// 					{
// 						left = 0,
// 						top = 0,
// 						right = 1920,
// 						bottom = 1080
// 					},
// 					(HMONITOR)2
// 				)
// 			}
// 		);

// 		IContext ctx = fixture.Freeze<IContext>();
// 		ctx.Store.Returns(new Store(ctx, internalCtx));
// 	}
// }

// [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unnecessary for tests")]
// public class MonitorManagerTests
// {
// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void Create(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		// When
// 		MonitorManager monitorManager = new(ctx, internalCtx);

// 		// Then
// 		Assert.Equal(new HMONITOR(2), (monitorManager.PrimaryMonitor as Monitor)!.Handle);
// 		Assert.Equal(new HMONITOR(2), (monitorManager.ActiveMonitor as Monitor)!.Handle);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void Create_NoPrimaryMonitorFound(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		internalCtx
// 			.CoreNativeManager.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
// 			.Returns(new HMONITOR(0));

// 		// When
// 		// Then
// 		var result = Assert.Throws<Exception>(() => new MonitorManager(ctx, internalCtx));
// 	}

// 	#region OnWindowFocused
// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void OnWindowFocused_DefinedWindow_MonitorDefined_Success(
// 		IContext ctx,
// 		IInternalContext internalCtx,
// 		IWindow window,
// 		IMonitor monitor
// 	)
// 	{
// 		// Given the window is defined, and GetMonitorForWindow returns a monitor
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		IMonitor originalMonitor = monitorManager.ActiveMonitor;
// 		ctx.Butler.Pantry.GetMonitorForWindow(window).Returns(monitor);

// 		// When
// 		monitorManager.OnWindowFocused(window);

// 		// Then the active monitors are both updated
// 		Assert.NotEqual(originalMonitor, monitorManager.ActiveMonitor);
// 		Assert.Equal(monitor, monitorManager.ActiveMonitor);
// 		Assert.Equal(monitor, monitorManager.LastWhimActiveMonitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void OnWindowFocused_DefinedWindow_MonitorNotDefined(
// 		IContext ctx,
// 		IInternalContext internalCtx,
// 		IWindow window
// 	)
// 	{
// 		// Given the window is defined, and GetMonitorForWindow does not return a monitor
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		IMonitor originalMonitor = monitorManager.ActiveMonitor;

// 		ctx.Butler.Pantry.GetMonitorForWindow(window).ReturnsNull();
// 		window.Handle.Returns((HWND)1);
// 		internalCtx
// 			.CoreNativeManager.MonitorFromWindow(window.Handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
// 			.Returns((HMONITOR)1);

// 		// When
// 		monitorManager.OnWindowFocused(window);

// 		// Then the active monitor is updated, but not the LastWhimActiveMonitor
// 		Assert.NotEqual(originalMonitor, monitorManager.ActiveMonitor);
// 		Assert.NotEqual(originalMonitor, monitorManager.LastWhimActiveMonitor);
// 		Assert.Equal((HMONITOR)1, ((Monitor)monitorManager.ActiveMonitor).Handle);
// 		Assert.Equal((HMONITOR)1, ((Monitor)monitorManager.LastWhimActiveMonitor).Handle);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void OnWindowFocused_NullWindow_InvalidHandle(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given the window is null, and Windows returns an invalid handle
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		IMonitor originalMonitor = monitorManager.ActiveMonitor;
// 		internalCtx.CoreNativeManager.GetForegroundWindow().Returns((HWND)0);

// 		// When
// 		monitorManager.OnWindowFocused(null);

// 		// Then neither active monitors are updated
// 		Assert.Equal(originalMonitor, monitorManager.ActiveMonitor);
// 		Assert.Equal(originalMonitor, monitorManager.LastWhimActiveMonitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void OnWindowFocused_NullWindow_CannotFindHMonitor(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given the window is null, we get a valid HWND, but we can't find the monitor
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		IMonitor originalMonitor = monitorManager.ActiveMonitor;

// 		HWND hwnd = (HWND)100;
// 		HMONITOR hmonitor = (HMONITOR)100;
// 		internalCtx.CoreNativeManager.GetForegroundWindow().Returns(hwnd);
// 		internalCtx
// 			.CoreNativeManager.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
// 			.Returns(hmonitor);

// 		// When
// 		monitorManager.OnWindowFocused(null);

// 		// Then neither active monitors are updated
// 		Assert.Equal(originalMonitor, monitorManager.ActiveMonitor);
// 		Assert.Equal(originalMonitor, monitorManager.LastWhimActiveMonitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void OnWindowFocused_NullWindow_Success(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given the window is null, we get a valid HWND, but we can't find the monitor
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		IMonitor originalMonitor = monitorManager.ActiveMonitor;

// 		HWND hwnd = (HWND)100;
// 		HMONITOR hmonitor = (HMONITOR)1;
// 		internalCtx.CoreNativeManager.GetForegroundWindow().Returns(hwnd);
// 		internalCtx
// 			.CoreNativeManager.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
// 			.Returns(hmonitor);

// 		// When
// 		monitorManager.OnWindowFocused(null);

// 		// Then the active monitor is updated, but not the LastWhimActiveMonitor
// 		Assert.NotEqual(originalMonitor, monitorManager.ActiveMonitor);
// 		Assert.Equal((HMONITOR)1, ((Monitor)monitorManager.ActiveMonitor).Handle);
// 		Assert.Equal(originalMonitor, monitorManager.LastWhimActiveMonitor);
// 	}
// 	#endregion


// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetMonitorAtPoint_Error_ReturnsFirstMonitor(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		Point<int> point = new() { X = 10 * 1000, Y = 10 * 1000 };

// 		internalCtx
// 			.CoreNativeManager.MonitorFromPoint(point.ToSystemPoint(), Arg.Any<MONITOR_FROM_FLAGS>())
// 			.Returns((HMONITOR)0);

// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		monitorManager.Initialize();

// 		// When
// 		IMonitor monitor = monitorManager.GetMonitorAtPoint(point);

// 		// Then
// 		Assert.Equal(monitorManager.First(), monitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetMonitorAtPoint_MultipleMonitors_ReturnsCorrectMonitor(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		Point<int> point = new() { X = 1930, Y = 10 };

// 		internalCtx
// 			.CoreNativeManager.MonitorFromPoint(point.ToSystemPoint(), Arg.Any<MONITOR_FROM_FLAGS>())
// 			.Returns((HMONITOR)1);

// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		monitorManager.Initialize();

// 		// When
// 		IMonitor monitor = monitorManager.GetMonitorAtPoint(point);

// 		// Then
// 		Assert.Equal(monitorManager.ElementAt(1), monitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetPreviousMonitor_Error_ReturnsFirstMonitor(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		monitorManager.Initialize();

// 		// When
// 		IMonitor monitor = monitorManager.GetPreviousMonitor(Substitute.For<IMonitor>());

// 		// Then
// 		Assert.Equal(monitorManager.First(), monitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetPreviousMonitor_MultipleMonitors_ReturnsCorrectMonitor(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		monitorManager.Initialize();

// 		// When
// 		IMonitor monitor = monitorManager.GetPreviousMonitor(monitorManager.ElementAt(1));

// 		// Then
// 		Assert.Equal(monitorManager.First(), monitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetPreviousMonitor_MultipleMonitors_Mod(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		monitorManager.Initialize();

// 		// When
// 		IMonitor monitor = monitorManager.GetPreviousMonitor(monitorManager.ElementAt(0));

// 		// Then
// 		Assert.Equal(monitorManager.ElementAt(1), monitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetNextMonitor_Error_ReturnsFirstMonitor(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		monitorManager.Initialize();

// 		// When
// 		IMonitor monitor = monitorManager.GetNextMonitor(Substitute.For<IMonitor>());

// 		// Then
// 		Assert.Equal(monitorManager.First(), monitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetNextMonitor_MultipleMonitors_ReturnsCorrectMonitor(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		monitorManager.Initialize();

// 		// When
// 		IMonitor monitor = monitorManager.GetNextMonitor(monitorManager.First());

// 		// Then
// 		Assert.Equal(monitorManager.ElementAt(1), monitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetNextMonitor_MultipleMonitors_Mod(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		monitorManager.Initialize();

// 		// When
// 		IMonitor monitor = monitorManager.GetNextMonitor(monitorManager.ElementAt(1));

// 		// Then
// 		Assert.Equal(monitorManager.First(), monitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void Length(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);

// 		// When
// 		int length = monitorManager.Length;

// 		// Then
// 		Assert.Equal(2, length);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void MouseHook_MouseLeftButtonUp(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		monitorManager.Initialize();

// 		IMonitor monitor = monitorManager.ActiveMonitor;
// 		IMonitor? monitor2 = monitorManager.ElementAt(1);

// 		internalCtx
// 			.CoreNativeManager.MonitorFromPoint(Arg.Any<Point>(), Arg.Any<MONITOR_FROM_FLAGS>())
// 			.Returns(((Monitor)monitor2).Handle);

// 		// When
// 		Assert.Equal(monitor, monitorManager.ActiveMonitor);
// 		internalCtx.MouseHook.MouseLeftButtonUp += Raise.Event<EventHandler<MouseEventArgs>>(
// 			internalCtx.MouseHook,
// 			new MouseEventArgs(new Point<int>())
// 		);

// 		// Then
// 		Assert.Equal(monitor2, monitorManager.ActiveMonitor);
// 	}

// 	#region GetEnumerator
// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetEnumerator(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);

// 		// When
// 		IEnumerator<IMonitor> enumerator = monitorManager.GetEnumerator();
// 		List<IMonitor> monitors = new();

// 		while (enumerator.MoveNext())
// 		{
// 			monitors.Add(enumerator.Current);
// 		}

// 		// Then
// 		Assert.Equal(monitorManager.ToList(), monitors);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetEnumerator_Explicit(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);

// 		// When
// 		IEnumerator enumerator = ((IEnumerable)monitorManager).GetEnumerator();
// 		List<IMonitor> monitors = new();

// 		while (enumerator.MoveNext())
// 		{
// 			monitors.Add((IMonitor)enumerator.Current);
// 		}

// 		// Then
// 		Assert.Equal(monitorManager.ToList(), monitors);
// 	}
// 	#endregion

// 	#region ActivateEmptyMonitor
// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void ActivateEmptyMonitor_MonitorNotInManager(IContext ctx, IInternalContext internalCtx, IMonitor monitor)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);

// 		// When
// 		monitorManager.ActivateEmptyMonitor(monitor);

// 		// Then
// 		Assert.Equal(monitorManager.ActiveMonitor, monitorManager.First());
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void ActivateEmptyMonitor_Success(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		IMonitor monitor = monitorManager.ElementAt(1);

// 		ctx.Butler.Pantry.GetWorkspaceForMonitor(monitor).Returns(workspace);
// 		workspace.Windows.GetEnumerator().Returns(new List<IWindow>().GetEnumerator());

// 		// When
// 		monitorManager.ActivateEmptyMonitor(monitor);

// 		// Then
// 		Assert.Equal(monitorManager.ActiveMonitor, monitor);
// 	}
// 	#endregion

// 	#region GetMonitorByHandle
// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetMonitorByHandle_ReturnsNull(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);

// 		// When
// 		IMonitor? monitor = monitorManager.GetMonitorByHandle((HMONITOR)5);

// 		// Then
// 		Assert.Null(monitor);
// 	}

// 	[Theory, AutoSubstituteData<MonitorManagerCustomization>]
// 	internal void GetMonitorByHandle_ReturnsMonitor(IContext ctx, IInternalContext internalCtx)
// 	{
// 		// Given
// 		MonitorManager monitorManager = new(ctx, internalCtx);
// 		IMonitor monitor = monitorManager.ElementAt(1);

// 		// When
// 		IMonitor? result = monitorManager.GetMonitorByHandle((HMONITOR)1);

// 		// Then
// 		Assert.Equal(monitor, result);
// 	}
// 	#endregion
// }
