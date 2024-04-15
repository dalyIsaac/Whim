using System.Collections.Immutable;
using System.Drawing;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;

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
		(
			new RECT()
			{
				left = 0,
				top = 0,
				right = 1920,
				bottom = 1080
			},
			(HMONITOR)2
		)
	};

	[Theory, AutoSubstituteData<MonitorUtilsTestCustomization>]
	internal void GetCurrentMonitors_SingleMonitor(IInternalContext internalCtx)
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
	internal void GetCurrentMonitors_MultipleMonitors(IInternalContext internalCtx)
	{
		// Given there are multiple monitors
		MonitorTestUtils.SetupMultipleMonitors(internalCtx, MULTI_MONITOR_SETUP);

		// When we get the current monitors
		ImmutableArray<IMonitor> monitors = MonitorUtils.GetCurrentMonitors(internalCtx);

		// Then there are two monitors, one of which is the primary.
		Assert.Equal(2, monitors.Length);

		Assert.Equal(1920, monitors[0].WorkingArea.Width);
		Assert.Equal(1080, monitors[0].WorkingArea.Height);
		Assert.Equal(0, monitors[0].WorkingArea.X);
		Assert.Equal(0, monitors[0].WorkingArea.Y);
		Assert.True(monitors[0].IsPrimary);

		Assert.Equal(1920, monitors[1].WorkingArea.Width);
		Assert.Equal(1080, monitors[1].WorkingArea.Height);
		Assert.Equal(1920, monitors[1].WorkingArea.X);
		Assert.Equal(0, monitors[1].WorkingArea.Y);
		Assert.False(monitors[1].IsPrimary);
	}
}
