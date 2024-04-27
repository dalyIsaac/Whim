using System.Collections.Immutable;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class ActivateEmptyMonitorTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorNotFound(IContext ctx, MutableRootSector mutableRootSector, IMonitor monitor)
	{
		// Given the monitor does not exist in the store
		ActivateEmptyMonitorTransform sut = new(monitor);

		// When we execute the transform
		ctx.Store.Dispatch(sut);

		// Then the ActiveMonitorIndex was not updated
		Assert.Equal(-1, mutableRootSector.Monitors.ActiveMonitorIndex);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorFound(IContext ctx, MutableRootSector mutableRootSector, IMonitor monitor, IMonitor monitor1)
	{
		// Given the store contains multiple monitors
		mutableRootSector.Monitors.Monitors = ImmutableArray.Create(monitor, monitor1);
		mutableRootSector.Monitors.ActiveMonitorIndex = 1;

		ActivateEmptyMonitorTransform sut = new(monitor);

		// When we execute the transform
		ctx.Store.Dispatch(sut);

		// Then the ActiveMonitorIndex was updated
		Assert.Equal(0, mutableRootSector.Monitors.ActiveMonitorIndex);
	}
}
