using DotNext;
using System.Collections.Immutable;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class GetNextMonitorPickerTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	public void CannotFindMonitor(IContext ctx, IMonitor monitor1, IMonitor monitor2, IMonitor unknownMonitor)
	{
		// Given
		ctx.Store.MonitorSlice.Monitors = ImmutableArray.Create(monitor1, monitor2);
		GetNextMonitorPicker sut = new(unknownMonitor, GetFirst: true);

		// When
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(monitor1, result.Value);
	}

	[InlineAutoSubstituteData<StoreCustomization>(0, 1)]
	[InlineAutoSubstituteData<StoreCustomization>(2, 0)]
	[Theory]
	public void GetNextMonitor(
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
		GetNextMonitorPicker sut = new(ctx.Store.MonitorSlice.Monitors[startIdx]);

		// When
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(ctx.Store.MonitorSlice.Monitors[endIdx], result.Value);
	}
}
