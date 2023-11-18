using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.UI.Dispatching;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Whim.TestUtils;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Xunit;

namespace Whim.Tests;

internal class WindowManagerCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IWorkspaceManager workspaceManager = Substitute.For<IWorkspaceManager, IInternalWorkspaceManager>();
		IMonitorManager monitorManager = Substitute.For<IMonitorManager, IInternalMonitorManager>();

		IContext context = fixture.Freeze<IContext>();
		context.WorkspaceManager.Returns(workspaceManager);
		context.MonitorManager.Returns(monitorManager);
	}
}

internal class WindowManagerFakeSafeHandle : UnhookWinEventSafeHandle
{
	public bool HasDisposed { get; set; }

	private bool _isInvalid;
	public override bool IsInvalid => _isInvalid;

	public WindowManagerFakeSafeHandle(bool isInvalid, bool isClosed)
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

/// <summary>
/// Captures the <see cref="WINEVENTPROC"/> passed to <see cref="CoreNativeManager.SetWinEventHook(uint, uint, WINEVENTPROC)"/>,
/// and stores the <see cref="WindowManagerFakeSafeHandle"/> returned by <see cref="CoreNativeManager.SetWinEventHook(uint, uint, WINEVENTPROC)"/>.
/// </summary>
internal class CaptureWinEventProc
{
	public WINEVENTPROC? WinEventProc { get; private set; }
	public List<WindowManagerFakeSafeHandle> Handles { get; } = new();

