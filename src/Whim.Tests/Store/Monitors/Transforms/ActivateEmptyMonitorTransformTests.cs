using System.Collections.Immutable;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class ActivateEmptyMonitorTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorNotFound(IContext ctx, IMonitor monitor)
	{
		// Given the monitor does not exist in the store
		ActivateEmptyMonitorTransform sut = new(monitor);

		// When we execute the transform
		ctx.Store.Dispatch(sut);

		// Then the ActiveMonitorIndex was not updated
		Assert.Equal(-1, ctx.Store.MonitorSlice.ActiveMonitorIndex);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorFound(IContext ctx, IMonitor monitor, IMonitor monitor1)
	{
		// Given the store contains multiple monitors
		ctx.Store.MonitorSlice.Monitors = ImmutableArray.Create(monitor, monitor1);
		ctx.Store.MonitorSlice.ActiveMonitorIndex = 1;

		ActivateEmptyMonitorTransform sut = new(monitor);

		// When we execute the transform
		ctx.Store.Dispatch(sut);

		// Then the ActiveMonitorIndex was updated
		Assert.Equal(0, ctx.Store.MonitorSlice.ActiveMonitorIndex);
	}
}
