using System;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class WindowMessageMonitorTests
{
	private class CaptureProc
	{
		public SUBCLASSPROC? SubclassProc { get; private set; }

		public static CaptureProc Create(IInternalContext internalCtx)
		{
			CaptureProc captureProc = new();
			internalCtx
				.CoreNativeManager
				.SetWindowSubclass(Arg.Any<HWND>(), Arg.Any<SUBCLASSPROC>(), 4561, 0)
				.Returns(
					(callInfo) =>
					{
						captureProc.SubclassProc = callInfo.ArgAt<SUBCLASSPROC>(1);
						return (BOOL)true;
					}
				);
			return captureProc;
		}
	}

	[Theory, AutoSubstituteData]
	internal void Constructor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureProc capture = CaptureProc.Create(internalCtx);

		// When
		WindowMessageMonitor windowMessageMonitor = new(ctx, internalCtx);
		windowMessageMonitor.PreInitialize();

		// Then
		internalCtx
			.CoreNativeManager
			.Received(1)
			.SetWindowSubclass(Arg.Any<HWND>(), Arg.Any<SUBCLASSPROC>(), Arg.Any<nuint>(), Arg.Any<nuint>());
		internalCtx.CoreNativeManager.Received(1).WTSRegisterSessionNotification(Arg.Any<HWND>(), Arg.Any<uint>());
	}

	[Theory, AutoSubstituteData]
	internal void HandlException(IContext ctx, IInternalContext internalCtx)
	{
		// Given there is a subscriber to WM_DISPLAYCHANGE which throws
		CaptureProc capture = CaptureProc.Create(internalCtx);
		WindowMessageMonitor windowMessageMonitor = new(ctx, internalCtx);
		windowMessageMonitor.PreInitialize();

		static void handler(object? sender, WindowMessageMonitorEventArgs args)
		{
			throw new Exception("Test");
		}

		windowMessageMonitor.DisplayChanged += handler;

		// When we raise the event
		capture.SubclassProc?.Invoke((HWND)0, PInvoke.WM_DISPLAYCHANGE, (WPARAM)1, (LPARAM)1, 0, 0);

		// Then the exception is handled
		internalCtx
			.CoreNativeManager
			.Received(1)
			.DefSubclassProc(Arg.Any<HWND>(), Arg.Any<uint>(), Arg.Any<WPARAM>(), Arg.Any<LPARAM>());
	}

	[Theory, AutoSubstituteData]
	internal void WindowProc_WM_DISPLAYCHANGE(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureProc capture = CaptureProc.Create(internalCtx);

		// When
		WindowMessageMonitor windowMessageMonitor = new(ctx, internalCtx);
		windowMessageMonitor.PreInitialize();

		// Then
		Assert.Raises<WindowMessageMonitorEventArgs>(
			h => windowMessageMonitor.DisplayChanged += h,
			h => windowMessageMonitor.DisplayChanged -= h,
			() => capture.SubclassProc?.Invoke((HWND)0, PInvoke.WM_DISPLAYCHANGE, (WPARAM)1, (LPARAM)1, 0, 0)
		);
	}

	[Theory, AutoSubstituteData]
	internal void WindowProc_WM_WTSESSION_CHANGE(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureProc capture = CaptureProc.Create(internalCtx);

		// When
		WindowMessageMonitor windowMessageMonitor = new(ctx, internalCtx);
		windowMessageMonitor.PreInitialize();

		// Then
		Assert.Raises<WindowMessageMonitorEventArgs>(
			h => windowMessageMonitor.SessionChanged += h,
			h => windowMessageMonitor.SessionChanged -= h,
			() => capture.SubclassProc?.Invoke((HWND)0, PInvoke.WM_WTSSESSION_CHANGE, (WPARAM)1, (LPARAM)1, 0, 0)
		);
	}

	[Theory, AutoSubstituteData]
	internal void WindowProc_WM_SETTINGCHANGE_SPI_GETWORKAREA(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureProc capture = CaptureProc.Create(internalCtx);

		// When
		WindowMessageMonitor windowMessageMonitor = new(ctx, internalCtx);
		windowMessageMonitor.PreInitialize();

		// Then
		Assert.Raises<WindowMessageMonitorEventArgs>(
			h => windowMessageMonitor.WorkAreaChanged += h,
			h => windowMessageMonitor.WorkAreaChanged -= h,
			() =>
				capture
					.SubclassProc
					?.Invoke(
						(HWND)0,
						PInvoke.WM_SETTINGCHANGE,
						new WPARAM((nuint)SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA),
						(LPARAM)1,
						0,
						0
					)
		);
	}

	[Theory, AutoSubstituteData]
	internal void WindowProc_WM_SETTINGCHANGE_SPI_SETLOGICALDPIOVERRIDE(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureProc capture = CaptureProc.Create(internalCtx);

		// When
		WindowMessageMonitor windowMessageMonitor = new(ctx, internalCtx);
		windowMessageMonitor.PreInitialize();

		// Then
		Assert.Raises<WindowMessageMonitorEventArgs>(
			h => windowMessageMonitor.DpiChanged += h,
			h => windowMessageMonitor.DpiChanged -= h,
			() =>
				capture
					.SubclassProc
					?.Invoke(
						(HWND)0,
						PInvoke.WM_SETTINGCHANGE,
						new WPARAM((nuint)SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETLOGICALDPIOVERRIDE),
						(LPARAM)1,
						0,
						0
					)
		);
	}

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureProc capture = CaptureProc.Create(internalCtx);

		// When
		WindowMessageMonitor windowMessageMonitor = new(ctx, internalCtx);
		windowMessageMonitor.PreInitialize();
		windowMessageMonitor.Dispose();

		// Then
		internalCtx
			.CoreNativeManager
			.Received(1)
			.RemoveWindowSubclass(Arg.Any<HWND>(), Arg.Any<SUBCLASSPROC>(), Arg.Any<nuint>());
		internalCtx.CoreNativeManager.Received(1).WTSUnRegisterSessionNotification(Arg.Any<HWND>());
	}
}
