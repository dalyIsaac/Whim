using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

		IContext context = fixture.Freeze<IContext>();
		context.WorkspaceManager.Returns(workspaceManager);
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
	// TODO
}
