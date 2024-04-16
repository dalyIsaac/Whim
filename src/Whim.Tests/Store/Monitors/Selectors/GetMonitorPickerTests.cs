using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Whim.TestUtils;
using Xunit;

namespace Whim;

public class GetMonitorPickerTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetActiveMonitor(IContext ctx, IMonitor monitor)
	{
		// Given
		ctx.Store.MonitorSlice.Monitors = ImmutableArray.Create(monitor);
		ctx.Store.MonitorSlice.ActiveMonitorIndex = 0;

		GetActiveMonitorPicker sut = new();

		// When
		IMonitor result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(monitor, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetPrimaryMonitor(IContext ctx, IMonitor monitor)
	{
		// Given
		ctx.Store.MonitorSlice.Monitors = ImmutableArray.Create(monitor);
		ctx.Store.MonitorSlice.PrimaryMonitorIndex = 0;

		GetPrimaryMonitorPicker sut = new();

		// When
		IMonitor result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(monitor, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetLastWhimActiveMonitor(IContext ctx, IMonitor monitor)
	{
		// Given
		ctx.Store.MonitorSlice.Monitors = ImmutableArray.Create(monitor);
		ctx.Store.MonitorSlice.LastWhimActiveMonitorIndex = 0;

		GetLastWhimActiveMonitorPicker sut = new();

		// When
		IMonitor result = ctx.Store.Pick(sut);

		// Then
		Assert.Equal(monitor, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void GetAllMonitors(IContext ctx, IMonitor monitor, IMonitor monitor2)
	{
		// Given
		ctx.Store.MonitorSlice.Monitors = ImmutableArray.Create(monitor, monitor2);

		GetAllMonitorsPicker sut = new();

		// When
		IMonitor[] results = ctx.Store.Pick(sut).ToArray();

		// Then
		Assert.Equal(2, results.Length);
		results.Should().BeEquivalentTo(new[] { monitor, monitor2 });
	}
}
