using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

public class DeferWindowPosHandleCustomization : ICustomization
{
	private int _windowCount;

	private int GetNextHwnd()
	{
		_windowCount++;
		return _windowCount;
	}

	public void Customize(IFixture fixture)
	{
		fixture.Register(() =>
		{
			IWindow window = Substitute.For<IWindow>();
			window.Handle.Returns(new HWND(GetNextHwnd()));

			return new DeferWindowPosState(
				new WindowState()
				{
					Rectangle = new Rectangle<int>(),
					Window = window,
					WindowSize = WindowSize.Normal
				}
			);
		});

		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();
		internalCtx.DeferWindowPosManager.ParallelOptions.Returns(new ParallelOptions { MaxDegreeOfParallelism = 1 });
	}
}

public class DeferWindowPosHandleTests
{
	private const SET_WINDOW_POS_FLAGS COMMON_FLAGS =
		SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
		| SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
		| SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS
		| SET_WINDOW_POS_FLAGS.SWP_NOZORDER
		| SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER;

	private static void AssertSetWindowPos(
		IInternalContext internalCtx,
		DeferWindowPosState deferWindowPosState,
		SET_WINDOW_POS_FLAGS expectedFlags = COMMON_FLAGS,
		int expectedCallCount = 1
	)
	{
		internalCtx
			.CoreNativeManager.Received(expectedCallCount)
			.SetWindowPos(
				deferWindowPosState.WindowState.Window.Handle,
				deferWindowPosState.HandleInsertAfter,
				deferWindowPosState.WindowState.Rectangle.X,
				deferWindowPosState.WindowState.Rectangle.Y,
				deferWindowPosState.WindowState.Rectangle.Width,
				deferWindowPosState.WindowState.Rectangle.Height,
				expectedFlags
			);
	}

	[Theory, AutoSubstituteData]
	internal void Dispose_NoWindows(IContext ctx, IInternalContext internalCtx)
	{
		// Given no windows
		using DeferWindowPosHandle handle = new(ctx, internalCtx);

		// When disposing
		handle.Dispose();

		// Then nothing happens
		CustomAssert.NoContextCalls(ctx);
		CustomAssert.NoInternalContextCalls(internalCtx);
	}

	[Theory, AutoSubstituteData<DeferWindowPosHandleCustomization>]
	internal void Dispose_CannotDoLayout(
		IContext ctx,
		IInternalContext internalCtx,
		DeferWindowPosState deferWindowPosState,
		DeferWindowPosState deferWindowPosState2
	)
	{
		// Given DeferWindowPosManager.CanDoLayout() returns false
		deferWindowPosState2.WindowState.WindowSize = WindowSize.Minimized;
		using DeferWindowPosHandle handle =
			new(ctx, internalCtx, new DeferWindowPosState[] { deferWindowPosState, deferWindowPosState2 });
		internalCtx.DeferWindowPosManager.CanDoLayout().Returns(false);

		// When disposing
		handle.Dispose();

		// Then the layout is deferred
		internalCtx.DeferWindowPosManager.DeferLayout(
			Arg.Is<List<DeferWindowPosState>>(x => x.Count == 1 && x[0] == deferWindowPosState),
			Arg.Is<List<DeferWindowPosState>>(x => x.Count == 1 && x[0] == deferWindowPosState2)
		);
	}

