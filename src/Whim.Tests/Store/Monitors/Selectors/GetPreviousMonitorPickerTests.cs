using System.Collections.Immutable;
using Whim.TestUtils;
using Xunit;

namespace Whim;

public class GetPreviousMonitorPickerTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	public void CannotFindMonitor(IContext ctx, IMonitor monitor1, IMonitor monitor2, IMonitor unknownMonitor)
	{
		// Given
		ctx.Store.MonitorSlice.Monitors = ImmutableArray.Create(monitor1, monitor2);
		GetPreviousMonitorPicker sut = new(unknownMonitor);

		// When
		IMonitor result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(monitor1, result);
	}

	[InlineAutoSubstituteData<StoreCustomization>(0, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(2, 1)]
	[Theory]
	public void GetPreviousMonitor(
		int startIdx,
		int endIdx,
		IContext ctx,
		IMonitor monitor0,
		IMonitor monitor1,
		IMonitor monitor2
	)
	{
		// Given
		ctx.Store.MonitorSlice.Monitors = ImmutableArray.Create(monitor0, monitor1, monitor2);
		GetPreviousMonitorPicker sut = new(ctx.Store.MonitorSlice.Monitors[startIdx]);

		// When
		IMonitor result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(ctx.Store.MonitorSlice.Monitors[endIdx], result);
	}
}
