using NSubstitute;
using System.Collections.Generic;
using System.Threading;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

public record struct DeferWindowPosTestData(
	IWindowState WindowState,
	HWND? HwndInsertAfter,
	SET_WINDOW_POS_FLAGS? Flags,
	SET_WINDOW_POS_FLAGS ExpectedFlags,
	int ExpectedNormalCallCount,
	int ExpectedMinimizedCallCount,
	int ExpectedMaximizedCallCount
);

public class WindowDeferPosHandleTests
{
	private static IContext Setup()
	{
		IContext ctx = Substitute.For<IContext>();
		ctx.NativeManager.BeginDeferWindowPos(Arg.Any<int>()).Returns((HDWP)1);
		ctx.MonitorManager
			.GetEnumerator()
			.Returns((_) => new List<IMonitor>() { Substitute.For<IMonitor>() }.GetEnumerator());
		return ctx;
	}

	private const SET_WINDOW_POS_FLAGS COMMON_FLAGS =
		SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
		| SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
		| SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS
		| SET_WINDOW_POS_FLAGS.SWP_NOZORDER
		| SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER;

	public static IEnumerable<object[]> DeferWindowPos_Data()
	{
		yield return new object[]
		{
			new DeferWindowPosTestData(
				new WindowState()
				{
					Location = new Location<int>(),
					Window = Substitute.For<IWindow>(),
					WindowSize = WindowSize.Normal
				},
				null,
				null,
				COMMON_FLAGS,
				1,
				0,
				0
			)
		};

		yield return new object[]
		{
			new DeferWindowPosTestData(
				new WindowState()
				{
					Location = new Location<int>(),
					Window = Substitute.For<IWindow>(),
					WindowSize = WindowSize.Minimized
				},
				(HWND)1,
				null,
				COMMON_FLAGS | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE,
				0,
				1,
				0
			)
		};

		yield return new object[]
		{
			new DeferWindowPosTestData(
				new WindowState()
				{
					Location = new Location<int>(),
					Window = Substitute.For<IWindow>(),
					WindowSize = WindowSize.Maximized
				},
				(HWND)3,
				null,
				COMMON_FLAGS | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE,
				0,
				0,
				1
			)
		};

		yield return new object[]
		{
			new DeferWindowPosTestData(
				new WindowState()
				{
					Location = new Location<int>(),
					Window = Substitute.For<IWindow>(),
					WindowSize = WindowSize.Normal
				},
				(HWND)4,
				SET_WINDOW_POS_FLAGS.SWP_NOREDRAW | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE,
				SET_WINDOW_POS_FLAGS.SWP_NOREDRAW | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE,
				1,
				0,
				0
			)
		};
	}

	[Theory]
	[MemberData(nameof(DeferWindowPos_Data))]
	public void DeferWindowPos(DeferWindowPosTestData data)
	{
		// Given
		IContext ctx = Setup();

		ctx.NativeManager.BeginDeferWindowPos(Arg.Any<int>()).Returns((HDWP)1);
		ctx.NativeManager
			.DeferWindowPos(
				Arg.Any<HDWP>(),
				Arg.Any<HWND>(),
				Arg.Any<HWND>(),
				Arg.Any<int>(),
				Arg.Any<int>(),
				Arg.Any<int>(),
				Arg.Any<int>(),
				Arg.Any<SET_WINDOW_POS_FLAGS>()
			)
			.Returns((HDWP)2);
		ctx.NativeManager.GetWindowOffset(Arg.Any<HWND>()).Returns(new Location<int>());

		DeferWindowPosHandle handle = new(ctx);

		// When
		handle.DeferWindowPos(data.WindowState, data.HwndInsertAfter, data.Flags);
		handle.Dispose();

		// Then
		ctx.NativeManager
			.Received(2)
			.DeferWindowPos(
				(HDWP)1,
				data.WindowState.Window.Handle,
				data.HwndInsertAfter ?? (HWND)1,
				data.WindowState.Location.X,
				data.WindowState.Location.Y,
				data.WindowState.Location.Width,
				data.WindowState.Location.Height,
				data.ExpectedFlags
			);
		ctx.NativeManager.Received(2).EndDeferWindowPos(Arg.Any<HDWP>());
		ctx.NativeManager.Received(data.ExpectedNormalCallCount * 2).ShowWindowNoActivate(Arg.Any<HWND>());
		ctx.NativeManager.Received(data.ExpectedMinimizedCallCount * 2).MinimizeWindow(Arg.Any<HWND>());
		ctx.NativeManager.Received(data.ExpectedMaximizedCallCount * 2).ShowWindowMaximized(Arg.Any<HWND>());
	}

	[Fact]
	public void DeferWindowPos_NoWindowOffset()
	{
		// Given
		IContext ctx = Setup();
		ctx.NativeManager.GetWindowOffset(Arg.Any<HWND>()).Returns((Location<int>?)null);

		using DeferWindowPosHandle handle = new(ctx);

		// When
		handle.DeferWindowPos(
			new WindowState()
			{
				Location = new Location<int>(),
				Window = Substitute.For<IWindow>(),
				WindowSize = WindowSize.Normal
			}
		);

		// Then
		ctx.NativeManager
			.DidNotReceive()
			.DeferWindowPos(
				Arg.Any<HDWP>(),
				Arg.Any<HWND>(),
				Arg.Any<HWND>(),
				Arg.Any<int>(),
				Arg.Any<int>(),
				Arg.Any<int>(),
				Arg.Any<int>(),
				Arg.Any<SET_WINDOW_POS_FLAGS>()
			);
	}

	// [Fact]
	// public void DeferWindowPos_Cancelled()
	// {
	// 	// Given
	// 	IContext ctx = Setup();

	// 	using CancellationTokenSource cts = new();
	// 	cts.Cancel();

	// 	WindowDeferPosHandle handle = new(ctx, cts.Token);

	// 	// When
	// 	handle.DeferWindowPos(
	// 		new WindowState()
	// 		{
	// 			Location = new Location<int>(),
	// 			Window = Substitute.For<IWindow>(),
	// 			WindowSize = WindowSize.Normal
	// 		}
	// 	);
	// 	handle.Dispose();

	// 	// Then
	// 	ctx.NativeManager.DidNotReceive().BeginDeferWindowPos(Arg.Any<int>());
	// }
}
