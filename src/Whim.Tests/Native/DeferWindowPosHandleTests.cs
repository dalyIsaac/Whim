using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

public class DeferWindowPosHandleTests
{
	private const SET_WINDOW_POS_FLAGS DEFAULT_FLAGS =
		SET_WINDOW_POS_FLAGS.SWP_NOZORDER
		| SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
		| SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
		| SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS
		| SET_WINDOW_POS_FLAGS.SWP_NOREPOSITION;

	private static DeferWindowPosState Create(HWND hwnd) => new(hwnd, WindowSize.Normal, Rectangle.UnitSquare<int>());

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWindows(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		DeferWindowPosHandle sut = new(ctx, internalCtx);

		// When
		sut.Dispose();

		// Then
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

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ForceTwoPasses_Argument(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given
		DeferWindowPosHandle sut = new(ctx, internalCtx);
		HWND hwnd = (HWND)123;

		AddMonitorsToManager(ctx, root, CreateMonitor((HMONITOR)1));

		// When
		sut.DeferWindowPos(Create(hwnd), forceTwoPasses: true);
		sut.Dispose();

		// Then
		internalCtx.CoreNativeManager.Received(2).SetWindowPos(hwnd, (HWND)1, 0, 0, 1, 1, DEFAULT_FLAGS);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ForceTwoPasses_Monitor(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given
		DeferWindowPosHandle sut = new(ctx, internalCtx);
		HWND hwnd = (HWND)123;

		IMonitor monitor = CreateMonitor((HMONITOR)1);
		monitor.ScaleFactor.Returns(150);

		AddMonitorsToManager(ctx, root, monitor);

		// When
		sut.DeferWindowPos(Create(hwnd), forceTwoPasses: false);
		sut.Dispose();

		// Then
		internalCtx.CoreNativeManager.Received(2).SetWindowPos(hwnd, (HWND)1, 0, 0, 1, 1, DEFAULT_FLAGS);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoForceTwoPasses(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given
		DeferWindowPosHandle sut = new(ctx, internalCtx);
		HWND hwnd = (HWND)123;

		AddMonitorsToManager(ctx, root, CreateMonitor((HMONITOR)1));

		// When
		sut.DeferWindowPos(Create(hwnd), forceTwoPasses: false);
		sut.Dispose();

		// Then
		internalCtx.CoreNativeManager.Received(1).SetWindowPos(hwnd, (HWND)1, 0, 0, 1, 1, DEFAULT_FLAGS);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SetMultipleWindows(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given
		DeferWindowPosState state1 =
			new(
				(HWND)1,
				WindowSize.Normal,
				Rectangle.UnitSquare<int>(),
				(HWND)123,
				SET_WINDOW_POS_FLAGS.SWP_NOSENDCHANGING
			);
		DeferWindowPosState state2 = new((HWND)2, WindowSize.Minimized, new Rectangle<int>(1, 2, 3, 4), (HWND)234);
		DeferWindowPosState state3 = new((HWND)3, WindowSize.Maximized, new Rectangle<int>(5, 6, 7, 8));
		DeferWindowPosState state4 = new((HWND)4, WindowSize.Normal, new Rectangle<int>(9, 10, 11, 12));

		DeferWindowPosHandle sut = new(ctx, internalCtx, new[] { state1, state2, state3, state4 });

		AddMonitorsToManager(ctx, root, CreateMonitor((HMONITOR)1));

		// When
		sut.Dispose();

		// Then
		internalCtx
			.CoreNativeManager.Received(1)
			.SetWindowPos((HWND)1, (HWND)123, 0, 0, 1, 1, SET_WINDOW_POS_FLAGS.SWP_NOSENDCHANGING);
		ctx.NativeManager.Received(1).GetWindowOffset(state1.Handle);
		ctx.NativeManager.Received(1).ShowWindowNoActivate(state1.Handle);

		internalCtx
			.CoreNativeManager.Received(1)
			.SetWindowPos(
				(HWND)2,
				(HWND)234,
				1,
				2,
				3,
				4,
				DEFAULT_FLAGS | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE
			);
		ctx.NativeManager.Received(1).GetWindowOffset(state2.Handle);
		ctx.NativeManager.Received(1).MinimizeWindow(state2.Handle);

		internalCtx
			.CoreNativeManager.Received(1)
			.SetWindowPos(
				(HWND)3,
				(HWND)1,
				5,
				6,
				7,
				8,
				DEFAULT_FLAGS | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE
			);
		ctx.NativeManager.Received(1).GetWindowOffset(state3.Handle);
		ctx.NativeManager.Received(1).ShowWindowMaximized(state3.Handle);

		internalCtx.CoreNativeManager.Received(1).SetWindowPos((HWND)4, (HWND)1, 9, 10, 11, 12, DEFAULT_FLAGS);
		ctx.NativeManager.Received(1).GetWindowOffset(state4.Handle);
		ctx.NativeManager.Received(1).ShowWindowNoActivate(state4.Handle);
	}
}
