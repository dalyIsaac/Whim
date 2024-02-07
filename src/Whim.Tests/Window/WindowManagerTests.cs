using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
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
		IWorkspaceManager workspaceManager = Substitute.For<IWorkspaceManager>();
		IMonitorManager monitorManager = Substitute.For<IMonitorManager, IInternalMonitorManager>();

		IContext context = fixture.Freeze<IContext>();
		context.WorkspaceManager.Returns(workspaceManager);
		context.MonitorManager.Returns(monitorManager);

		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();
		internalCtx.MonitorManager.Returns((IInternalMonitorManager)monitorManager);
	}
}

internal class WindowManagerSubscriber
{
	public WindowEventArgs? WindowAddedArgs { get; private set; }
	public WindowFocusedEventArgs? WindowFocusedArgs { get; private set; }
	public WindowEventArgs? WindowRemovedArgs { get; private set; }
	public WindowEventArgs? WindowMoveStartArgs { get; private set; }
	public WindowEventArgs? WindowMovedArgs { get; private set; }
	public WindowEventArgs? WindowMoveEndArgs { get; private set; }
	public WindowEventArgs? WindowMinimizeStartArgs { get; private set; }
	public WindowEventArgs? WindowMinimizeEndArgs { get; private set; }

	public WindowManagerSubscriber(WindowManager windowManager)
	{
		windowManager.WindowAdded += (sender, args) => WindowAddedArgs = args;
		windowManager.WindowFocused += (sender, args) => WindowFocusedArgs = args;
		windowManager.WindowRemoved += (sender, args) => WindowRemovedArgs = args;
		windowManager.WindowMoveStart += (sender, args) => WindowMoveStartArgs = args;
		windowManager.WindowMoved += (sender, args) => WindowMovedArgs = args;
		windowManager.WindowMoveEnd += (sender, args) => WindowMoveEndArgs = args;
		windowManager.WindowMinimizeStart += (sender, args) => WindowMinimizeStartArgs = args;
		windowManager.WindowMinimizeEnd += (sender, args) => WindowMinimizeEndArgs = args;
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
			.CoreNativeManager.SetWinEventHook(Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<WINEVENTPROC>())
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
			.CoreNativeManager.GetWindowThreadProcessId(hwnd, out _)
			.Returns(callInfo =>
			{
				callInfo[1] = _processId;
				return (uint)1;
			});
		internalCtx
			.CoreNativeManager.GetProcessNameAndPath((int)_processId)
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
			.CoreNativeManager.GetCursorPos(out _)
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

		// Then
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowAdded += eh,
			eh => windowManager.WindowAdded -= eh,
			() => windowManager.OnWindowAdded(window)
		);
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
		internalCtx.MonitorManager.Received(1).OnWindowFocused(window);
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
		Assert.Equal(window, result.Arguments.Window);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void OnWindowMinimizeStart(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		WindowManager windowManager = new(ctx, internalCtx);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		windowManager.Initialize();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowMinimizeStart += eh,
			eh => windowManager.WindowMinimizeStart -= eh,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MINIMIZESTART, hwnd, 0, 0, 0, 0)
		);

		// Then
		Assert.Equal((int)_processId, result.Arguments.Window.ProcessId);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void OnWindowMinimizeEnd(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		WindowManager windowManager = new(ctx, internalCtx);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		windowManager.Initialize();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowMinimizeEnd += eh,
			eh => windowManager.WindowMinimizeEnd -= eh,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MINIMIZEEND, hwnd, 0, 0, 0, 0)
		);

		// Then
		Assert.Equal((int)_processId, result.Arguments.Window.ProcessId);
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
				.CoreNativeManager.SetWinEventHook(eventMin, eventMax, Arg.Any<WINEVENTPROC>())
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
			.CoreNativeManager.Received(6)
			.SetWinEventHook(Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<WINEVENTPROC>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void Initialize_Fail(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		InitializeCoreNativeManagerMock(internalCtx);

		internalCtx
			.CoreNativeManager.SetWinEventHook(
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
	internal void WinEventProc_IsEventWindowValid_False(
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

		// Then
		CustomAssert.DoesNotRaise<WindowEventArgs>(
			h => windowManager.WindowAdded += h,
			h => windowManager.WindowAdded -= h,
			() =>
				capture.WinEventProc!.Invoke(
					(HWINEVENTHOOK)0,
					PInvoke.EVENT_OBJECT_SHOW,
					hwndValue == null ? HWND.Null : (HWND)hwndValue,
					idObject,
					idChild,
					0,
					0
				)
		);
	}

	#region AddWindow
	[InlineAutoSubstituteData<WindowManagerCustomization>(true, false, true, true)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(false, true, true, true)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(false, false, false, true)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(false, false, true, false)]
	[Theory]
	internal void WinEventProc_AddWindow_Fail(
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
	internal void WinEventProc_AddWindow_CreateWindowNull(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		internalCtx
			.CoreNativeManager.When(cnm => cnm.GetProcessNameAndPath((int)_processId))
			.Do(_ => throw new Win32Exception());

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		CustomAssert.DoesNotRaise<WindowEventArgs>(
			h => windowManager.WindowAdded += h,
			h => windowManager.WindowAdded -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0)
		);

		// Then
		ctx.FilterManager.DidNotReceive().ShouldBeIgnored(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_AddWindow_IgnoreWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.FilterManager.ShouldBeIgnored(Arg.Any<IWindow>()).Returns(true);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();

		CustomAssert.DoesNotRaise<WindowEventArgs>(
			h => windowManager.WindowAdded += h,
			h => windowManager.WindowAdded -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0)
		);

		// Then
		ctx.FilterManager.Received(1).ShouldBeIgnored(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_AddWindow_WindowIsMinimized(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		internalCtx.CoreNativeManager.IsWindowMinimized(hwnd).Returns((BOOL)true);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowAdded += h,
			h => windowManager.WindowAdded -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0)
		);

		// Then
		ctx.FilterManager.Received(1).ShouldBeIgnored(Arg.Any<IWindow>());
	}
	#endregion

	#region MonitorsAreChanging
	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_MonitorsAreChanging_NewWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		internalCtx.CoreNativeManager.IsWindowMinimized(hwnd).Returns((BOOL)true);

		WindowManager windowManager = new(ctx, internalCtx);
		internalCtx.ButlerEventHandlers.AreMonitorsChanging.Returns(true);

		// When
		windowManager.Initialize();
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowAdded += h,
			h => windowManager.WindowAdded -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_FOREGROUND, hwnd, 0, 0, 0, 0)
		);

		// Then
		ctx.FilterManager.Received(1).ShouldBeIgnored(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_MonitorsAreNotChanging_OldWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		internalCtx.CoreNativeManager.IsWindowMinimized(hwnd).Returns((BOOL)true);

		WindowManager windowManager = new(ctx, internalCtx);
		internalCtx.ButlerEventHandlers.AreMonitorsChanging.Returns(false);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_FOREGROUND, hwnd, 0, 0, 0, 0);
		CustomAssert.DoesNotRaise<WindowEventArgs>(
			h => windowManager.WindowAdded += h,
			h => windowManager.WindowAdded -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_FOREGROUND, hwnd, 0, 0, 0, 0)
		);

		// Then
		ctx.FilterManager.Received(1).ShouldBeIgnored(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_MonitorsAreChanging_OldWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		internalCtx.CoreNativeManager.IsWindowMinimized(hwnd).Returns((BOOL)true);

		WindowManager windowManager = new(ctx, internalCtx);
		internalCtx.ButlerEventHandlers.AreMonitorsChanging.Returns(true);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_FOREGROUND, hwnd, 0, 0, 0, 0);
		CustomAssert.DoesNotRaise<WindowEventArgs>(
			h => windowManager.WindowAdded += h,
			h => windowManager.WindowAdded -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_FOREGROUND, hwnd, 0, 0, 0, 0)
		);

		// Then
		ctx.FilterManager.Received(1).ShouldBeIgnored(Arg.Any<IWindow>());
	}
	#endregion

	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_SYSTEM_FOREGROUND)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_OBJECT_UNCLOAKED)]
	[Theory]
	internal void WinEventProc_OnWindowFocused(uint eventType, IContext ctx, IInternalContext internalCtx)
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
		internalCtx.MonitorManager.Received(1).OnWindowFocused(Arg.Any<IWindow>());
	}

	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_SYSTEM_FOREGROUND)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_OBJECT_UNCLOAKED)]
	[Theory]
	internal void WinEventProc_OnWindowFocused_IgnoredWindow(uint eventType, IContext ctx, IInternalContext internalCtx)
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
		internalCtx.MonitorManager.Received(1).OnWindowFocused(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowHidden_IgnoreWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		ctx.Butler.GetMonitorForWindow(Arg.Any<IWindow>()).Returns((IMonitor?)null);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_HIDE, hwnd, 0, 0, 0, 0);

		// Then
		ctx.Butler.Received(1).GetMonitorForWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowHidden(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();

		// Then
		Assert.Raises<WindowEventArgs>(
			h => windowManager.WindowRemoved += h,
			h => windowManager.WindowRemoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_HIDE, hwnd, 0, 0, 0, 0)
		);
	}

	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_OBJECT_DESTROY)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(PInvoke.EVENT_OBJECT_CLOAKED)]
	[Theory]
	internal void WinEventProc_OnWindowRemoved(uint eventType, IContext ctx, IInternalContext internalCtx)
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
		Assert.Empty(windowManager.HandleWindowMap);
	}

	#region OnWindowMoveStart
	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowMoveStart(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);
		internalCtx
			.CoreNativeManager.GetCursorPos(out _)
			.Returns(
				(callInfo) =>
				{
					callInfo[0] = null;
					return (BOOL)false;
				}
			);
		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);
		workspace.TryGetWindowState(Arg.Any<IWindow>()).Returns((IWindowState?)null);

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
	internal void WinEventProc_OnWindowMoveStart_GetCursorPos(IContext ctx, IInternalContext internalCtx)
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
#pragma warning disable xUnit1026 // Theory methods should use all of their parameter
	internal void WinEventProc_OnWindowMoveStart_MovedEdges(
		IRectangle<int> originalRect,
		IRectangle<int> newRect,
		Direction _direction,
		IPoint<int> _pixelsDelta
	)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
	{
		// Given
		IContext ctx = Substitute.For<IContext>();
		ctx.WorkspaceManager.Returns(Substitute.For<IWorkspaceManager>());
		ctx.MonitorManager.Returns(Substitute.For<IMonitorManager, IInternalMonitorManager>());
		IInternalContext internalCtx = Substitute.For<IInternalContext>();
		IWorkspace workspace = Substitute.For<IWorkspace>();

		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowState(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Rectangle = originalRect,
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns(newRect);

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
	private static (CaptureWinEventProc, WindowManager, HWND) Setup_RectRestoring(
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
	internal void WinEventProc_OnWindowMoved_DoesNotRaise_ProcessFileNameIsNull(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the window has no process file name
		internalCtx.CoreNativeManager.GetProcessNameAndPath((int)_processId).Returns(("", null));
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_RectRestoring(ctx, internalCtx);

		// When the window is moved
		// Then no event is raised
		CustomAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowMoved_DoesNotRaise_WindowDoesNotRestore(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the window is not registered as restoring
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_RectRestoring(ctx, internalCtx);

		// When the window is moved
		// Then no event is raised
		CustomAssert.DoesNotRaise<WindowMovedEventArgs>(
			h => windowManager.WindowMoved += h,
			h => windowManager.WindowMoved -= h,
			() => capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_LOCATIONCHANGE, hwnd, 0, 0, 0, 0)
		);
	}

	private static IWorkspace Setup_RectRestoring_Success(IContext ctx, IInternalContext internalCtx, HWND hwnd)
	{
		Window.CreateWindow(ctx, internalCtx, hwnd);

		IWorkspace workspace = Substitute.For<IWorkspace>();
		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);
		NativeManagerUtils.SetupTryEnqueue(ctx);

		internalCtx
			.CoreNativeManager.GetProcessNameAndPath((int)_processId)
			.Returns(("firefox.exe", "C:\\Program Files\\Mozilla Firefox\\firefox.exe"));

		return workspace;
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal async void WinEventProc_OnWindowMoved_Raises_CannotFindWorkspaceForWindow(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the window is registered as restoring, but no workspace is found for it
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_RectRestoring(ctx, internalCtx);
		IWorkspace workspace = Setup_RectRestoring_Success(ctx, internalCtx, hwnd);
		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns((IWorkspace?)null);

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
	internal async void WinEventProc_OnWindowMoved_Raises_DoLayout(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window is registered as restoring, and a workspace is found for it
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_RectRestoring(ctx, internalCtx);
		IWorkspace workspace = Setup_RectRestoring_Success(ctx, internalCtx, hwnd);

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
	internal async void WinEventProc_OnWindowMoved_DoesNotRaise_WindowAlreadyHandled(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the window is registered as restoring, a workspace is found for it, and the window is already handled
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_RectRestoring(ctx, internalCtx);
		IWorkspace workspace = Setup_RectRestoring_Success(ctx, internalCtx, hwnd);

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
	internal async void WinEventProc_OnWindowMoved_WindowGetsRemoved(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window has been been handled, but is removed
		(CaptureWinEventProc capture, WindowManager windowManager, HWND hwnd) = Setup_RectRestoring(ctx, internalCtx);
		IWorkspace workspace = Setup_RectRestoring_Success(ctx, internalCtx, hwnd);

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
	internal void WinEventProc_OnWindowMoved_DoesNotRaise(IContext ctx, IInternalContext internalCtx)
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
	internal void WinEventProc_OnWindowMoved_DoesNotRaise_MouseIsUp(IContext ctx, IInternalContext internalCtx)
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
	internal void WinEventProc_OnWindowMoved_GetCursorPos(IContext ctx, IInternalContext internalCtx)
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
	internal void WinEventProc_OnWindowMoved(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		IRectangle<int> originalRect = new Rectangle<int>() { X = 4, Width = 4 };
		IRectangle<int> newRect = new Rectangle<int>() { X = 3, Width = 5 };
		HWND hwnd = new(1);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowState(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Rectangle = originalRect,
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns(newRect);

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
	internal void WinEventProc_OnWindowMinimizeStart(IContext ctx, IInternalContext internalCtx)
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
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowMinimizeEnd(IContext ctx, IInternalContext internalCtx)
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
	}

	#region OnWindowMoveEnd
	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowMoveEnd_WindowNotMoving(IContext ctx, IInternalContext internalCtx)
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
		ctx.Butler.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
		ctx.Butler.DidNotReceive().MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowMoveEnd_GetMovedEdges_NoWorkspace(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);
		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns((IWorkspace?)null);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		ctx.Butler.Received(1).GetWorkspaceForWindow(Arg.Any<IWindow>());
		ctx.NativeManager.DidNotReceive().DwmGetWindowRectangle(Arg.Any<HWND>());
		ctx.Butler.DidNotReceive().MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowMoveEnd_GetMovedEdges_DoesNotContainWindowState(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);
		workspace.TryGetWindowState(Arg.Any<IWindow>()).Returns((IWindowState?)null);

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
		workspace.Received(1).TryGetWindowState(Arg.Any<IWindow>());
		ctx.NativeManager.DidNotReceive().DwmGetWindowRectangle(Arg.Any<HWND>());
		internalCtx.CoreNativeManager.Received(1).GetCursorPos(out IPoint<int> _);
		ctx.Butler.DidNotReceive().MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
		Assert.Null(result.Arguments.CursorDraggedPoint);
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowMoveEnd_MoveWindowToPoint(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);
		workspace.TryGetWindowState(Arg.Any<IWindow>()).Returns((IWindowState?)null);
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
		workspace.Received(1).TryGetWindowState(Arg.Any<IWindow>());
		internalCtx.CoreNativeManager.Received(1).GetCursorPos(out IPoint<int> _);
		ctx.Butler.Received(1).MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
	}
	#endregion

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowMoved_GetMovedEdges_CannotGetNewWindowRect(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowState(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Rectangle = new Rectangle<int>(),
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns((Rectangle<int>?)null);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		ctx.NativeManager.Received(1).DwmGetWindowRectangle(Arg.Any<HWND>());
	}

	[InlineAutoSubstituteData<WindowManagerCustomization>(1, 0, 1, 0)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(0, 1, 1, 0)]
	[InlineAutoSubstituteData<WindowManagerCustomization>(1, 1, 1, 1)]
	[Theory]
	internal void WinEventProc_OnWindowMoveEnd_GetMovedEdges_MoveTooManyEdges(
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
		Rectangle<int> originalRect = new();
		Rectangle<int> newRect =
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

		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowState(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Rectangle = originalRect,
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns(newRect);

		WindowManager windowManager = new(ctx, internalCtx);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZESTART, hwnd, 0, 0, 0, 0);
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MOVESIZEEND, hwnd, 0, 0, 0, 0);

		// Then
		ctx.NativeManager.Received(1).DwmGetWindowRectangle(Arg.Any<HWND>());
		workspace
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow>());
	}

	public static IEnumerable<object[]> MoveEdgesSuccessData()
	{
		// Move left edge to the left
		yield return new object[]
		{
			new Rectangle<int>() { X = 4, Width = 4 },
			new Rectangle<int>() { X = 3, Width = 5 },
			Direction.Left,
			new Point<int>() { X = -1, Y = 0 }
		};

		// Move left edge to the right
		yield return new object[]
		{
			new Rectangle<int>() { X = 4, Width = 4 },
			new Rectangle<int>() { X = 5, Width = 3 },
			Direction.Left,
			new Point<int>() { X = 1, Y = 0 }
		};

		// Move right edge to the right
		yield return new object[]
		{
			new Rectangle<int>() { X = 4, Width = 4 },
			new Rectangle<int>() { X = 4, Width = 5 },
			Direction.Right,
			new Point<int>() { X = 1, Y = 0 }
		};

		// Move right edge to the left
		yield return new object[]
		{
			new Rectangle<int>() { X = 4, Width = 4 },
			new Rectangle<int>() { X = 4, Width = 3 },
			Direction.Right,
			new Point<int>() { X = -1, Y = 0 }
		};

		// Move top edge up
		yield return new object[]
		{
			new Rectangle<int>() { Y = 4, Height = 4 },
			new Rectangle<int>() { Y = 3, Height = 5 },
			Direction.Up,
			new Point<int>() { X = 0, Y = -1 }
		};

		// Move top edge down
		yield return new object[]
		{
			new Rectangle<int>() { Y = 4, Height = 4 },
			new Rectangle<int>() { Y = 5, Height = 3 },
			Direction.Up,
			new Point<int>() { X = 0, Y = 1 }
		};

		// Move bottom edge down
		yield return new object[]
		{
			new Rectangle<int>() { Y = 4, Height = 4 },
			new Rectangle<int>() { Y = 4, Height = 5 },
			Direction.Down,
			new Point<int>() { X = 0, Y = 1 }
		};

		// Move bottom edge up
		yield return new object[]
		{
			new Rectangle<int>() { Y = 4, Height = 4 },
			new Rectangle<int>() { Y = 4, Height = 3 },
			Direction.Down,
			new Point<int>() { X = 0, Y = -1 }
		};
	}

	[Theory]
	[MemberData(nameof(MoveEdgesSuccessData))]
	internal void WinEventProc_OnWindowMoveEnd_Success(
		IRectangle<int> originalRect,
		IRectangle<int> newRect,
		Direction direction,
		IPoint<int> pixelsDelta
	)
	{
		// Given
		IContext ctx = Substitute.For<IContext>();
		ctx.MonitorManager.Returns(Substitute.For<IMonitorManager, IInternalMonitorManager>());
		IInternalContext internalCtx = Substitute.For<IInternalContext>();
		IWorkspace workspace = Substitute.For<IWorkspace>();

		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowState(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Rectangle = originalRect,
					WindowSize = WindowSize.Normal,
					Window = Substitute.For<IWindow>()
				}
			);

		ctx.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns(newRect);

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
		ctx.Butler.Received().MoveWindowEdgesInDirection(direction, pixelsDelta, Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void WinEventProc_OnWindowMoveEnd_GetCursorPos(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace
	)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		ctx.Butler.GetWorkspaceForWindow(Arg.Any<IWindow>()).Returns(workspace);

		workspace
			.TryGetWindowState(Arg.Any<IWindow>())
			.Returns(
				new WindowState()
				{
					Rectangle = new Rectangle<int>()
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

		ctx.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>())
			.Returns(
				new Rectangle<int>()
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
	internal void WinEventProc_InvalidEvent(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);
		WindowManagerSubscriber subscriber = new(windowManager);

		// When
		windowManager.Initialize();
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, 0xBAADF00D, hwnd, 0, 0, 0, 0);

		// Then
		ctx.Butler.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
		Assert.NotNull(subscriber.WindowAddedArgs);
		Assert.Null(subscriber.WindowFocusedArgs);
		Assert.Null(subscriber.WindowRemovedArgs);
		Assert.Null(subscriber.WindowMoveStartArgs);
		Assert.Null(subscriber.WindowMovedArgs);
		Assert.Null(subscriber.WindowMoveEndArgs);
		Assert.Null(subscriber.WindowMinimizeStartArgs);
		Assert.Null(subscriber.WindowMinimizeEndArgs);
		ctx.Butler.DidNotReceive().MoveWindowToPoint(Arg.Any<IWindow>(), Arg.Any<IPoint<int>>());
		ctx.Butler.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<int>>(), Arg.Any<IWindow>());
		ctx.Butler.DidNotReceive().MoveWindowToWorkspace(Arg.Any<IWorkspace>(), Arg.Any<IWindow>());
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

		capture.Handles.ForEach(h =>
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

	[Theory, AutoSubstituteData<WindowManagerCustomization>]
	internal void HandleException(IContext ctx, IInternalContext internalCtx)
	{
		// Given an exception is thrown
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		AllowWindowCreation(ctx, internalCtx, hwnd);

		WindowManager windowManager = new(ctx, internalCtx);
		windowManager.Initialize();

		// When
		internalCtx
			.CoreNativeManager.WhenForAnyArgs(x => x.GetWindowThreadProcessId(hwnd, out uint _))
			.Do(x => throw new Exception());
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);
		IWindow[] windows = windowManager.ToArray();

		// Then
		Assert.Empty(windows);
	}
}
