using System.Collections.Immutable;
using DotNext;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class GetPreviousMonitorPickerTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void CannotFindMonitor(
		IContext ctx,
		MutableRootSector mutableRootSector,
		IMonitor monitor1,
		IMonitor monitor2,
		IMonitor unknownMonitor
	)
	{
		// Given
		mutableRootSector.Monitors.Monitors = ImmutableArray.Create(monitor1, monitor2);
		GetPreviousMonitorPicker sut = new(unknownMonitor, GetFirst: true);

		// When
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(monitor1, result.Value);
	}

	[InlineAutoSubstituteData<StoreCustomization>(0, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(2, 1)]
	[Theory]
	internal void GetPreviousMonitor(
		int startIdx,
		int endIdx,
		IContext ctx,
		MutableRootSector mutableRootSector,
		IMonitor monitor0,
		IMonitor monitor1,
		IMonitor monitor2
	)
	{
		// Given
		mutableRootSector.Monitors.Monitors = ImmutableArray.Create(monitor0, monitor1, monitor2);
		GetPreviousMonitorPicker sut = new(mutableRootSector.Monitors.Monitors[startIdx]);

		// When
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(mutableRootSector.Monitors.Monitors[endIdx], result.Value);
	}
}
