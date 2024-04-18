using System;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MonitorSliceTests
{
	[Theory, AutoSubstituteData]
	internal void Initialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorSlice sut = new(ctx, internalCtx);

		// When
		sut.Initialize();

		// Then a MonitorsChangedTransform was dispatched, and the listener subscribed to events
		ctx.Store.Received(1).Dispatch(Arg.Any<MonitorsChangedTransform>());
		internalCtx.WindowMessageMonitor.DisplayChanged += Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
	}

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		MonitorSlice sut = new(ctx, internalCtx);

		// When we initialize and dispose
		sut.Initialize();
		sut.Dispose();

		// Then the listener unsubscribed to events
		internalCtx.WindowMessageMonitor.DisplayChanged -= Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
	}
}