	public static CaptureWinEventProc Create(
		IInternalContext internalCtx,
		bool isInvalid = false,
		bool isClosed = false
	)
	{
		CaptureWinEventProc capture = new();
		internalCtx
			.CoreNativeManager
			.SetWinEventHook(Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<WINEVENTPROC>())
			.Returns(callInfo =>
			{
				capture.WinEventProc = callInfo.ArgAt<WINEVENTPROC>(2);
				capture.Handles.Add(new WindowManagerFakeSafeHandle(isInvalid, isClosed));
				return capture.Handles[^1];
			});
		return capture;
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class WindowManagerTests
{
	private const uint _processId = 1;

	private static void AllowWindowCreation(IContext ctx, IInternalContext internalCtx, HWND hwnd)
	{
		internalCtx.CoreNativeManager.IsSplashScreen(hwnd).Returns(false);
		internalCtx.CoreNativeManager.IsCloakedWindow(hwnd).Returns(false);
		internalCtx.CoreNativeManager.IsStandardWindow(hwnd).Returns(true);
		internalCtx.CoreNativeManager.HasNoVisibleOwner(hwnd).Returns(true);

		ctx.NativeManager.GetClassName(hwnd).Returns("WindowClass");
		internalCtx
			.CoreNativeManager
			.GetWindowThreadProcessId(hwnd, out _)
			.Returns(callInfo =>
			{
				callInfo[1] = _processId;
				return (uint)1;
			});
		internalCtx
			.CoreNativeManager
			.GetProcessNameAndPath((int)_processId)
			.Returns(("chrome.exe", "C:\\Program Files\\Google Chrome\\chrome.exe"));
	}

	private WindowManagerTests Trigger_MouseLeftButtonDown(IInternalContext internalCtx)
	{
		internalCtx.MouseHook.MouseLeftButtonDown += Raise.Event<EventHandler<MouseEventArgs>>(
			new MouseEventArgs(new Point<int>() { X = 1, Y = 2 })
		);
		return this;
	}

	private WindowManagerTests Trigger_MouseLeftButtonUp(IInternalContext internalCtx)
	{
		internalCtx.MouseHook.MouseLeftButtonUp += Raise.Event<EventHandler<MouseEventArgs>>(
			new MouseEventArgs(new Point<int>() { X = 1, Y = 2 })
		);
		return this;
	}

	private WindowManagerTests Setup_GetCursorPos(IInternalContext internalCtx)
	{
		internalCtx
			.CoreNativeManager
			.GetCursorPos(out _)
			.Returns(callInfo =>
			{
				callInfo[0] = new Point<int>(1, 2);
				return (BOOL)true;
			});
		return this;
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void OnWindowAdded(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		WindowManager windowManager = new(ctx, internalCtx);

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowAdded += eh,
			eh => windowManager.WindowAdded -= eh,
			() => windowManager.OnWindowAdded(window)
		);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.Received(1)
			.WindowAdded(window);
		Assert.Equal(window, result.Arguments.Window);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void OnWindowFocused(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		WindowManager windowManager = new(ctx, internalCtx);

		// When
		var result = Assert.Raises<WindowFocusedEventArgs>(
			eh => windowManager.WindowFocused += eh,
			eh => windowManager.WindowFocused -= eh,
			() => windowManager.OnWindowFocused(window)
		);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.Received(1)
			.WindowFocused(window);
		((IInternalMonitorManager)ctx.MonitorManager).Received(1).WindowFocused(window);
		Assert.Equal(window, result.Arguments.Window);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void OnWindowRemoved(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		WindowManager windowManager = new(ctx, internalCtx);

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowRemoved += eh,
			eh => windowManager.WindowRemoved -= eh,
			() => windowManager.OnWindowRemoved(window)
		);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.Received(1)
			.WindowRemoved(window);
		Assert.Equal(window, result.Arguments.Window);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void OnWindowMinimizeStart(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		WindowManager windowManager = new(ctx, internalCtx);

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowMinimizeStart += eh,
			eh => windowManager.WindowMinimizeStart -= eh,
			() => windowManager.OnWindowMinimizeStart(window)
		);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.Received(1)
			.WindowMinimizeStart(window);
		Assert.Equal(window, result.Arguments.Window);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void OnWindowMinimizeEnd(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		WindowManager windowManager = new(ctx, internalCtx);

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowMinimizeEnd += eh,
			eh => windowManager.WindowMinimizeEnd -= eh,
			() => windowManager.OnWindowMinimizeEnd(window)
		);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.Received(1)
			.WindowMinimizeEnd(window);
		Assert.Equal(window, result.Arguments.Window);
	}

	private static void InitializeCoreNativeManagerMock(IInternalContext internalCtx)
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
			internalCtx
				.CoreNativeManager
				.SetWinEventHook(eventMin, eventMax, Arg.Any<WINEVENTPROC>())
				.Returns(new UnhookWinEventSafeHandle(1));
		}
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void Initialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		InitializeCoreNativeManagerMock(internalCtx);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();

		// Then
		internalCtx
			.CoreNativeManager
			.Received(6)
			.SetWinEventHook(Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<WINEVENTPROC>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void Initialize_Fail(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		InitializeCoreNativeManagerMock(internalCtx);

		internalCtx
			.CoreNativeManager
			.SetWinEventHook(
				PInvoke.EVENT_SYSTEM_MINIMIZESTART,
				PInvoke.EVENT_SYSTEM_MINIMIZEEND,
				Arg.Any<WINEVENTPROC>()
			)
			.Returns(new UnhookWinEventSafeHandle());

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		// Then
		Assert.Throws<InvalidOperationException>(windowManager.Initialize);
	}

	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.CHILDID_SELF + 1, 0, 0)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.CHILDID_SELF, 1, 0)]
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.CHILDID_SELF, 0, null)]
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	[Theory]
	internal void WindowsEventHook_IsEventWindowValid_False(
		int idObject,
		int idChild,
		int? hwndValue,
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture
			.WinEventProc!
			.Invoke(
				(HWINEVENTHOOK)0,
				PInvoke.EVENT_OBJECT_SHOW,
				hwndValue == null ? HWND.Null : (HWND)hwndValue,
				idObject,
				idChild,
				0,
				0
			);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.DidNotReceive()
			.WindowAdded(Arg.Any<IWindow>());
	}

	[InlineAutoSubstituteData<WindowManagerCustomization>(true, false, true, true)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(false, true, true, true)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(false, false, false, true)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(false, false, true, false)]
	[Theory]
	internal void WindowsEventHook_AddWindow_Fail(
		bool isSplashScreen,
		bool isCloakedWindow,
		bool isStandardWindow,
		bool hasNoVisibleOwner,
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		internalCtx.CoreNativeManager.IsSplashScreen(hwnd).Returns(isSplashScreen);
		internalCtx.CoreNativeManager.IsCloakedWindow(hwnd).Returns(isCloakedWindow);
		internalCtx.CoreNativeManager.IsStandardWindow(hwnd).Returns(isStandardWindow);
		internalCtx.CoreNativeManager.HasNoVisibleOwner(hwnd).Returns(hasNoVisibleOwner);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then
		internalCtx.CoreNativeManager.DidNotReceive().GetWindowThreadProcessId(hwnd, out _);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_CreateWindow_Null(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		internalCtx
			.CoreNativeManager
			.When(cnm => cnm.GetProcessNameAndPath((int)_processId))
			.Do(_ => throw new Win32Exception());

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.DidNotReceive()
			.WindowAdded(Arg.Any<IWindow>());
		ctx.FilterManager.DidNotReceive().ShouldBeIgnored(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_IgnoreWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.FilterManager.ShouldBeIgnored(Arg.Any<IWindow>()).Returns(true);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.DidNotReceive()
			.WindowAdded(Arg.Any<IWindow>());
		ctx.FilterManager.Received(1).ShouldBeIgnored(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_WindowIsMinimized(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		internalCtx.CoreNativeManager.IsWindowMinimized(hwnd).Returns((BOOL)true);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.DidNotReceive()
			.WindowAdded(Arg.Any<IWindow>());
		ctx.FilterManager.Received(1).ShouldBeIgnored(Arg.Any<IWindow>());
		((IInternalWorkspaceManager)ctx.WorkspaceManager).DidNotReceive().WindowAdded(Arg.Any<IWindow>());
	}

	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_SYSTEM_FOREGROUND)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_OBJECT_UNCLOAKED)]
	[Theory]
	internal void WindowsEventHook_OnWindowFocused(uint eventType, IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowFocusedEventArgs>(
			h => windowManager.WindowFocused += h,
			h => windowManager.WindowFocused -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, eventType, hwnd, 0, 0, 0, 0)
		);

		// Then
		((IInternalMonitorManager)ctx.MonitorManager)
			.Received(1)
			.WindowFocused(Arg.Any<IWindow>());
		((IInternalWorkspaceManager)ctx.WorkspaceManager).Received(1).WindowFocused(Arg.Any<IWindow>());
	}

	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_SYSTEM_FOREGROUND)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_OBJECT_UNCLOAKED)]
	[Theory]
	internal void WindowsEventHook_OnWindowFocused_IgnoredWindow(
		uint eventType,
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.FilterManager.ShouldBeIgnored(Arg.Any<IWindow>()).Returns(true);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();

		// Then
		Assert.Raises<WindowFocusedEventArgs>(
			h => windowManager.WindowFocused += h,
			h => windowManager.WindowFocused -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, eventType, hwnd, 0, 0, 0, 0)
		);

		// Then
		((IInternalMonitorManager)ctx.MonitorManager)
			.Received(1)
			.WindowFocused(Arg.Any<IWindow>());
		((IInternalWorkspaceManager)ctx.WorkspaceManager).Received(1).WindowFocused(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowHidden_IgnoreWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		ctx.WorkspaceManager.GetMonitorForWindow(Arg.Any<IWindow>()).Returns((IMonitor?)null);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_HIDE, hwnd, 0, 0, 0, 0);

		// Then
		ctx.WorkspaceManager.Received(1).GetMonitorForWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowHidden(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowRemoved += h,
			h => windowManager.WindowRemoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_HIDE, hwnd, 0, 0, 0, 0)
		);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.Received(1)
			.WindowRemoved(Arg.Any<IWindow>());
	}

	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_OBJECT_DESTROY)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_OBJECT_CLOAKED)]
	[Theory]
	internal void WindowsEventHook_OnWindowRemoved(uint eventType, IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowRemoved += h,
			h => windowManager.WindowRemoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, eventType, hwnd, 0, 0, 0, 0)
		);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.Received(1)
			.WindowRemoved(Arg.Any<IWindow>());
		Assert.Empty(windowManager.HandleWindowMap);
	}

	#region OnWindowMoveStart
	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoveStart(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);
		internalCtx
			.CoreNativeManager
			.GetCursorPos(out _)
			.Returns(
				(callInfo) =>
				{
					callInfo[0] = null;
					return (BOOL)false;
				}
			);
		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);
		workspace.TryGetWindowLocation(Arg.Any<IWindow>()).Returns((IWindowState?)null);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();

		// Then
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveStart += h,
			h => windowManager.WindowMoveStart -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0)
		);
		Assert.Null(result.Arguments.CursorDraggedPoint);
		Assert.Null(result.Arguments.MovedEdges);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoveStart_GetCursorPos(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);
		windowManager.PostInitialize();

		Trigger_MouseLeftButtonDown(internalCtx).Setup_GetCursorPos(internalCtx);

		// When
		windowManager.Initialize();
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveStart += h,
			h => windowManager.WindowMoveStart -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0)
		);

		// Then
		internalCtx.CoreNativeManager.Received(1).GetCursorPos(out IPoint<int> _);
		Assert.NotNull(result.Arguments.CursorDraggedPoint);
	}

	[Theory]
	[MemberData(nameof(MoveEdgesSuccessData))]
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable xUnit1026 // Theory methods should use all of their parameter
	internal void WindowsEventHook_OnWindowMoveStart_MovedEdges(
		ILocation<int> originalLocation,
		ILocation<int> newLocation,
		Direction _direction,
		IPoint<int> _pixelsDelta
	)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
