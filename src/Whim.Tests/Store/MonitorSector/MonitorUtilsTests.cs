using System.Drawing;
using AutoFixture;

namespace Whim.Tests;

internal class MonitorUtilsTestCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		internalCtx
			.CoreNativeManager.MonitorFromPoint(Arg.Any<Point>(), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
			.Returns(new HMONITOR(1));
	}
}

public class MonitorUtilsTests
{
	private static readonly (RECT, HMONITOR)[] MULTI_MONITOR_SETUP = new[]
	{
		// right
		(
			new RECT()
			{
				left = 1920,
				top = 0,
				right = 3840,
				bottom = 1080
			},
			(HMONITOR)1
		),
		// left top
		(
			new RECT()
			{
				left = 0,
				top = 0,
				right = 1920,
				bottom = 1080
			},
			(HMONITOR)2
		),
		// left bottom
		(
			new RECT()
			{
				left = 0,
				top = 1080,
				right = 1920,
				bottom = 2160
			},
			(HMONITOR)3
		),
	};

	[Theory, AutoSubstituteData<MonitorUtilsTestCustomization>]
	internal void SingleMonitor(IInternalContext internalCtx)
	{
		// Given there is a single monitor
		internalCtx.CoreNativeManager.HasMultipleMonitors().Returns(false);

		// When we get the current monitors
		ImmutableArray<IMonitor> monitors = MonitorUtils.GetCurrentMonitors(internalCtx);

		// Then we get the only monitor, which is also the primary monitor.
		Assert.Single(monitors);

		IMonitor m = monitors[0];
		Assert.True(m.IsPrimary);
	}

	[Theory, AutoSubstituteData<MonitorUtilsTestCustomization>]
	internal void MultipleMonitors(IInternalContext internalCtx)
	{
		// Given there are multiple monitors
		MonitorTestUtils.SetupMultipleMonitors(internalCtx, MULTI_MONITOR_SETUP);

		// When we get the current monitors
		ImmutableArray<IMonitor> monitors = MonitorUtils.GetCurrentMonitors(internalCtx);

		// Then there are three monitors, the first of which is the primary.
		// The monitors are ordered.
		Assert.Equal(3, monitors.Length);

		Assert.True(monitors[0].IsPrimary);
		Assert.False(monitors[1].IsPrimary);
		Assert.False(monitors[2].IsPrimary);

		Assert.Equal((HMONITOR)2, monitors[0].Handle);
		Assert.Equal((HMONITOR)3, monitors[1].Handle);
		Assert.Equal((HMONITOR)1, monitors[2].Handle);
	}
}
