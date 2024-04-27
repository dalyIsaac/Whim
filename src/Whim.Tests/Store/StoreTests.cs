using System;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class StoreTests
{
	[Theory, AutoSubstituteData]
	internal void Initialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		Store sut = new(ctx, internalCtx);

		// When we initialize
		sut.Initialize();

		// Then the MonitorSector has initialized
		internalCtx.WindowMessageMonitor.WorkAreaChanged += Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
	}

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		Store sut = new(ctx, internalCtx);

		// When we initialize and then dispose
		sut.Initialize();
		sut.Dispose();

		// Then the MonitorSector has initialized
		internalCtx.WindowMessageMonitor.WorkAreaChanged -= Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
	}
}