#pragma warning restore IDE0060 // Remove unused parameter
	{
		// Given
		IContext ctx = Substitute.For<IContext>();
		ctx.WorkspaceManager.Returns(Substitute.For<IWorkspaceManager, IInternalWorkspaceManager>());
		ctx.MonitorManager.Returns(Substitute.For<IMonitorManager, IInternalMonitorManager>());
		IInternalContext internalCtx = Substitute.For<IInternalContext>();
		IWorkspace workspace = Substitute.For<IWorkspace>();

		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowLocation(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Location = originalLocation,
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowLocation(Arg.Any<HWND>()).Returns(newLocation);

		WindowManager windowManager = new(ctx, internalCtx);
		windowManager.PostInitialize();

		Trigger_MouseLeftButtonDown(internalCtx).Setup_GetCursorPos(internalCtx);

		// When
		windowManager.Initialize();
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveStart += h,
			h => windowManager.WindowMoveStart -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0)
		);

		// Then
		internalCtx.CoreNativeManager.Received(1).GetCursorPos(out IPoint<int> _);
		Assert.NotNull(result.Arguments.CursorDraggedPoint);
		Assert.NotNull(result.Arguments.MovedEdges);
	}
	#endregion

	#region OnWindowMoved
	private static (CaptureWinEventProc, WindowManager, HWND) Setup_LocationRestoring(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);
		WindowManager windowManager = new(ctx, internalCtx);
		windowManager.Initialize();
		return (capture, windowManager, hwnd);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoved_DoesNotRaise_ProcessFileNameIsNull(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the window has no process file name
		internalCtx.CoreNativeManager.GetProcessNameAndPath((int)_processId).Returns(("", null));
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_LocationRestoring(
			ctx,
			internalCtx
		);

		// When the window is moved
		// Then no event is raised
		CustomAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoved_DoesNotRaise_WindowDoesNotRestore(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the window is not registered as restoring
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_LocationRestoring(
			ctx,
			internalCtx
		);

		// When the window is moved
		// Then no event is raised
		CustomAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	private static IWorkspace Setup_LocationRestoring_Success(IContext ctx, IInternalContext internalCtx, HWND hwnd)
	{
		IWindow window = Window.CreateWindow(ctx, internalCtx, hwnd)!;

		IWorkspace workspace = Substitute.For<IWorkspace>();
		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		internalCtx
			.CoreNativeManager
			.When(cnm => cnm.TryEnqueue(Arg.Any<DispatcherQueueHandler>()))
			.Do(callInfo =>
			{
				var handler = callInfo.ArgAt<DispatcherQueueHandler>(0);
				handler.Invoke();
			});

		internalCtx
			.CoreNativeManager
			.GetProcessNameAndPath((int)_processId)
			.Returns(("firefox.exe", "C:\\Program Files\\Mozilla Firefox\\firefox.exe"));

		return workspace;
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal async void WindowsEventHook_OnWindowMoved_Raises_CannotFindWorkspaceForWindow(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the window is registered as restoring, but no workspace is found for it
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_LocationRestoring(
			ctx,
			internalCtx
		);
		IWorkspace workspace = Setup_LocationRestoring_Success(ctx, internalCtx, hwnd);
		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns((IWorkspace?)null);

		// When the window is moved
		// Then an event is raised, but the workspace is not asked to do a layout
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
		await Task.Delay(2200);

		Assert.Null(result.Arguments.CursorDraggedPoint);
		Assert.Null(result.Arguments.MovedEdges);
		Assert.NotNull(result.Arguments.Window);
		workspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal async void WindowsEventHook_OnWindowMoved_Raises_DoLayout(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window is registered as restoring, and a workspace is found for it
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_LocationRestoring(
			ctx,
			internalCtx
		);
		IWorkspace workspace = Setup_LocationRestoring_Success(ctx, internalCtx, hwnd);

		// When the window is moved
		// Then an event is raised, and the workspace is asked to do a layout
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
		await Task.Delay(2200);

		Assert.Null(result.Arguments.CursorDraggedPoint);
		Assert.Equal(result.Arguments.MovedEdges, Direction.None);
		Assert.NotNull(result.Arguments.Window);
		workspace.Received(1).DoLayout();
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal async void WindowsEventHook_OnWindowMoved_DoesNotRaise_WindowAlreadyHandled(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the window is registered as restoring, a workspace is found for it, and the window is already handled
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_LocationRestoring(
			ctx,
			internalCtx
		);
		IWorkspace workspace = Setup_LocationRestoring_Success(ctx, internalCtx, hwnd);

		// When the window is moved for the second time
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0);
		await Task.Delay(2200).ConfigureAwait(true);

		// Then no event is raised
		CustomAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
		await Task.Delay(2200).ConfigureAwait(true);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal async void WindowsEventHook_OnWindowMoved_WindowGetsRemoved(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window has been been handled, but is removed
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_LocationRestoring(
			ctx,
			internalCtx
		);
		IWorkspace workspace = Setup_LocationRestoring_Success(ctx, internalCtx, hwnd);

		// When the window is moved and then removed
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0);
		await Task.Delay(2200).ConfigureAwait(true);

		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_DESTROY, hwnd, 0, 0, 0, 0);
		await Task.Delay(2200).ConfigureAwait(true);

		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0);
		await Task.Delay(2200).ConfigureAwait(true);

		// Then the workspace is asked to do two layouts
		workspace.Received(2).DoLayout();
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoved_DoesNotRaise(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();

		// Then
		CustomAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoved_DoesNotRaise_MouseIsUp(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window has not had OnWindowMoveStart called
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When the window is moved
		windowManager.Initialize();
		windowManager.PostInitialize();

		Trigger_MouseLeftButtonDown(internalCtx).Trigger_MouseLeftButtonUp(internalCtx);

		// Then no event is raised
		CustomAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoved_GetCursorPos(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);
		internalCtx.CoreNativeManager.IsWindowMinimized(hwnd).Returns((BOOL)false);

		WindowManager windowManager = new(ctx, internalCtx);
		windowManager.PostInitialize();

		Trigger_MouseLeftButtonDown(internalCtx).Setup_GetCursorPos(internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);

		// Then
		internalCtx.CoreNativeManager.Received(2).GetCursorPos(out IPoint<int> _);
		Assert.NotNull(result.Arguments.CursorDraggedPoint);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoved(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		ILocation<int> originalLocation = new Location<int>() { X = 4, Width = 4 };
		ILocation<int> newLocation = new Location<int>() { X = 3, Width = 5 };
		HWND hwnd = new(1);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowLocation(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Location = originalLocation,
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowLocation(Arg.Any<HWND>()).Returns(newLocation);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);

		// Then
		Assert.Null(result.Arguments.CursorDraggedPoint);
		Assert.NotNull(result.Arguments.MovedEdges);
	}
	#endregion

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMinimizeStart(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowMinimizeStart += h,
			h => windowManager.WindowMinimizeStart -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MINIMIZESTART, hwnd, 0, 0, 0, 0)
		);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.Received(1)
			.WindowMinimizeStart(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMinimizeEnd(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowMinimizeEnd += h,
			h => windowManager.WindowMinimizeEnd -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MINIMIZEEND, hwnd, 0, 0, 0, 0)
		);

		// Then
		((IInternalWorkspaceManager)ctx.WorkspaceManager)
			.Received(1)
			.WindowMinimizeEnd(Arg.Any<IWindow>());
	}

	#region OnWindowMoveEnd
	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoveEnd_WindowNotMoving(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		ctx.WorkspaceManager.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
		ctx.WorkspaceManager.DidNotReceive().MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoveEnd_GetMovedEdges_NoWorkspace(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);
		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns((IWorkspace?)null);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		ctx.WorkspaceManager.Received(1).GetWorkspaceForWindow(Arg.Any<IWindow>());
		ctx.NativeManager.DidNotReceive().DwmGetWindowLocation(Arg.Any<HWND>());
		ctx.WorkspaceManager.DidNotReceive().MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoveEnd_GetMovedEdges_DoesNotContainWindowState(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);
		workspace.TryGetWindowLocation(Arg.Any<IWindow>()).Returns((IWindowState?)null);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		var result = Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveEnd += h,
			h => windowManager.WindowMoveEnd -= h,
			() =>
			{
				capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
				capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);
			}
		);

		// Then
		workspace.Received(1).TryGetWindowLocation(Arg.Any<IWindow>());
		ctx.NativeManager.DidNotReceive().DwmGetWindowLocation(Arg.Any<HWND>());
		internalCtx.CoreNativeManager.Received(1).GetCursorPos(out IPoint<int> _);
		ctx.WorkspaceManager.DidNotReceive().MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
		Assert.Null(result.Arguments.CursorDraggedPoint);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoveEnd_MoveWindowToPoint(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);
		workspace.TryGetWindowLocation(Arg.Any<IWindow>()).Returns((IWindowState?)null);
		internalCtx.CoreNativeManager.GetCursorPos(out IPoint<int> _).Returns((BOOL)true);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveEnd += h,
			h => windowManager.WindowMoveEnd -= h,
			() =>
			{
				windowManager.Initialize();
				capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
				capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);
			}
		);

		// Then
		workspace.Received(1).TryGetWindowLocation(Arg.Any<IWindow>());
		internalCtx.CoreNativeManager.Received(1).GetCursorPos(out IPoint<int> _);
		ctx.WorkspaceManager.Received(1).MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
	}
	#endregion

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoved_GetMovedEdges_CannotGetNewWindowLocation(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowLocation(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Location = new Location<int>(),
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowLocation(Arg.Any<HWND>()).Returns((Location<int>?)null);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		ctx.NativeManager.Received(1).DwmGetWindowLocation(Arg.Any<HWND>());
	}

	[InlineAutoSubstituteData<WindowManagerCustomization>(1, 0, 1, 0)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(0, 1, 1, 0)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(1, 1, 1, 1)]
	[Theory]
	internal void WindowsEventHook_OnWindowMoveEnd_GetMovedEdges_MoveTooManyEdges(
		int newX,
		int newY,
		int newWidth,
		int newHeight,
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace
	)
	{
		// Given
		Location<int> originalLocation = new();
		Location<int> newLocation =
			new()
			{
				X = newX,
				Y = newY,
				Width = newWidth,
				Height = newHeight
			};
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowLocation(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Location = originalLocation,
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowLocation(Arg.Any<HWND>()).Returns(newLocation);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		ctx.NativeManager.Received(1).DwmGetWindowLocation(Arg.Any<HWND>());
		workspace
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow>());
	}

	public static IEnumerable<object[]> MoveEdgesSuccessData()
	{
		// Move left edge to the left
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
	internal void WindowsEventHook_OnWindowMoveEnd_Success(
		ILocation<int> originalLocation,
		ILocation<int> newLocation,
		Direction direction,
		IPoint<int> pixelsDelta
	)
	{
		// Given
		IContext ctx = Substitute.For<IContext>();
		ctx.WorkspaceManager.Returns(Substitute.For<IWorkspaceManager, IInternalWorkspaceManager>());
		ctx.MonitorManager.Returns(Substitute.For<IMonitorManager, IInternalMonitorManager>());
		IInternalContext internalCtx = Substitute.For<IInternalContext>();
		IWorkspace workspace = Substitute.For<IWorkspace>();

		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowLocation(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Location = originalLocation,
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowLocation(Arg.Any<HWND>()).Returns(newLocation);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);

		// Then
		Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoveEnd += h,
			h => windowManager.WindowMoveEnd -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0)
		);
		ctx.WorkspaceManager.Received().MoveWindowEdgesInDirection(direction, pixelsDelta, Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_OnWindowMoveEnd_GetCursorPos(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.WorkspaceManager.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowLocation(Arg.Any<IWindow>())
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
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager
			.DwmGetWindowLocation(Arg.Any<HWND>())
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
		internalCtx.CoreNativeManager.GetCursorPos(out point).Returns((BOOL)true);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);

		// Then
		Assert.Raises<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WindowsEventHook_InvalidEvent(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, 0xBAADF00D, hwnd, 0, 0, 0, 0);

		// Then
		ctx.WorkspaceManager.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
		(ctx.WorkspaceManager as IInternalWorkspaceManager)!.Received(1).WindowAdded(Arg.Any<IWindow>());
		(ctx.WorkspaceManager as IInternalWorkspaceManager)!.DidNotReceive().WindowRemoved(Arg.Any<IWindow>());
		(ctx.WorkspaceManager as IInternalWorkspaceManager)!.DidNotReceive().WindowMinimizeStart(Arg.Any<IWindow>());
		(ctx.WorkspaceManager as IInternalWorkspaceManager)!.DidNotReceive().WindowMinimizeEnd(Arg.Any<IWindow>());
		ctx.WorkspaceManager.DidNotReceive().MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
		ctx.WorkspaceManager
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<int>>(), Arg.Any<IWindow>());
		ctx.WorkspaceManager.DidNotReceive().MoveWindowToWorkspace(Arg.Any<IWorkspace>(), Arg.Any<IWindow>());
	}

	[Theory]
	[InlineAutoSubstituteData<WindowManagerCustomization>(false)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(true)]
	internal void PostInitialize(bool routeToActiveWorkspace, IContext ctx, IInternalContext internalCtx)
	{
		// Given
		ctx.RouterManager.RouteToActiveWorkspace.Returns(routeToActiveWorkspace);
		internalCtx.CoreNativeManager.GetAllWindows().Returns(new List<HWND>() { new(1), new(2), new(3) });

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.PostInitialize();

		// Then RouteToActiveWorkspace was get and set
		internalCtx.CoreNativeManager.Received(3).IsSplashScreen(Arg.Any<HWND>());
		_ = ctx.RouterManager.Received(1).RouteToActiveWorkspace;
		ctx.RouterManager.Received().RouteToActiveWorkspace = routeToActiveWorkspace;
	}

	#region Dispose
	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void Dispose_NotInitialized(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Dispose();

		// Then
		Assert.Empty(capture.Handles);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void Dispose_IsClosed(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx, false, true);
		WindowManager windowManager = new(ctx, internalCtx);

		windowManager.Initialize();

		capture.Handles.ForEach(h => h.HasDisposed = false);

		// When
		windowManager.Dispose();

		// Then
		capture.Handles.Should().OnlyContain(h => !h.HasDisposed);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void Dispose_IsInvalid(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx, false, true);
		WindowManager windowManager = new(ctx, internalCtx);

		windowManager.Initialize();

		capture
			.Handles
			.ForEach(h =>
			{
				h.HasDisposed = false;
				h.MarkAsInvalid();
			});

		// When
		windowManager.Dispose();

		// Then
		capture.Handles.Should().OnlyContain(h => !h.HasDisposed);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void Dispose_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		WindowManager windowManager = new(ctx, internalCtx);

		windowManager.Initialize();

		// When
		windowManager.Dispose();
		windowManager.Dispose();

		// Then
		capture.Handles.Should().OnlyContain(h => h.HasDisposed);
	}
	#endregion


	#region GetEnumerator
	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void GetEnumerator(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// When
		IWindow[] windows = windowManager.ToArray();

		// Then
		Assert.Single(windows);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void IEnumerable_GetEnumerator(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// When
		IEnumerator enumerator = ((IEnumerable)windowManager).GetEnumerator();
		List<IWindow> windows = new();
		while (enumerator.MoveNext())
		{
			windows.Add((IWindow)enumerator.Current);
		}

		// Then
		Assert.Single(windows);
	}
	#endregion
}
