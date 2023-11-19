using NSubstitute;
using Whim.TestUtils;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

public class MouseHookTests
{
	private class WindowsHookHandle : UnhookWindowsHookExSafeHandle
	{
		public int Calls { get; private set; }

		protected override void Dispose(bool disposing)
		{
			Calls++;
			base.Dispose(disposing);
		}
	}

	private class CaptureMouseHook
	{
		public HOOKPROC? MouseHook { get; private set; }
		public WindowsHookHandle? Handle { get; private set; }

		public static CaptureMouseHook Create(IInternalContext internalCtx)
		{
			CaptureMouseHook captureMouseHook = new();
			internalCtx
				.CoreNativeManager
				.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_MOUSE_LL, Arg.Any<HOOKPROC>(), null, 0)
				.Returns(
					(callInfo) =>
					{
						captureMouseHook.MouseHook = callInfo.ArgAt<HOOKPROC>(1);
						captureMouseHook.Handle ??= new WindowsHookHandle();
						return captureMouseHook.Handle;
					}
				);

			return captureMouseHook;
		}
	}

	[Theory, AutoSubstituteData]
	internal void PostInitialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureMouseHook capture = CaptureMouseHook.Create(internalCtx);
		using MouseHook mouseHook = new(ctx, internalCtx);

		// When
		mouseHook.PostInitialize();

		// Then
		Assert.NotNull(capture.MouseHook);
	}

	[Theory, AutoSubstituteData]
	internal void OnMouseTriggerEvent_LParamIsZero(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureMouseHook capture = CaptureMouseHook.Create(internalCtx);
		using MouseHook mouseHook = new(ctx, internalCtx);

		// When
		mouseHook.PostInitialize();

		// Then
		CustomAssert.DoesNotRaise<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonDown += h,
			h => mouseHook.MouseLeftButtonDown -= h,
			() => capture.MouseHook!.Invoke(0, (WPARAM)PInvoke.WM_LBUTTONDOWN, 0)
		);
	}

	[Theory, AutoSubstituteData]
	internal void OnMouseTriggerEvent_NotMSLLHOOKSTRUCT(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureMouseHook capture = CaptureMouseHook.Create(internalCtx);
		internalCtx.CoreNativeManager.PtrToStructure<MSLLHOOKSTRUCT>(Arg.Any<nint>()).Returns((MSLLHOOKSTRUCT?)null);
		using MouseHook mouseHook = new(ctx, internalCtx);

		// When
		mouseHook.PostInitialize();

		// Then
		CustomAssert.DoesNotRaise<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonDown += h,
			h => mouseHook.MouseLeftButtonDown -= h,
			() => capture.MouseHook!.Invoke(0, (WPARAM)PInvoke.WM_LBUTTONDOWN, 1)
		);
	}

	[Theory, AutoSubstituteData]
	internal void OnMouseTriggerEvent_Success_WM_LBUTTONDOWN(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureMouseHook capture = CaptureMouseHook.Create(internalCtx);
		System.Drawing.Point point = new(1, 2);
		MSLLHOOKSTRUCT msllhookstruct = new() { pt = point };
		internalCtx.CoreNativeManager.PtrToStructure<MSLLHOOKSTRUCT>(Arg.Any<nint>()).Returns(msllhookstruct);
		using MouseHook mouseHook = new(ctx, internalCtx);

		// When
		mouseHook.PostInitialize();

		// Then
		var result = Assert.Raises<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonDown += h,
			h => mouseHook.MouseLeftButtonDown -= h,
			() => capture.MouseHook!.Invoke(0, (WPARAM)PInvoke.WM_LBUTTONDOWN, 1)
		);
		Assert.Equal(point.X, result.Arguments.Point.X);
		Assert.Equal(point.Y, result.Arguments.Point.Y);
	}

	[Theory, AutoSubstituteData]
	internal void OnMouseTriggerEvent_Success_WM_LBUTTONUP(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureMouseHook capture = CaptureMouseHook.Create(internalCtx);
		System.Drawing.Point point = new(1, 2);
		MSLLHOOKSTRUCT msllhookstruct = new() { pt = point };
		internalCtx.CoreNativeManager.PtrToStructure<MSLLHOOKSTRUCT>(Arg.Any<nint>()).Returns(msllhookstruct);
		using MouseHook mouseHook = new(ctx, internalCtx);

		// When
		mouseHook.PostInitialize();

		// Then
		var result = Assert.Raises<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonUp += h,
			h => mouseHook.MouseLeftButtonUp -= h,
			() => capture.MouseHook!.Invoke(0, (WPARAM)PInvoke.WM_LBUTTONUP, 1)
		);
		Assert.Equal(point.X, result.Arguments.Point.X);
		Assert.Equal(point.Y, result.Arguments.Point.Y);
	}

	[Theory, AutoSubstituteData]
	internal void OtherKey(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureMouseHook capture = CaptureMouseHook.Create(internalCtx);
		internalCtx.CoreNativeManager.PtrToStructure<MSLLHOOKSTRUCT>(Arg.Any<nint>()).Returns(new MSLLHOOKSTRUCT());
		using MouseHook mouseHook = new(ctx, internalCtx);

		// When
		mouseHook.PostInitialize();

		// Then
		CustomAssert.DoesNotRaise<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonDown += h,
			h => mouseHook.MouseLeftButtonDown -= h,
			() => capture.MouseHook!.Invoke(0, (WPARAM)PInvoke.WM_KEYDOWN, 1)
		);

		CustomAssert.DoesNotRaise<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonUp += h,
			h => mouseHook.MouseLeftButtonUp -= h,
			() => capture.MouseHook!.Invoke(0, (WPARAM)PInvoke.WM_KEYDOWN, 1)
		);
	}

	[Theory, AutoSubstituteData]
	internal void HandleException(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureMouseHook capture = CaptureMouseHook.Create(internalCtx);
		System.Drawing.Point point = new(1, 2);
		MSLLHOOKSTRUCT msllhookstruct = new() { pt = point };
		internalCtx.CoreNativeManager.PtrToStructure<MSLLHOOKSTRUCT>(Arg.Any<nint>()).Returns(msllhookstruct);
		using MouseHook mouseHook = new(ctx, internalCtx);

		mouseHook.PostInitialize();

		// When
		internalCtx
			.CoreNativeManager
			.PtrToStructure<MSLLHOOKSTRUCT>(Arg.Any<nint>())
			.Returns(_ => throw new System.Exception("Test exception"));
		CustomAssert.DoesNotRaise<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonDown += h,
			h => mouseHook.MouseLeftButtonDown -= h,
			() => capture.MouseHook!.Invoke(0, (WPARAM)PInvoke.WM_LBUTTONDOWN, 1)
		);

		// The
		Assert.Equal(0, capture.Handle?.Calls);
	}

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureMouseHook capture = CaptureMouseHook.Create(internalCtx);
		using MouseHook mouseHook = new(ctx, internalCtx);
		mouseHook.PostInitialize();

		// When
		mouseHook.Dispose();

		// Then
		Assert.Equal(1, capture.Handle?.Calls);
	}

	[Theory, AutoSubstituteData]
	internal void AlreadyDisposed(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureMouseHook capture = CaptureMouseHook.Create(internalCtx);
		using MouseHook mouseHook = new(ctx, internalCtx);
		mouseHook.PostInitialize();
		mouseHook.Dispose();

		// When
		mouseHook.Dispose();

		// Then
		Assert.Equal(1, capture.Handle?.Calls);
	}
}
