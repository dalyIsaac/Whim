using NSubstitute;
using System.Collections.Generic;
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

public class DeferWindowPosHandleTests
{
	private class Wrapper
	{
		public IContext Context { get; } = Substitute.For<IContext>();

		public IInternalContext InternalContext { get; } = Substitute.For<IInternalContext>();

		public Wrapper()
		{
			Context.MonitorManager
				.GetEnumerator()
				.Returns((_) => new List<IMonitor>() { Substitute.For<IMonitor>() }.GetEnumerator());
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
		Wrapper wrapper = new();

		wrapper.Context.NativeManager.GetWindowOffset(Arg.Any<HWND>()).Returns(new Location<int>());
		wrapper.InternalContext.DeferWindowPosManager.CanDoLayout().Returns(true);

		DeferWindowPosHandle handle = new(wrapper.Context, wrapper.InternalContext);

		// When
		handle.DeferWindowPos(data.WindowState, data.HwndInsertAfter, data.Flags);
		handle.Dispose();

		// Then
		wrapper.InternalContext.CoreNativeManager
			.Received(2)
			.SetWindowPos(
				data.WindowState.Window.Handle,
				data.HwndInsertAfter ?? (HWND)1,
				data.WindowState.Location.X,
				data.WindowState.Location.Y,
				data.WindowState.Location.Width,
				data.WindowState.Location.Height,
				data.ExpectedFlags
			);

		wrapper.Context.NativeManager.Received(data.ExpectedNormalCallCount * 2).ShowWindowNoActivate(Arg.Any<HWND>());
		wrapper.Context.NativeManager.Received(data.ExpectedMinimizedCallCount * 2).MinimizeWindow(Arg.Any<HWND>());
		wrapper.Context.NativeManager
			.Received(data.ExpectedMaximizedCallCount * 2)
			.ShowWindowMaximized(Arg.Any<HWND>());
	}

	[Fact]
	public void DeferWindowPos_NoWindowOffset()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.Context.NativeManager.GetWindowOffset(Arg.Any<HWND>()).Returns((Location<int>?)null);

		using DeferWindowPosHandle handle = new(wrapper.Context, wrapper.InternalContext);

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
		wrapper.InternalContext.CoreNativeManager
			.DidNotReceive()
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
}
