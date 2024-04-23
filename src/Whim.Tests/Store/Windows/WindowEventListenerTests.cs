using System;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32;
using Windows.Win32.UI.Accessibility;
using Xunit;

namespace Whim.Tests;

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
}