	[Theory]
	[InlineAutoSubstituteData<DeferWindowPosHandleCustomization>(100, 1)]
	[InlineAutoSubstituteData<DeferWindowPosHandleCustomization>(200, 2)]
	internal void Dispose_SingleWindow(
		int monitorScaleFactor,
		int numPasses,
		IContext ctx,
		IInternalContext internalCtx,
		DeferWindowPosState deferWindowPosState
	)
	{
		// Given a single window, and a monitor which has a scale factor != 100
		using DeferWindowPosHandle handle = new(ctx, internalCtx);
		internalCtx.DeferWindowPosManager.CanDoLayout().Returns(true);

		IMonitor monitor1 = Substitute.For<IMonitor>();
		monitor1.ScaleFactor.Returns(100);
		IMonitor monitor2 = Substitute.For<IMonitor>();
		monitor2.ScaleFactor.Returns(monitorScaleFactor);

		ctx.MonitorManager.GetEnumerator().Returns((_) => new List<IMonitor>() { monitor1, monitor2 }.GetEnumerator());

		// When disposing
		handle.DeferWindowPos(
			deferWindowPosState.WindowState,
			deferWindowPosState.HandleInsertAfter,
			deferWindowPosState.Flags
		);
		handle.Dispose();

		// Then the window is laid out twice
		AssertSetWindowPos(internalCtx, deferWindowPosState, expectedCallCount: numPasses);
	}

	[Theory]
	[InlineAutoSubstituteData<DeferWindowPosHandleCustomization>(100, 1)]
	[InlineAutoSubstituteData<DeferWindowPosHandleCustomization>(200, 2)]
	internal void Dispose_MultipleWindows(
		int monitorScaleFactor,
		int numPasses,
		IContext ctx,
		IInternalContext internalCtx,
		DeferWindowPosState deferWindowPosState1,
		DeferWindowPosState deferWindowPosState2
	)
	{
		// Given multiple windows, and a monitor which has a scale factor != 100
		using DeferWindowPosHandle handle =
			new(ctx, internalCtx, new DeferWindowPosState[] { deferWindowPosState1, deferWindowPosState2 });
		internalCtx.DeferWindowPosManager.CanDoLayout().Returns(true);

		IMonitor monitor1 = Substitute.For<IMonitor>();
		monitor1.ScaleFactor.Returns(100);
		IMonitor monitor2 = Substitute.For<IMonitor>();
		monitor2.ScaleFactor.Returns(monitorScaleFactor);

		ctx.MonitorManager.GetEnumerator().Returns((_) => new List<IMonitor>() { monitor1, monitor2 }.GetEnumerator());

		// When disposing
		handle.Dispose();

		// Then the windows are laid out twice
		AssertSetWindowPos(internalCtx, deferWindowPosState1, expectedCallCount: numPasses);
		AssertSetWindowPos(internalCtx, deferWindowPosState2, expectedCallCount: numPasses);
	}

	[Theory, AutoSubstituteData<DeferWindowPosHandleCustomization>]
	internal void Dispose_ForceTwoPasses(
		IContext ctx,
		IInternalContext internalCtx,
		DeferWindowPosState deferWindowPosState1,
		DeferWindowPosState deferWindowPosState2
	)
	{
		// Given multiple windows, and a monitor which has a scale factor == 100
		int monitorScaleFactor = 100;
		int numPasses = 2;

		using DeferWindowPosHandle handle = new(ctx, internalCtx);
		internalCtx.DeferWindowPosManager.CanDoLayout().Returns(true);

		IMonitor monitor1 = Substitute.For<IMonitor>();
		monitor1.ScaleFactor.Returns(100);
		IMonitor monitor2 = Substitute.For<IMonitor>();
		monitor2.ScaleFactor.Returns(monitorScaleFactor);

		ctx.MonitorManager.GetEnumerator().Returns((_) => new List<IMonitor>() { monitor1, monitor2 }.GetEnumerator());

		// When disposing
		handle.DeferWindowPos(
			deferWindowPosState1.WindowState,
			deferWindowPosState1.HandleInsertAfter,
			deferWindowPosState1.Flags
		);
		handle.DeferWindowPos(
			deferWindowPosState2.WindowState,
			deferWindowPosState2.HandleInsertAfter,
			deferWindowPosState2.Flags,
			forceTwoPasses: true
		);
		handle.Dispose();

		// Then the windows are laid out twice
		AssertSetWindowPos(internalCtx, deferWindowPosState1, expectedCallCount: numPasses);
		AssertSetWindowPos(internalCtx, deferWindowPosState2, expectedCallCount: numPasses);
	}

