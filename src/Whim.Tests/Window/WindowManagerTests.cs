using Moq;
using System;
using System.ComponentModel;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class WindowManagerTests
{
	private class Wrapper
	{
		public Mock<IContext> Context = new();

		public Mock<IMonitorManager> MonitorManager;
		public Mock<IInternalMonitorManager> InternalMonitorManager;

		public Mock<IWorkspaceManager> WorkspaceManager;
		public Mock<IInternalWorkspaceManager> InternalWorkspaceManager = new();

		public Mock<ICoreNativeManager> CoreNativeManager = new();

		public Mock<INativeManager> NativeManager = new();
		public Mock<IWindowMessageMonitor> WindowMessageMonitor = new();
		public Mock<IFilterManager> FilterManager = new();

		public WINEVENTPROC? WinEventProc;

		private uint _processId = 1;
		public uint ProcessId => _processId;

		public Wrapper()
		{
			InternalMonitorManager = new();
			InternalMonitorManager.Setup(m => m.WindowFocused(It.IsAny<IWindow>()));

			MonitorManager = InternalMonitorManager.As<IMonitorManager>();

			InternalWorkspaceManager.Setup(expression: m => m.WindowAdded(It.IsAny<IWindow>()));
			InternalWorkspaceManager.Setup(m => m.WindowFocused(It.IsAny<IWindow>()));
			InternalWorkspaceManager.Setup(m => m.WindowRemoved(It.IsAny<IWindow>()));
			InternalWorkspaceManager.Setup(m => m.WindowMinimizeStart(It.IsAny<IWindow>()));
			InternalWorkspaceManager.Setup(m => m.WindowMinimizeEnd(It.IsAny<IWindow>()));

			WorkspaceManager = InternalWorkspaceManager.As<IWorkspaceManager>();

			Context.SetupGet(m => m.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(m => m.MonitorManager).Returns(MonitorManager.Object);
			Context.SetupGet(m => m.FilterManager).Returns(FilterManager.Object);
			Context.SetupGet(m => m.NativeManager).Returns(NativeManager.Object);

			// Capture the delegate passed to SetWinEventHook
			CoreNativeManager
				.Setup(n => n.SetWinEventHook(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<WINEVENTPROC>()))
				.Callback<uint, uint, WINEVENTPROC>((_, _, proc) => WinEventProc = proc)
				.Returns(new UnhookWinEventSafeHandle(1));
		}

		public void AllowWindowCreation(HWND hwnd)
		{
			CoreNativeManager.Setup(cnm => cnm.IsSplashScreen(hwnd)).Returns(false);
			CoreNativeManager.Setup(cnm => cnm.IsCloakedWindow(hwnd)).Returns(false);
			CoreNativeManager.Setup(cnm => cnm.IsStandardWindow(hwnd)).Returns(true);
			CoreNativeManager.Setup(cnm => cnm.HasNoVisibleOwner(hwnd)).Returns(true);
			NativeManager.Setup(nm => nm.GetClassName(It.IsAny<HWND>())).Returns("WindowClass");

			CoreNativeManager.Setup(cnm => cnm.GetWindowThreadProcessId(hwnd, out _processId));
			CoreNativeManager.Setup(cnm => cnm.GetProcessNameAndPath((int)ProcessId)).Returns(("name", "path"));
		}
	}

	[Fact]
	public void OnWindowAdded()
	{
		// Given
		Wrapper wrapper = new();

		WindowManager windowManager = new(context: wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowAdded += eh,
			eh => windowManager.WindowAdded -= eh,
			() => windowManager.OnWindowAdded(window.Object)
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowAdded(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowFocused()
	{
		// Given
		Wrapper wrapper = new();

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowFocused += eh,
			eh => windowManager.WindowFocused -= eh,
			() => windowManager.OnWindowFocused(window.Object)
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowFocused(window.Object), Times.Once);
		wrapper.InternalMonitorManager.Verify(m => m.WindowFocused(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowRemoved()
	{
		// Given
		Wrapper wrapper = new();

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowRemoved += eh,
			eh => windowManager.WindowRemoved -= eh,
			() => windowManager.OnWindowRemoved(window.Object)
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowRemoved(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowMinimizeStart()
	{
		// Given
		Wrapper wrapper = new();

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowMinimizeStart += eh,
			eh => windowManager.WindowMinimizeStart -= eh,
			() => windowManager.OnWindowMinimizeStart(window.Object)
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowMinimizeStart(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowMinimizeEnd()
	{
		// Given
		Wrapper wrapper = new();

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowMinimizeEnd += eh,
			eh => windowManager.WindowMinimizeEnd -= eh,
			() => windowManager.OnWindowMinimizeEnd(window.Object)
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowMinimizeEnd(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	private static void InitializeCoreNativeManagerMock(Wrapper wrapper)
	{
		(uint, uint)[] events = new[]
		{
			(PInvoke.EVENT_OBJECT_DESTROY, PInvoke.EVENT_OBJECT_HIDE),
			(PInvoke.EVENT_OBJECT_CLOAKED, PInvoke.EVENT_OBJECT_UNCLOAKED),
			(PInvoke.EVENT_SYSTEM_MOVESIZESTART, PInvoke.EVENT_SYSTEM_MOVESIZEEND),
			(PInvoke.EVENT_SYSTEM_FOREGROUND, PInvoke.EVENT_SYSTEM_FOREGROUND),
			(PInvoke.EVENT_OBJECT_LOCATIONCHANGE, PInvoke.EVENT_OBJECT_LOCATIONCHANGE),
			(PInvoke.EVENT_SYSTEM_MINIMIZESTART, PInvoke.EVENT_SYSTEM_MINIMIZEEND)
		};

		foreach (var (eventMin, eventMax) in events)
		{
			wrapper.CoreNativeManager
				.Setup(n => n.SetWinEventHook(eventMin, eventMax, It.IsAny<WINEVENTPROC>()))
				.Returns(new UnhookWinEventSafeHandle(1));
		}
	}

	[Fact]
	public void Initialize()
	{
		// Given
		Wrapper wrapper = new();
		InitializeCoreNativeManagerMock(wrapper);

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();

		// Then
		wrapper.CoreNativeManager.Verify(
			n => n.SetWinEventHook(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<WINEVENTPROC>()),
			Times.Exactly(6)
		);
	}

	[Fact]
	public void Initialize_Fail()
	{
		// Given
		Wrapper wrapper = new();
		InitializeCoreNativeManagerMock(wrapper);

		wrapper.CoreNativeManager
			.Setup(
				n =>
					n.SetWinEventHook(
						PInvoke.EVENT_SYSTEM_MINIMIZESTART,
						PInvoke.EVENT_SYSTEM_MINIMIZEEND,
						It.IsAny<WINEVENTPROC>()
					)
			)
			.Returns(new UnhookWinEventSafeHandle());

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		// Then
		Assert.Throws<InvalidOperationException>(windowManager.Initialize);
	}

	[InlineData(PInvoke.CHILDID_SELF + 1, 0, 0)]
	[InlineData(PInvoke.CHILDID_SELF, 1, 0)]
	[InlineData(PInvoke.CHILDID_SELF, 0, null)]
	[Theory]
	public void WindowsEventHook_IsEventWindowValid_False(int idObject, int idChild, int? hwndValue)
	{
		// Given
		Wrapper wrapper = new();
		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke(
			(HWINEVENTHOOK)0,
			PInvoke.EVENT_OBJECT_SHOW,
			hwndValue == null ? HWND.Null : (HWND)hwndValue,
			idObject,
			idChild,
			0,
			0
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowAdded(It.IsAny<IWindow>()), Times.Never);
	}

	[InlineData(true, false, true, true)]
	[InlineData(false, true, true, true)]
	[InlineData(false, false, false, true)]
	[InlineData(false, false, true, false)]
	[Theory]
	public void WindowsEventHook_AddWindow_Fail(
		bool isSplashScreen,
		bool isCloakedWindow,
		bool isStandardWindow,
		bool hasNoVisibleOwner
	)
	{
		// Given
		Wrapper wrapper = new();
		HWND hwnd = new(1);
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsSplashScreen(hwnd)).Returns(isSplashScreen);
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsCloakedWindow(hwnd)).Returns(isCloakedWindow);
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsStandardWindow(hwnd)).Returns(isStandardWindow);
		wrapper.CoreNativeManager.Setup(cnm => cnm.HasNoVisibleOwner(hwnd)).Returns(hasNoVisibleOwner);

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then
		wrapper.CoreNativeManager.Verify(
			cnm => cnm.GetWindowThreadProcessId(hwnd, out It.Ref<uint>.IsAny),
			Times.Never
		);
	}

	[Fact]
	public void WindowsEventHook_CreateWindow_Null()
	{
		// Given
		Wrapper wrapper = new();
		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetProcessNameAndPath((int)wrapper.ProcessId))
			.Throws(new Win32Exception());

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowAdded(It.IsAny<IWindow>()), Times.Never);
		wrapper.FilterManager.Verify(fm => fm.ShouldBeIgnored(It.IsAny<IWindow>()), Times.Never);
	}

	[Fact]
	public void WindowsEventHook_IgnoreWindow()
	{
		// Given
		Wrapper wrapper = new();
		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		wrapper.FilterManager.Setup(fm => fm.ShouldBeIgnored(It.IsAny<IWindow>())).Returns(true);

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowAdded(It.IsAny<IWindow>()), Times.Never);
		wrapper.FilterManager.Verify(fm => fm.ShouldBeIgnored(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void WindowsEventHook_WindowIsMinimized()
	{
		// Given
		Wrapper wrapper = new();
		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		wrapper.FilterManager.Setup(fm => fm.ShouldBeIgnored(It.IsAny<IWindow>())).Returns(false);
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsWindowMinimized(hwnd)).Returns(true);

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowAdded(It.IsAny<IWindow>()), Times.Never);
		wrapper.FilterManager.Verify(fm => fm.ShouldBeIgnored(It.IsAny<IWindow>()), Times.Once);
		wrapper.InternalWorkspaceManager.Verify(iwm => iwm.WindowAdded(It.IsAny<IWindow>()), Times.Never);
	}

	[InlineData(PInvoke.EVENT_SYSTEM_FOREGROUND)]
	[InlineData(PInvoke.EVENT_OBJECT_UNCLOAKED)]
	[Theory]
	public void WindowsEventHook_OnWindowFocused(uint eventType)
	{
		// Given
		Wrapper wrapper = new();
		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowFocused += h,
			h => windowManager.WindowFocused -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, eventType, hwnd, 0, 0, 0, 0)
		);

		// Then
		wrapper.InternalMonitorManager.Verify(m => m.WindowFocused(It.IsAny<IWindow>()), Times.Once);
		wrapper.InternalWorkspaceManager.Verify(m => m.WindowFocused(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void WindowsEventHook_OnWindowHidden_IgnoreWindow()
	{
		// Given
		Wrapper wrapper = new();
		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_HIDE, hwnd, 0, 0, 0, 0);

		// Then
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWindow(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void WindowsEventHook_OnWindowHidden()
	{
		// Given
		Wrapper wrapper = new();
		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		wrapper.WorkspaceManager
			.Setup(wm => wm.GetMonitorForWindow(It.IsAny<IWindow>()))
			.Returns(new Mock<IMonitor>().Object);

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowRemoved += h,
			h => windowManager.WindowRemoved -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_HIDE, hwnd, 0, 0, 0, 0)
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(wm => wm.WindowRemoved(It.IsAny<IWindow>()), Times.Once);
	}

	[InlineData(PInvoke.EVENT_OBJECT_DESTROY)]
	[InlineData(PInvoke.EVENT_OBJECT_CLOAKED)]
	[Theory]
	public void WindowsEventHook_OnWindowRemoved(uint eventType)
	{
		// Given
		Wrapper wrapper = new();
		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowRemoved += h,
			h => windowManager.WindowRemoved -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, eventType, hwnd, 0, 0, 0, 0)
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(wm => wm.WindowRemoved(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void WindowsEventHook_OnWindowMoveStart()
	{
		// Given
		Wrapper wrapper = new();
		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		WindowManager windowManager = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		// Then
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowMoveStart += h,
			h => windowManager.WindowMoveStart -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0)
		);
	}
}
