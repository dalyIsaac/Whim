using System;
using System.Collections.Generic;
using DotNext;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Whim.TestUtils;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Xunit;

namespace Whim.Tests;

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

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WindowEventListenerTests
{
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

	private static void Setup_EmptyWindowSlice(IContext ctx, IInternalContext internalCtx) =>
		ctx.Store.Pick(Arg.Any<TryGetWindowPicker>()).Returns(Result.FromException<IWindow>(new WhimException("welp")));

	private static IWindow Setup_FilledWindowSlice(IContext ctx, IInternalContext internalCtx)
	{
		IWindow window = Substitute.For<IWindow>();
		window.Handle.Returns((HWND)1);

		ctx.Store.Pick(Arg.Any<TryGetWindowPicker>()).Returns(Result.FromValue(window));

		return window;
	}

	private static void AssertDispatches(
		IContext ctx,
		int windowAddedTransformCount = 0,
		int windowFocusedTransformCount = 0,
		int windowHiddenTransformCount = 0,
		int windowRemovedTransformCount = 0,
		int windowMoveStartedTransformCount = 0,
		int windowMoveEndedTransformCount = 0,
		int windowMovedTransformCount = 0,
		int windowMinimizeStartedTransformCount = 0,
		int windowMinimizeEndedTransformCount = 0
	)
	{
		ctx.Store.Received(windowAddedTransformCount).Dispatch(Arg.Any<WindowAddedTransform>());
		ctx.Store.Received(windowFocusedTransformCount).Dispatch(Arg.Any<WindowFocusedTransform>());
		ctx.Store.Received(windowHiddenTransformCount).Dispatch(Arg.Any<WindowHiddenTransform>());
		ctx.Store.Received(windowRemovedTransformCount).Dispatch(Arg.Any<WindowRemovedTransform>());
		ctx.Store.Received(windowMoveStartedTransformCount).Dispatch(Arg.Any<WindowMoveStartedTransform>());
		ctx.Store.Received(windowMoveEndedTransformCount).Dispatch(Arg.Any<WindowMoveEndedTransform>());
		ctx.Store.Received(windowMovedTransformCount).Dispatch(Arg.Any<WindowMovedTransform>());
		ctx.Store.Received(windowMinimizeStartedTransformCount).Dispatch(Arg.Any<WindowMinimizeStartedTransform>());
		ctx.Store.Received(windowMinimizeEndedTransformCount).Dispatch(Arg.Any<WindowMinimizeEndedTransform>());
	}

	[Theory, AutoSubstituteData]
	internal void Initialize_Fail(IContext ctx, IInternalContext internalCtx)
	{
		// Given one hook doesn't return a safe value
		InitializeCoreNativeManagerMock(internalCtx);

		internalCtx
			.CoreNativeManager.SetWinEventHook(
				PInvoke.EVENT_SYSTEM_MINIMIZESTART,
				PInvoke.EVENT_SYSTEM_MINIMIZEEND,
				Arg.Any<WINEVENTPROC>()
			)
			.Returns(new UnhookWinEventSafeHandle());

		WindowEventListener sut = new(ctx, internalCtx);

		// When we initialize the event listener
		// Then it throws
		Assert.Throws<InvalidOperationException>(sut.Initialize);
	}

	[Theory, AutoSubstituteData]
	internal void Initialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given all hooks succeed
		InitializeCoreNativeManagerMock(internalCtx);
		WindowEventListener sut = new(ctx, internalCtx);

		// When we initialize the event listener
		sut.Initialize();

		// Then the core native manager received the expected number of calls
		internalCtx
			.CoreNativeManager.Received(6)
			.SetWinEventHook(Arg.Any<uint>(), Arg.Any<uint>(), Arg.Any<WINEVENTPROC>());
	}

	[InlineAutoSubstituteData(PInvoke.CHILDID_SELF + 1, 0, 0)]
	[InlineAutoSubstituteData(PInvoke.CHILDID_SELF, 1, 0)]
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
	[InlineAutoSubstituteData(PInvoke.CHILDID_SELF, 0, null)]
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	[Theory, AutoSubstituteData]
	internal void IsEventWindowValid_Fail(
		int idObject,
		int idChild,
		int? hwndValue,
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);

		WindowEventListener sut = new(ctx, internalCtx);
		sut.Initialize();

		// When we invoke the WinEventProc with the given params
		capture.WinEventProc!.Invoke(
			(HWINEVENTHOOK)0,
			PInvoke.EVENT_OBJECT_SHOW,
			hwndValue == null ? HWND.Null : (HWND)hwndValue,
			idObject,
			idChild,
			0,
			0
		);

		// Then no events were dispatched
		AssertDispatches(ctx);
	}

	[Theory, AutoSubstituteData]
	internal void CreateWindow_Fail(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window doesn't exist, and we can't create a window
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		Setup_EmptyWindowSlice(ctx, internalCtx);

		ctx.Store.Dispatch(Arg.Any<WindowAddedTransform>())
			.Returns(Result.FromException<IWindow>(new WhimException("welp")));

		WindowEventListener sut = new(ctx, internalCtx);
		sut.Initialize();

		// When we create a window
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then we don't receive any further dispatches
		AssertDispatches(ctx, windowAddedTransformCount: 1);
	}

	[Theory, AutoSubstituteData]
	internal void CreateWindow_ShowWindow(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given the window doesn't exist and we can create a window
		HWND hwnd = new(1);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);
		Setup_EmptyWindowSlice(ctx, internalCtx);

		ctx.Store.Dispatch(Arg.Any<WindowAddedTransform>()).Returns(Result.FromValue(window));

		WindowEventListener sut = new(ctx, internalCtx);
		sut.Initialize();

		// When we create a window
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_OBJECT_SHOW, hwnd, 0, 0, 0, 0);

		// Then we don't receive any further dispatches
		AssertDispatches(ctx, windowAddedTransformCount: 1);
	}

	public static IEnumerable<object[]> WinEventProcCasesData()
	{
		yield return new object[]
		{
			PInvoke.EVENT_SYSTEM_FOREGROUND,
			new Func<IWindow, Transform>(window => new WindowFocusedTransform(window))
		};
		yield return new object[]
		{
			PInvoke.EVENT_OBJECT_UNCLOAKED,
			new Func<IWindow, Transform>(window => new WindowFocusedTransform(window))
		};

		yield return new object[]
		{
			PInvoke.EVENT_OBJECT_HIDE,
			new Func<IWindow, Transform>(window => new WindowHiddenTransform(window))
		};

		yield return new object[]
		{
			PInvoke.EVENT_OBJECT_DESTROY,
			new Func<IWindow, Transform>(window => new WindowRemovedTransform(window))
		};

		yield return new object[]
		{
			PInvoke.EVENT_OBJECT_CLOAKED,
			new Func<IWindow, Transform>(window => new WindowRemovedTransform(window))
		};

		yield return new object[]
		{
			PInvoke.EVENT_SYSTEM_MOVESIZESTART,
			new Func<IWindow, Transform>(window => new WindowMoveStartedTransform(window))
		};

		yield return new object[]
		{
			PInvoke.EVENT_SYSTEM_MOVESIZEEND,
			new Func<IWindow, Transform>(window => new WindowMoveEndedTransform(window))
		};

		yield return new object[]
		{
			PInvoke.EVENT_OBJECT_LOCATIONCHANGE,
			new Func<IWindow, Transform>(window => new WindowMovedTransform(window))
		};

		yield return new object[]
		{
			PInvoke.EVENT_SYSTEM_MINIMIZESTART,
			new Func<IWindow, Transform>(window => new WindowMinimizeStartedTransform(window))
		};

		yield return new object[]
		{
			PInvoke.EVENT_SYSTEM_MINIMIZEEND,
			new Func<IWindow, Transform>(window => new WindowMinimizeEndedTransform(window))
		};
	}

	[MemberAutoSubstituteData(nameof(WinEventProcCasesData))]
	[Theory]
	internal void WindowFocusedTransform(
		uint ev,
		Func<IWindow, Transform> createTransform,
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given the window exists
		IWindow window = Setup_FilledWindowSlice(ctx, internalCtx);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);

		WindowEventListener sut = new(ctx, internalCtx);
		sut.Initialize();

		// When we send through the event
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, ev, window.Handle, 0, 0, 0, 0);

		// Then a transform was dispatched
		ctx.Store.Received(1).Dispatch(createTransform(window));
	}

	[Theory, AutoSubstituteData]
	internal void Default(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window exists
		IWindow window = Setup_FilledWindowSlice(ctx, internalCtx);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);

		WindowEventListener sut = new(ctx, internalCtx);
		sut.Initialize();

		// When we send through the event
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, 0xBAAAD, window.Handle, 0, 0, 0, 0);

		// Then nothing happens
		AssertDispatches(ctx);
	}

	[Theory, AutoSubstituteData]
	internal void Throws(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window exists, and dispatching throws
		IWindow window = Setup_FilledWindowSlice(ctx, internalCtx);
		CaptureWinEventProc capture = CaptureWinEventProc.Create(internalCtx);

		WindowEventListener sut = new(ctx, internalCtx);
		sut.Initialize();

		ctx.Store.Dispatch(Arg.Any<WindowMinimizeStartedTransform>()).Throws(new WhimException("welp"));

		// When we send through the event
		capture.WinEventProc!.Invoke((HWINEVENTHOOK)0, PInvoke.EVENT_SYSTEM_MINIMIZESTART, window.Handle, 0, 0, 0, 0);

		// Then nothing happens
		AssertDispatches(ctx, windowMinimizeStartedTransformCount: 1);
	}
}
