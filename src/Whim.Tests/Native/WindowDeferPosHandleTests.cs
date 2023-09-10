using Moq;
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
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<IMonitor> Monitor2 { get; } = new();

		public MocksBuilder()
		{
			Context.Setup(c => c.NativeManager).Returns(NativeManager.Object);
			Context.Setup(c => c.MonitorManager).Returns(MonitorManager.Object);

			NativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);
			MonitorManager
				.Setup(m => m.GetEnumerator())
				.Returns(new List<IMonitor>() { Monitor.Object, Monitor2.Object }.GetEnumerator());
		}
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		MocksBuilder mocks = new();

		mocks.NativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);
		mocks.NativeManager
			.Setup(
				n =>
					n.DeferWindowPos(
						It.IsAny<HDWP>(),
						It.IsAny<HWND>(),
						It.IsAny<HWND>(),
						It.IsAny<int>(),
						It.IsAny<int>(),
						It.IsAny<int>(),
						It.IsAny<int>(),
						It.IsAny<SET_WINDOW_POS_FLAGS>()
					)
			)
			.Returns((HDWP)2);
		mocks.NativeManager.Setup(n => n.GetWindowOffset(It.IsAny<HWND>())).Returns(new Location<int>());

		WindowDeferPosHandle handle = new(mocks.Context.Object);

		// When
		handle.DeferWindowPos(data.WindowState, data.HwndInsertAfter, data.Flags);
		handle.Dispose();

		// Then
		mocks.NativeManager.Verify(
			n =>
				n.DeferWindowPos(
					(HDWP)1,
					data.WindowState.Window.Handle,
					data.HwndInsertAfter ?? (HWND)1,
					data.WindowState.Location.X,
					data.WindowState.Location.Y,
					data.WindowState.Location.Width,
					data.WindowState.Location.Height,
					data.ExpectedFlags
				),
			Times.Exactly(2)
		);
		mocks.NativeManager.Verify(n => n.EndDeferWindowPos(It.IsAny<HDWP>()), Times.Exactly(2));
		mocks.NativeManager.Verify(
			n => n.ShowWindowNoActivate(It.IsAny<HWND>()),
			Times.Exactly(data.ExpectedNormalCallCount * 2)
		);
		mocks.NativeManager.Verify(
			n => n.MinimizeWindow(It.IsAny<HWND>()),
			Times.Exactly(data.ExpectedMinimizedCallCount * 2)
		);
		mocks.NativeManager.Verify(
			n => n.ShowWindowMaximized(It.IsAny<HWND>()),
			Times.Exactly(data.ExpectedMaximizedCallCount * 2)
		);
	}

	[Fact]
	public void DeferWindowPos_NoWindowOffset()
	{
		// Given
		MocksBuilder mocks = new();
		mocks.NativeManager.Setup(n => n.GetWindowOffset(It.IsAny<HWND>())).Returns((Location<int>?)null);

		using WindowDeferPosHandle handle = new(mocks.Context.Object);

		// When
		handle.DeferWindowPos(
			new WindowState()
			{
				Location = new Location<int>(),
				Window = new Mock<IWindow>().Object,
				WindowSize = WindowSize.Normal
			}
		);

		// Then
		mocks.NativeManager.Verify(
			n =>
				n.DeferWindowPos(
					It.IsAny<HDWP>(),
					It.IsAny<HWND>(),
					It.IsAny<HWND>(),
					It.IsAny<int>(),
					It.IsAny<int>(),
					It.IsAny<int>(),
					It.IsAny<int>(),
					It.IsAny<SET_WINDOW_POS_FLAGS>()
				),
			Times.Never
		);
	}

	[Fact]
	public void DeferWindowPos_Cancelled()
	{
		// Given
		MocksBuilder mocks = new();

		using CancellationTokenSource cts = new();
		cts.Cancel();

		WindowDeferPosHandle handle = new(mocks.Context.Object, cts.Token);

		// When
		handle.DeferWindowPos(
			new WindowState()
			{
				Location = new Location<int>(),
				Window = new Mock<IWindow>().Object,
				WindowSize = WindowSize.Normal
			}
		);
		handle.Dispose();

		// Then
		mocks.NativeManager.Verify(n => n.BeginDeferWindowPos(It.IsAny<int>()), Times.Never);
	}
}