	[Theory, AutoSubstituteData<DeferWindowPosHandleCustomization>]
	internal void Dispose_NoWindowOffset(
		IContext ctx,
		IInternalContext internalCtx,
		DeferWindowPosState deferWindowPosState
	)
	{
		// Given a window with no offset
		ctx.NativeManager.GetWindowOffset(deferWindowPosState.WindowState.Window.Handle)
			.Returns((Rectangle<int>?)null);

		using DeferWindowPosHandle handle = new(ctx, internalCtx, new DeferWindowPosState[] { deferWindowPosState });
		internalCtx.DeferWindowPosManager.CanDoLayout().Returns(true);

		// When disposing
		handle.Dispose();

		// Then the window is not laid out
		internalCtx
			.CoreNativeManager.DidNotReceive()
			.SetWindowPos(
				Arg.Any<HWND>(),
				Arg.Any<HWND>(),
				Arg.Any<int>(),
				Arg.Any<int>(),
				Arg.Any<int>(),
				Arg.Any<int>(),
				Arg.Any<SET_WINDOW_POS_FLAGS>()
			);
	}

	[Theory]
	[InlineAutoSubstituteData<DeferWindowPosHandleCustomization>(WindowSize.Maximized, 1, 0, 0)]
	[InlineAutoSubstituteData<DeferWindowPosHandleCustomization>(WindowSize.Minimized, 0, 1, 0)]
	[InlineAutoSubstituteData<DeferWindowPosHandleCustomization>(WindowSize.Normal, 0, 0, 1)]
	internal void Dispose_WindowSize(
		WindowSize windowSize,
		int expectedMaximized,
		int expectedMinimized,
		int expectedNormal,
		IContext ctx,
		IInternalContext internalCtx,
		DeferWindowPosState deferWindowPosState
	)
	{
		// Given a window with a specific size
		deferWindowPosState.WindowState.WindowSize = windowSize;

		using DeferWindowPosHandle handle = new(ctx, internalCtx, new DeferWindowPosState[] { deferWindowPosState });
		internalCtx.DeferWindowPosManager.CanDoLayout().Returns(true);

		// When disposing
		handle.Dispose();

		// Then the window is laid out with the correct flags
		ctx.NativeManager.Received(expectedMaximized)
			.ShowWindowMaximized(deferWindowPosState.WindowState.Window.Handle);
		ctx.NativeManager.Received(expectedMinimized).MinimizeWindow(deferWindowPosState.WindowState.Window.Handle);
		ctx.NativeManager.Received(expectedNormal).ShowWindowNoActivate(deferWindowPosState.WindowState.Window.Handle);

		SET_WINDOW_POS_FLAGS expectedFlags = COMMON_FLAGS;
		if (windowSize == WindowSize.Maximized || windowSize == WindowSize.Minimized)
		{
			expectedFlags |= SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE;
		}

		AssertSetWindowPos(internalCtx, deferWindowPosState, expectedFlags: expectedFlags);
	}

	[Theory, AutoSubstituteData<DeferWindowPosHandleCustomization>]
	internal void DeferWindowPos_DefaultToHwnd1(
		IContext ctx,
		IInternalContext internalCtx,
		DeferWindowPosState deferWindowPosState
	)
	{
		// Given no HWND is provided
		using DeferWindowPosHandle handle = new(ctx, internalCtx);
		internalCtx.DeferWindowPosManager.CanDoLayout().Returns(true);

		handle.DeferWindowPos(deferWindowPosState.WindowState, null, null);

		// When disposing
		handle.Dispose();

		// Then the window is laid out with HWND 1
		AssertSetWindowPos(internalCtx, deferWindowPosState, expectedFlags: COMMON_FLAGS);
	}
}
