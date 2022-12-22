using Moq;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

public class WindowDeferPosHandleTests
{
	[Fact]
	public void Create()
	{
		// Given
		Mock<INativeManager> nativeManager = new();
		nativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.NativeManager).Returns(nativeManager.Object);

		// When
		using WindowDeferPosHandle handle = new(configContext.Object, 2);

		// Then
		nativeManager.Verify(n => n.BeginDeferWindowPos(2), Times.Once);
	}

	private const SET_WINDOW_POS_FLAGS COMMON_FLAGS =
		SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
		| SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
		| SET_WINDOW_POS_FLAGS.SWP_NOCOPYBITS
		| SET_WINDOW_POS_FLAGS.SWP_NOZORDER
		| SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER;

	public static IEnumerable<object[]> DeferWindowPos_Data()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		yield return new object[]
		{
			new WindowState()
			{
				Location = new Location<int>(),
				Window = new Mock<IWindow>().Object,
				WindowSize = WindowSize.Normal
			},
			null,
			COMMON_FLAGS,
			1,
			0,
			0
		};
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		yield return new object[]
		{
			new WindowState()
			{
				Location = new Location<int>(),
				Window = new Mock<IWindow>().Object,
				WindowSize = WindowSize.Minimized
			},
			(HWND)1,
			COMMON_FLAGS | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE,
			0,
			1,
			0
		};
		yield return new object[]
		{
			new WindowState()
			{
				Location = new Location<int>(),
				Window = new Mock<IWindow>().Object,
				WindowSize = WindowSize.Maximized
			},
			(HWND)3,
			COMMON_FLAGS | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE,
			0,
			0,
			1
		};
	}

	[Theory]
	[MemberData(nameof(DeferWindowPos_Data))]
	public void DeferWindowPos(
		WindowState windowState,
		HWND? hwndInsertAfter,
		SET_WINDOW_POS_FLAGS expectedFlags,
		int expectedNormalCallCount,
		int expectedMinimizedCallCount,
		int expectedMaximizedCallCount
	)
	{
		// Given
		Mock<INativeManager> nativeManager = new();
		nativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);
		nativeManager
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
		nativeManager.Setup(n => n.GetWindowOffset(It.IsAny<HWND>())).Returns(new Location<int>());

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.NativeManager).Returns(nativeManager.Object);

		WindowDeferPosHandle handle = new(configContext.Object, 1);

		// When
		handle.DeferWindowPos(windowState, hwndInsertAfter);
		handle.Dispose();

		// Then
		nativeManager.Verify(
			n =>
				n.DeferWindowPos(
					(HDWP)1,
					windowState.Window.Handle,
					hwndInsertAfter ?? (HWND)1,
					windowState.Location.X,
					windowState.Location.Y,
					windowState.Location.Width,
					windowState.Location.Height,
					expectedFlags
				),
			Times.Once
		);
		nativeManager.Verify(n => n.EndDeferWindowPos(It.IsAny<HDWP>()), Times.Once);
		nativeManager.Verify(n => n.ShowWindowNoActivate(It.IsAny<HWND>()), Times.Exactly(expectedNormalCallCount));
		nativeManager.Verify(n => n.MinimizeWindow(It.IsAny<HWND>()), Times.Exactly(expectedMinimizedCallCount));
		nativeManager.Verify(n => n.ShowWindowMaximized(It.IsAny<HWND>()), Times.Exactly(expectedMaximizedCallCount));
	}

	[Fact]
	public void DeferWindowPos_NoWindowOffset()
	{
		// Given
		Mock<INativeManager> nativeManager = new();
		nativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);
		nativeManager.Setup(n => n.GetWindowOffset(It.IsAny<HWND>())).Returns((Location<int>?)null);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.NativeManager).Returns(nativeManager.Object);

		using WindowDeferPosHandle handle = new(configContext.Object, 1);

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
		nativeManager.Verify(
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
	public void SetWindowPosFixScaling_IsPrimary()
	{
		// Given
		Mock<INativeManager> nativeManager = new();
		nativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.NativeManager).Returns(nativeManager.Object);

		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.IsPrimary).Returns(true);

		// When
		WindowDeferPosHandle.SetWindowPosFixScaling(
			configContext.Object,
			new WindowState()
			{
				Location = new Location<int>(),
				Window = new Mock<IWindow>().Object,
				WindowSize = WindowSize.Normal
			},
			monitor.Object
		);

		// Then
		nativeManager.Verify(n => n.GetWindowOffset(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void SetWindowPosFixScaling_NotPrimary_SameScaleFactor()
	{
		// Given
		Mock<INativeManager> nativeManager = new();
		nativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);

		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.IsPrimary).Returns(false);
		monitor.Setup(m => m.ScaleFactor).Returns(1);

		Mock<IMonitorManager> monitorManager = new();
		monitorManager.SetupGet(m => m.PrimaryMonitor).Returns(monitor.Object);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.NativeManager).Returns(nativeManager.Object);
		configContext.Setup(c => c.MonitorManager).Returns(monitorManager.Object);

		// When
		WindowDeferPosHandle.SetWindowPosFixScaling(
			configContext.Object,
			new WindowState()
			{
				Location = new Location<int>(),
				Window = new Mock<IWindow>().Object,
				WindowSize = WindowSize.Normal
			},
			monitor.Object
		);

		// Then
		nativeManager.Verify(n => n.GetWindowOffset(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void SetWindowPosFixScaling_NotPrimary_DifferentScaleFactor()
	{
		// Given
		Mock<INativeManager> nativeManager = new();
		nativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);

		Mock<IMonitor> primaryMonitor = new();
		primaryMonitor.Setup(m => m.ScaleFactor).Returns(1);

		Mock<IMonitorManager> monitorManager = new();
		monitorManager.SetupGet(m => m.PrimaryMonitor).Returns(primaryMonitor.Object);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.NativeManager).Returns(nativeManager.Object);
		configContext.Setup(c => c.MonitorManager).Returns(monitorManager.Object);

		Mock<IMonitor> monitor = new();
		monitor.SetupGet(m => m.IsPrimary).Returns(false);
		monitor.SetupGet(m => m.ScaleFactor).Returns(2);

		// When
		WindowDeferPosHandle.SetWindowPosFixScaling(
			configContext.Object,
			new WindowState()
			{
				Location = new Location<int>(),
				Window = new Mock<IWindow>().Object,
				WindowSize = WindowSize.Normal
			},
			monitor.Object
		);

		// Then
		nativeManager.Verify(n => n.GetWindowOffset(It.IsAny<HWND>()), Times.Exactly(2));
	}
}
