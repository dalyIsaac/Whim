using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
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
		ctx.Store.Received(1).Pick(new GetActiveMonitorPicker());
	}

	[Theory, AutoSubstituteData]
	internal void PrimaryMonitor(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.PrimaryMonitor;

		// Then
		ctx.Store.Received(1).Pick(new GetPrimaryMonitorPicker());
	}

	[Theory, AutoSubstituteData]
	internal void LastWhimActiveMonitor(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.LastWhimActiveMonitor;

		// Then
		ctx.Store.Received(1).Pick(new GetLastWhimActiveMonitorPicker());
	}

	[Theory, AutoSubstituteData]
	internal void Length(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.Length;

		// Then
		ctx.Store.Received(1).Pick(new GetAllMonitorsPicker());
	}

	[Theory, AutoSubstituteData]
	internal void GetEnumerator(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.GetEnumerator();

		// Then
		ctx.Store.Received(1).Pick(new GetAllMonitorsPicker());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorsChanged(IContext ctx)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When the monitor slice triggers a MonitorsChanged event
		// Then an MonitorsChanged event from the MonitorManager was triggered
		sut.Initialize();
		Assert.Raises<MonitorsChangedEventArgs>(
			h => sut.MonitorsChanged += h,
			h => sut.MonitorsChanged -= h,
			() => ctx.Store.Dispatch(new MonitorsChangedTransform())
		);
	}

	[Theory, AutoSubstituteData]
	internal void OnWindowFocused(IContext ctx, IWindow window)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		sut.OnWindowFocused(window);

		// Then
		ctx.Store.Received(1).Dispatch(new WindowFocusedTransform(window));
	}

	[Theory, AutoSubstituteData]
	internal void ActivateEmptyMonitor(IContext ctx, IMonitor monitor)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		sut.ActivateEmptyMonitor(monitor);

		// Then
		ctx.Store.Received(1).Dispatch(new ActivateEmptyMonitorTransform(monitor));
	}

	[Theory, AutoSubstituteData]
	internal void GetMonitorAtPoint(IContext ctx, IPoint<int> point)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.GetMonitorAtPoint(point);

		// Then
		ctx.Store.Received(1).Pick(new GetMonitorAtPointPicker(point, true));
	}

	[Theory, AutoSubstituteData]
	internal void GetPreviousMonitor(IContext ctx, IMonitor monitor)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.GetPreviousMonitor(monitor);

		// Then
		ctx.Store.Received(1).Pick(new GetPreviousMonitorPicker(monitor, true));
	}

	[Theory, AutoSubstituteData]
	internal void GetNextMonitor(IContext ctx, IMonitor monitor)
	{
		// Given
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.GetNextMonitor(monitor);

		// Then
		ctx.Store.Received(requiredNumberOfCalls: 1).Pick(new GetNextMonitorPicker(monitor, true));
	}

	[Theory, AutoSubstituteData]
	internal void GetMonitorByHandle(IContext ctx)
	{
		// Given
		HMONITOR hMONITOR = (HMONITOR)1;
		MonitorManager sut = new(ctx);

		// When
		var _ = sut.GetMonitorByHandle(hMONITOR);

		// Then
		ctx.Store.Received(1).Pick(new GetAllMonitorsPicker());
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
