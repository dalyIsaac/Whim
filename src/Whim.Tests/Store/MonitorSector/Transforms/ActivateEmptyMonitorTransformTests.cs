namespace Whim.Tests;

public class ActivateEmptyMonitorTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorNotFound(IContext ctx, MutableRootSector mutableRootSector)
	{
		// Given the monitor does not exist in the store
		ActivateEmptyMonitorTransform sut = new((HMONITOR)123);

		// When we execute the transform
		ctx.Store.WhimDispatch(sut);

		// Then the ActiveMonitorIndex was not updated
		Assert.Equal((HMONITOR)0, mutableRootSector.MonitorSector.ActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorFound(IContext ctx, MutableRootSector mutableRootSector, IMonitor monitor, IMonitor monitor1)
	{
		// Given the store contains multiple monitors
		HMONITOR handle = (HMONITOR)123;
		mutableRootSector.MonitorSector.Monitors = [monitor, monitor1];
		mutableRootSector.MonitorSector.ActiveMonitorHandle = handle;

		ActivateEmptyMonitorTransform sut = new(handle);

		// When we execute the transform
		ctx.Store.WhimDispatch(sut);

		// Then the ActiveMonitorIndex was updated
		Assert.Equal(handle, mutableRootSector.MonitorSector.ActiveMonitorHandle);
	}
}
