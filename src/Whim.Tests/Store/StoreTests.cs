using System;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32;
using Windows.Win32.UI.Accessibility;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class StoreTests
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

	[Theory, AutoSubstituteData]
	internal void Initialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		Store sut = new(ctx, internalCtx);
		InitializeCoreNativeManagerMock(internalCtx);

		// When we initialize
		sut.Initialize();

		// Then the MonitorSector has initialized
		internalCtx.WindowMessageMonitor.WorkAreaChanged += Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
	}

	[Theory, AutoSubstituteData]
	internal void Initialize_Fail(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		Store sut = new(ctx, internalCtx);
		InitializeCoreNativeManagerMock(internalCtx);

		internalCtx
			.CoreNativeManager.SetWinEventHook(
				PInvoke.EVENT_SYSTEM_MINIMIZESTART,
				PInvoke.EVENT_SYSTEM_MINIMIZEEND,
				Arg.Any<WINEVENTPROC>()
			)
			.Returns(new UnhookWinEventSafeHandle());

		// When we initialize, then we throw
		Assert.Throws<InvalidOperationException>(sut.Initialize);
	}

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		Store sut = new(ctx, internalCtx);
		InitializeCoreNativeManagerMock(internalCtx);

		// When we initialize and then dispose
		sut.Initialize();
		sut.Dispose();

		// Then the MonitorSector has initialized
		internalCtx.WindowMessageMonitor.WorkAreaChanged -= Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
	}
}
