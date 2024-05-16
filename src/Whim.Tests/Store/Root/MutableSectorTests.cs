using System;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class MutableRootSectorTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Initialize_Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MutableRootSector sut = new(ctx, internalCtx);
		CaptureWinEventProc.Create(internalCtx);

		// When
		sut.Initialize();
		sut.Dispose();

		// Then
		internalCtx.WindowMessageMonitor.DisplayChanged += Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
		internalCtx.WindowMessageMonitor.DisplayChanged -= Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
	}
}
