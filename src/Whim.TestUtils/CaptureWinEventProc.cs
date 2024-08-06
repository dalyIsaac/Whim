using System.Collections.Generic;
using NSubstitute;
using Windows.Win32;
using Windows.Win32.UI.Accessibility;

namespace Whim.TestUtils;

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
	public List<WindowManagerFakeSafeHandle> Handles { get; } = [];

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
