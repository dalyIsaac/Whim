using System.Collections;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MonitorManagerTests
{
	[Theory, AutoSubstituteData]
	internal void ActiveMonitor(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.ActiveMonitor;

		// Then
		ctx.Store.Received(1).Pick(Pickers.GetActiveMonitor());
	}

	[Theory, AutoSubstituteData]
	internal void PrimaryMonitor(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.PrimaryMonitor;

		// Then
		ctx.Store.Received(1).Pick(Pickers.GetPrimaryMonitor());
	}

	[Theory, AutoSubstituteData]
	internal void Length(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.Length;

		// Then
		ctx.Store.Received(1).Pick(Pickers.GetAllMonitors());
	}

	[Theory, AutoSubstituteData]
	internal void GetEnumerator(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		sut.GetEnumerator();
		((IEnumerable)sut).GetEnumerator();

		// Then
		ctx.Store.Received(2).Pick(Pickers.GetAllMonitors());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsChanged(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When the monitor sector triggers a MonitorsChanged event
		// Then an MonitorsChanged event from the MonitorManager was triggered
		sut.Initialize();
		Assert.Raises<MonitorsChangedEventArgs>(
			h => sut.MonitorsChanged += h,
			h => sut.MonitorsChanged -= h,
			() => ctx.Store.Dispatch(new MonitorsChangedTransform())
		);
	}

	[Theory, AutoSubstituteData]
	internal void GetMonitorAtPoint(IContext ctx, IPoint<int> point)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.GetMonitorAtPoint(point);

		// Then
		ctx.Store.Received(1).Pick(Pickers.GetMonitorAtPoint(point, true));
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Dispose(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		sut.Initialize();
		sut.Dispose();

		// Then
		CustomAssert.DoesNotRaise<MonitorsChangedEventArgs>(
			h => sut.MonitorsChanged += h,
			h => sut.MonitorsChanged -= h,
			() => ctx.Store.Dispatch(new MonitorsChangedTransform())
		);
	}
}
