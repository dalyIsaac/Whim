using System.Collections.Immutable;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Tests;

public class WindowFocusedTransformTests
{
	private static ImmutableArray<IMonitor> Setup(MutableRootSector mutableRootSector)
	{
		IMonitor monitor0 = Substitute.For<IMonitor>();
		monitor0.Handle.Returns((HMONITOR)0);

		IMonitor monitor1 = Substitute.For<IMonitor>();
		monitor1.Handle.Returns((HMONITOR)1);

		IMonitor monitor2 = Substitute.For<IMonitor>();
		monitor2.Handle.Returns((HMONITOR)2);

		ImmutableArray<IMonitor> monitors = ImmutableArray.Create(monitor0, monitor1, monitor2);
		mutableRootSector.MonitorSector.Monitors = monitors;
		return monitors;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowIsNotNull_WindowIsTrackedByButler(
		IContext ctx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given the window is tracked by the butler
		ImmutableArray<IMonitor> monitors = Setup(mutableRootSector);
		ctx.Butler.Pantry.GetMonitorForWindow(window).Returns(monitors[1]);

		WindowFocusedTransform sut = new(window);

		// When we dispatch the transform
		ctx.Store.Dispatch(sut);

		// Then the active monitor indices are updated.
		Assert.Equal((HMONITOR)1, mutableRootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal((HMONITOR)1, mutableRootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowIsNotNull_WindowIsNotTrackedByButler(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given the window is not tracked by the butler
		Setup(mutableRootSector);

		window.Handle.Returns((HWND)1);
		ctx.Butler.Pantry.GetMonitorForWindow(window).Returns((IMonitor?)null);
		internalCtx
			.CoreNativeManager.MonitorFromWindow(window.Handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns((HMONITOR)2);

		WindowFocusedTransform sut = new(window);

		// When we dispatch the transform
		ctx.Store.Dispatch(sut);

		// Then the active monitor index is updated based on MonitorFromWindow
		Assert.Equal((HMONITOR)2, mutableRootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal((HMONITOR)2, mutableRootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void HandleIsNull(IContext ctx, IInternalContext internalCtx, MutableRootSector mutableRootSector)
	{
		// Given there is no window and GetForegroundWindow returns null
		Setup(mutableRootSector);

		internalCtx.CoreNativeManager.GetForegroundWindow().Returns((HWND)0);

		WindowFocusedTransform sut = new(null);

		// When we dispatch the transform
		ctx.Store.Dispatch(sut);

		// Then the active monitor index is not updated.
		Assert.Equal((HMONITOR)0, mutableRootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal((HMONITOR)0, mutableRootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ForegroundWindowIsNotNull(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		// Given there is no window and GetForegroundWindow returns a handle
		Setup(mutableRootSector);

		HWND handle = (HWND)2;
		internalCtx.CoreNativeManager.GetForegroundWindow().Returns(handle);
		internalCtx
			.CoreNativeManager.MonitorFromWindow(handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns((HMONITOR)2);

		WindowFocusedTransform sut = new(null);

		// When we dispatch the transform
		ctx.Store.Dispatch(sut);

		// Then the active monitor index is updated
		Assert.Equal((HMONITOR)2, mutableRootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal((HMONITOR)0, mutableRootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}
}
