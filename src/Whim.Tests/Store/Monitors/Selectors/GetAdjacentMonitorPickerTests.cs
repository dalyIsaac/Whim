using System.Collections.Immutable;
using DotNext;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class GetAdjacentMonitorPickerTests
{
	[InlineAutoSubstituteData<StoreCustomization>(true)]
	[InlineAutoSubstituteData<StoreCustomization>(false)]
	[Theory]
	internal void CannotFindMonitor(
		bool reverse,
		IContext ctx,
		MutableRootSector mutableRootSector,
		IMonitor monitor1,
		IMonitor monitor2,
		IMonitor unknownMonitor
	)
	{
		// Given
		mutableRootSector.Monitors.Monitors = ImmutableArray.Create(monitor1, monitor2);
		GetAdjacentMonitorPicker sut = new(unknownMonitor, Reverse: reverse, GetFirst: true);

		// When
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(monitor1, result.Value);
	}

	[InlineAutoSubstituteData<StoreCustomization>(true, 0, 1)]
	[InlineAutoSubstituteData<StoreCustomization>(true, 2, 0)]
	[InlineAutoSubstituteData<StoreCustomization>(false, 0, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(false, 2, 1)]
	[Theory]
	internal void GetAdjacentMonitor(
		bool reverse,
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
		GetAdjacentMonitorPicker sut = new(mutableRootSector.Monitors.Monitors[startIdx], Reverse: reverse);

		// When
		Result<IMonitor> result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(mutableRootSector.Monitors.Monitors[endIdx], result.Value);
	}
}
