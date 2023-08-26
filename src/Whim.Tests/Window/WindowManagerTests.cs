using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Whim.TestUtils;
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
	private class FakeSafeHandle : UnhookWinEventSafeHandle
	{
		public bool HasDisposed { get; set; }

		private bool _isInvalid;
		public override bool IsInvalid => _isInvalid;

		public FakeSafeHandle(bool isInvalid, bool isClosed)
			: base(default, default)
		{
			_isInvalid = isInvalid;

			if (isClosed)
			{
				Close();
			}
		}

		public void MarkAsInvalid() => _isInvalid = true;

		protected override bool ReleaseHandle()
		{
			return true;
		}

		protected override void Dispose(bool disposing)
		{
			HasDisposed = true;
			base.Dispose(disposing);
		}
	}

	private class Wrapper
	{
		public Mock<IContext> Context = new();

		public Mock<IMonitorManager> MonitorManager;
		public Mock<IInternalMonitorManager> InternalMonitorManager;

		public Mock<IWorkspaceManager> WorkspaceManager;
		public Mock<IInternalWorkspaceManager> InternalWorkspaceManager = new();

		public Mock<ICoreNativeManager> CoreNativeManager = new();
		public Mock<IMouseHook> MouseHook = new();

		public Mock<INativeManager> NativeManager = new();
		public Mock<IWindowMessageMonitor> WindowMessageMonitor = new();
		public Mock<IFilterManager> FilterManager = new();

		public List<FakeSafeHandle> Handles = new();

		public WINEVENTPROC? WinEventProc { get; set; }

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
				.Callback<uint, uint, WINEVENTPROC>(
					(_, _, proc) =>
					{
						WinEventProc = proc;
						Handles.Add(new FakeSafeHandle(false, false));
					}
				)
				.Returns(() => Handles[^1]);
		}

		public Wrapper AllowWindowCreation(HWND hwnd)
		{
			CoreNativeManager.Setup(cnm => cnm.IsSplashScreen(hwnd)).Returns(false);
			CoreNativeManager.Setup(cnm => cnm.IsCloakedWindow(hwnd)).Returns(false);
			CoreNativeManager.Setup(cnm => cnm.IsStandardWindow(hwnd)).Returns(true);
			CoreNativeManager.Setup(cnm => cnm.HasNoVisibleOwner(hwnd)).Returns(true);
			NativeManager.Setup(nm => nm.GetClassName(It.IsAny<HWND>())).Returns("WindowClass");

			CoreNativeManager.Setup(cnm => cnm.GetWindowThreadProcessId(hwnd, out _processId));
			CoreNativeManager.Setup(cnm => cnm.GetProcessNameAndPath((int)ProcessId)).Returns(("name", "path"));
			return this;
		}

		public Wrapper Trigger_MouseLeftButtonDown()
		{
			MouseHook.Raise(m => m.MouseLeftButtonDown += null, new MouseEventArgs(new Point<int>() { X = 1, Y = 2 }));
			return this;
		}

		public Wrapper Trigger_MouseLeftButtonUp()
		{
			MouseHook.Raise(m => m.MouseLeftButtonUp += null, new MouseEventArgs(new Point<int>() { X = 1, Y = 2 }));
			return this;
		}
	}

	[Fact]
	public void OnWindowAdded()
	{
		// Given
		Wrapper wrapper = new();

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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
		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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
		HWND hwnd = new(1);
		Wrapper wrapper = new();
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsSplashScreen(hwnd)).Returns(isSplashScreen);
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsCloakedWindow(hwnd)).Returns(isCloakedWindow);
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsStandardWindow(hwnd)).Returns(isStandardWindow);
		wrapper.CoreNativeManager.Setup(cnm => cnm.HasNoVisibleOwner(hwnd)).Returns(hasNoVisibleOwner);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetProcessNameAndPath((int)wrapper.ProcessId))
			.Throws(new Win32Exception());

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		wrapper.FilterManager.Setup(fm => fm.ShouldBeIgnored(It.IsAny<IWindow>())).Returns(true);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		wrapper.FilterManager.Setup(fm => fm.ShouldBeIgnored(It.IsAny<IWindow>())).Returns(false);
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsWindowMinimized(hwnd)).Returns(true);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		wrapper.WorkspaceManager
			.Setup(wm => wm.GetMonitorForWindow(It.IsAny<IWindow>()))
			.Returns(new Mock<IMonitor>().Object);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

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

	#region OnWindowMoveStart
	[Fact]
	public void WindowsEventHook_OnWindowMoveStart()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();

		// Then
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveStart += h,
			h => windowManager.WindowMoveStart -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0)
		);
		Assert.Null(result.Arguments.CursorDraggedPoint);
	}

	[Fact]
	public void WindowsEventHook_OnWindowMoveStart_GetCursorPos()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);
		windowManager.PostInitialize();

		wrapper.Trigger_MouseLeftButtonDown();
		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetCursorPos(out It.Ref<IPoint<int>>.IsAny))
			.Callback(
				(out IPoint<int> point) =>
				{
					point = new Point<int>(1, 2);
				}
			)
			.Returns(true);

		// When
		windowManager.Initialize();
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveStart += h,
			h => windowManager.WindowMoveStart -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0)
		);

		// Then
		wrapper.CoreNativeManager.Verify(cnm => cnm.GetCursorPos(out It.Ref<IPoint<int>>.IsAny), Times.Once);
		Assert.NotNull(result.Arguments.CursorDraggedPoint);
	}
	#endregion

	#region OnWindowMoved
	[Fact]
	public void WindowsEventHook_OnWindowMoved_DoesNotRaise()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();

		// Then
		WhimAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	[Fact]
	public void WindowsEventHook_OnWindowMoved_DoesNotRaise_MouseIsUp()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		windowManager.PostInitialize();

		wrapper.Trigger_MouseLeftButtonDown().Trigger_MouseLeftButtonUp();

		// Then
		WhimAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	[Fact]
	public void WindowsEventHook_OnWindowMoved_GetCursorPos()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsWindowMinimized(hwnd)).Returns(false);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);
		windowManager.PostInitialize();

		wrapper.Trigger_MouseLeftButtonDown();
		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetCursorPos(out It.Ref<IPoint<int>>.IsAny))
			.Callback(
				(out IPoint<int> point) =>
				{
					point = new Point<int>(1, 2);
				}
			)
			.Returns(true);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);

		// Then
		wrapper.CoreNativeManager.Verify(cnm => cnm.GetCursorPos(out It.Ref<IPoint<int>>.IsAny), Times.Exactly(2));
		Assert.NotNull(result.Arguments.CursorDraggedPoint);
	}

	[Fact]
	public void WindowsEventHook_OnWindowMoved()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();

		// Then
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}
	#endregion

	[Fact]
	public void WindowsEventHook_OnWindowMinimizeStart()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowMinimizeStart += h,
			h => windowManager.WindowMinimizeStart -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MINIMIZESTART, hwnd, 0, 0, 0, 0)
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(wm => wm.WindowMinimizeStart(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void WindowsEventHook_OnWindowMinimizeEnd()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowMinimizeEnd += h,
			h => windowManager.WindowMinimizeEnd -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MINIMIZEEND, hwnd, 0, 0, 0, 0)
		);

		// Then
		wrapper.InternalWorkspaceManager.Verify(wm => wm.WindowMinimizeEnd(It.IsAny<IWindow>()), Times.Once);
	}

	#region OnWindowMoveEnd
	[Fact]
	public void WindowsEventHook_OnWindowMoveEnd_WindowNotMoving()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		wrapper.WorkspaceManager.Verify(wm => wm.GetWorkspaceForWindow(It.IsAny<IWindow>()), Times.Never);
		wrapper.WorkspaceManager.Verify(
			wm => wm.MoveWindowToPoint(It.IsAny<IWindow>(), It.IsAny<IPoint<int>>()),
			Times.Never
		);
	}

	[Fact]
	public void WindowsEventHook_OnWindowMoveEnd_TryMoveWindowEdgesInDirection_NoWorkspace()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);
		wrapper.WorkspaceManager.Setup(wm => wm.GetWorkspaceForWindow(It.IsAny<IWindow>())).Returns<IWindow>(null);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		wrapper.WorkspaceManager.Verify(wm => wm.GetWorkspaceForWindow(It.IsAny<IWindow>()), Times.Once);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
		wrapper.WorkspaceManager.Verify(
			wm => wm.MoveWindowToPoint(It.IsAny<IWindow>(), It.IsAny<IPoint<int>>()),
			Times.Never
		);
	}

	[Fact]
	public void WindowsEventHook_OnWindowMoveEnd_TryMoveWindowEdgesInDirection_DoesNotContainWindowState()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		Mock<IWorkspace> workspace = new();
		wrapper.WorkspaceManager.Setup(wm => wm.GetWorkspaceForWindow(It.IsAny<IWindow>())).Returns(workspace.Object);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveEnd += h,
			h => windowManager.WindowMoveEnd -= h,
			() =>
			{
				wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
				wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);
			}
		);

		// Then
		workspace.Verify(w => w.TryGetWindowLocation(It.IsAny<IWindow>()), Times.Once);
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Never);
		wrapper.CoreNativeManager.Verify(cnm => cnm.GetCursorPos(out It.Ref<IPoint<int>>.IsAny), Times.Once);
		wrapper.WorkspaceManager.Verify(
			wm => wm.MoveWindowToPoint(It.IsAny<IWindow>(), It.IsAny<IPoint<int>>()),
			Times.Never
		);
		Assert.Null(result.Arguments.CursorDraggedPoint);
	}

	[Fact]
	public void WindowsEventHook_OnWindowMoveEnd_MoveWindowToPoint()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		Mock<IWorkspace> workspace = new();
		wrapper.WorkspaceManager.Setup(wm => wm.GetWorkspaceForWindow(It.IsAny<IWindow>())).Returns(workspace.Object);
		wrapper.CoreNativeManager.Setup(cnm => cnm.GetCursorPos(out It.Ref<IPoint<int>>.IsAny)).Returns(true);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveEnd += h,
			h => windowManager.WindowMoveEnd -= h,
			() =>
			{
				windowManager.Initialize();
				wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
				wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);
			}
		);

		// Then
		workspace.Verify(w => w.TryGetWindowLocation(It.IsAny<IWindow>()), Times.Once);
		wrapper.CoreNativeManager.Verify(cnm => cnm.GetCursorPos(out It.Ref<IPoint<int>>.IsAny), Times.Once);
		wrapper.WorkspaceManager.Verify(
			wm => wm.MoveWindowToPoint(It.IsAny<IWindow>(), It.IsAny<IPoint<int>>()),
			Times.Once
		);
	}
	#endregion

	[Fact]
	public void WindowsEventHook_OnWindowMoved_TryMoveWindowEdgesInDirection_CannotGetNewWindowLocation()
	{
		// Given
		HWND hwnd = new(1);
		Wrapper wrapper = new Wrapper().AllowWindowCreation(hwnd);

		Mock<IWorkspace> workspace = new();
		wrapper.WorkspaceManager.Setup(wm => wm.GetWorkspaceForWindow(It.IsAny<IWindow>())).Returns(workspace.Object);

		workspace
			.Setup(w => w.TryGetWindowLocation(It.IsAny<IWindow>()))
			.Returns(
				new WindowState()
				{
					Location = new Location<int>(),
					WindowSize = WindowSize.Normal,
					Window = new Mock<IWindow>().Object
				}
			);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
	}

	public static IEnumerable<object[]> MoveTooManyEdgesData()
	{
		yield return new object[]
		{
			new Location<int>(),
			new Location<int>() { X = 1, Width = 1 }
		};

		yield return new object[]
		{
			new Location<int>(),
			new Location<int>() { Y = 1, Width = 1 }
		};

		yield return new object[]
		{
			new Location<int>(),
			new Location<int>()
			{
				X = 1,
				Width = 1,
				Y = 1,
				Height = 1
			}
		};
	}

	[MemberData(nameof(MoveTooManyEdgesData))]
	[Theory]
	public void WindowsEventHook_OnWindowMoveEnd_TryMoveWindowEdgesInDirection_MoveTooManyEdges(
		ILocation<int> originalLocation,
		ILocation<int> newLocation
	)
	{
		// Given
		Wrapper wrapper = new();

		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		Mock<IWorkspace> workspace = new();
		wrapper.WorkspaceManager.Setup(wm => wm.GetWorkspaceForWindow(It.IsAny<IWindow>())).Returns(workspace.Object);

		workspace
			.Setup(w => w.TryGetWindowLocation(It.IsAny<IWindow>()))
			.Returns(
				new WindowState()
				{
					Location = originalLocation,
					WindowSize = WindowSize.Normal,
					Window = new Mock<IWindow>().Object
				}
			);

		wrapper.NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns(newLocation);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		wrapper.NativeManager.Verify(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()), Times.Once);
		workspace.Verify(
			w => w.MoveWindowEdgesInDirection(It.IsAny<Direction>(), It.IsAny<IPoint<double>>(), It.IsAny<IWindow>()),
			Times.Never()
		);
	}

	public static IEnumerable<object[]> MoveEdgesSuccessData()
	{
		// Moeve left edge to the left
		yield return new object[]
		{
			new Location<int>() { X = 4, Width = 4 },
			new Location<int>() { X = 3, Width = 5 },
			Direction.Left,
			new Point<int>() { X = -1, Y = 0 }
		};

		// Move left edge to the right
		yield return new object[]
		{
			new Location<int>() { X = 4, Width = 4 },
			new Location<int>() { X = 5, Width = 3 },
			Direction.Left,
			new Point<int>() { X = 1, Y = 0 }
		};

		// Move right edge to the right
		yield return new object[]
		{
			new Location<int>() { X = 4, Width = 4 },
			new Location<int>() { X = 4, Width = 5 },
			Direction.Right,
			new Point<int>() { X = 1, Y = 0 }
		};

		// Move right edge to the left
		yield return new object[]
		{
			new Location<int>() { X = 4, Width = 4 },
			new Location<int>() { X = 4, Width = 3 },
			Direction.Right,
			new Point<int>() { X = -1, Y = 0 }
		};

		// Move top edge up
		yield return new object[]
		{
			new Location<int>() { Y = 4, Height = 4 },
			new Location<int>() { Y = 3, Height = 5 },
			Direction.Up,
			new Point<int>() { X = 0, Y = -1 }
		};

		// Move top edge down
		yield return new object[]
		{
			new Location<int>() { Y = 4, Height = 4 },
			new Location<int>() { Y = 5, Height = 3 },
			Direction.Up,
			new Point<int>() { X = 0, Y = 1 }
		};

		// Move bottom edge down
		yield return new object[]
		{
			new Location<int>() { Y = 4, Height = 4 },
			new Location<int>() { Y = 4, Height = 5 },
			Direction.Down,
			new Point<int>() { X = 0, Y = 1 }
		};

		// Move bottom edge up
		yield return new object[]
		{
			new Location<int>() { Y = 4, Height = 4 },
			new Location<int>() { Y = 4, Height = 3 },
			Direction.Down,
			new Point<int>() { X = 0, Y = -1 }
		};
	}

	[Theory]
	[MemberData(nameof(MoveEdgesSuccessData))]
	public void WindowsEventHook_OnWindowMoveEnd_Success(
		ILocation<int> originalLocation,
		ILocation<int> newLocation,
		Direction direction,
		IPoint<int> pixelsDelta
	)
	{
		// Given
		Wrapper wrapper = new();

		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		Mock<IWorkspace> workspace = new();
		wrapper.WorkspaceManager.Setup(wm => wm.GetWorkspaceForWindow(It.IsAny<IWindow>())).Returns(workspace.Object);

		workspace
			.Setup(w => w.TryGetWindowLocation(It.IsAny<IWindow>()))
			.Returns(
				new WindowState()
				{
					Location = originalLocation,
					WindowSize = WindowSize.Normal,
					Window = new Mock<IWindow>().Object
				}
			);

		wrapper.NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns(newLocation);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);

		// Then
		Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveEnd += h,
			h => windowManager.WindowMoveEnd -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0)
		);
		wrapper.WorkspaceManager.Verify(w => w.MoveWindowEdgesInDirection(direction, pixelsDelta, It.IsAny<IWindow>()));
	}

	[Fact]
	public void WindowsEventHook_OnWindowMoveEnd_GetCursorPos()
	{
		// Given
		Wrapper wrapper = new();

		HWND hwnd = new(1);
		wrapper.AllowWindowCreation(hwnd);

		Mock<IWorkspace> workspace = new();
		wrapper.WorkspaceManager.Setup(wm => wm.GetWorkspaceForWindow(It.IsAny<IWindow>())).Returns(workspace.Object);

		workspace
			.Setup(w => w.TryGetWindowLocation(It.IsAny<IWindow>()))
			.Returns(
				new WindowState()
				{
					Location = new Location<int>()
					{
						X = 4,
						Y = 4,
						Width = 4,
						Height = 4
					},
					WindowSize = WindowSize.Normal,
					Window = new Mock<IWindow>().Object
				}
			);

		wrapper.NativeManager
			.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()))
			.Returns(
				new Location<int>()
				{
					X = 5,
					Y = 5,
					Width = 4,
					Height = 4
				}
			);

		IPoint<int> point = new Point<int>(10, 10);
		wrapper.CoreNativeManager.Setup(nm => nm.GetCursorPos(out point)).Returns(true);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Initialize();
		wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);

		// Then
		Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => wrapper.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	[Fact]
	public void PostInitialize()
	{
		// Given
		Wrapper wrapper = new();

		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetAllWindows())
			.Returns(new List<HWND>() { new(1), new(2), new(3) });

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.PostInitialize();

		// Then
		wrapper.CoreNativeManager.Verify(cnm => cnm.IsSplashScreen(It.IsAny<HWND>()), Times.Exactly(3));
	}

	#region Dispose
	[Fact]
	public void Dispose_NotInitialized()
	{
		// Given
		Wrapper wrapper = new();

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		// When
		windowManager.Dispose();

		// Then
		Assert.Empty(wrapper.Handles);
	}

	[Fact]
	public void Dispose_IsClosed()
	{
		// Given
		Wrapper wrapper = new();

		wrapper.CoreNativeManager
			.Setup(n => n.SetWinEventHook(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<WINEVENTPROC>()))
			.Callback<uint, uint, WINEVENTPROC>(
				(_, _, proc) =>
				{
					wrapper.WinEventProc = proc;
					wrapper.Handles.Add(new FakeSafeHandle(false, true));
				}
			)
			.Returns(() => wrapper.Handles[^1]);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		windowManager.Initialize();

		wrapper.Handles.ForEach(h => h.HasDisposed = false);

		// When
		windowManager.Dispose();

		// Then
		wrapper.Handles.Should().OnlyContain(h => !h.HasDisposed);
	}

	[Fact]
	public void Dispose_IsInvalid()
	{
		// Given
		Wrapper wrapper = new();

		wrapper.CoreNativeManager
			.Setup(n => n.SetWinEventHook(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<WINEVENTPROC>()))
			.Callback<uint, uint, WINEVENTPROC>(
				(_, _, proc) =>
				{
					wrapper.WinEventProc = proc;
					wrapper.Handles.Add(new FakeSafeHandle(false, true));
				}
			)
			.Returns(() => wrapper.Handles[^1]);

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		windowManager.Initialize();

		wrapper.Handles.ForEach(h =>
		{
			h.HasDisposed = false;
			h.MarkAsInvalid();
		});

		// When
		windowManager.Dispose();

		// Then
		wrapper.Handles.Should().OnlyContain(h => !h.HasDisposed);
	}

	[Fact]
	public void Dispose_Success()
	{
		// Given
		Wrapper wrapper = new();

		WindowManager windowManager =
			new(wrapper.Context.Object, wrapper.CoreNativeManager.Object, wrapper.MouseHook.Object);

		windowManager.Initialize();

		// When
		windowManager.Dispose();
		windowManager.Dispose();

		// Then
		wrapper.Handles.Should().OnlyContain(h => h.HasDisposed);
	}
	#endregion
}
