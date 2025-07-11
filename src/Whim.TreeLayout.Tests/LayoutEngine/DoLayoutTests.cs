using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class DoLayoutTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void DoLayout_RootIsNull(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IMonitor monitor
	)
	{
		// Given
		TreeLayoutEngine engine = new(ctx, plugin, identity);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		IWindowState[] windowStates = [.. engine.DoLayout(rect, monitor)];

		// Then
		Assert.Empty(windowStates);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void DoLayout_OnlyMinimizedWindows(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IMonitor monitor
	)
	{
		// Given a layout with two minimized windows
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		IWindow[] minimizedWindows = [.. Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>())];

		IWindowState[] expectedWindowStates =
		[
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = minimizedWindows[0],
				WindowSize = WindowSize.Minimized,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = minimizedWindows[1],
				WindowSize = WindowSize.Minimized,
			},
		];

		// When the windows are added, and DoLayout is called
		for (int idx = 0; idx < minimizedWindows.Length; idx++)
		{
			engine = engine.MinimizeWindowStart(minimizedWindows[idx]);
		}

		IWindowState[] windowStates = [.. engine.DoLayout(rect, monitor)];

		// Then there will be 2 windows from the result of DoLayout
		Assert.Equal(2, windowStates.Length);
		Assert.Equal(2, engine.Count);
		expectedWindowStates.Should().BeEquivalentTo(windowStates);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void DoLayout_MinimizedWindows(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IMonitor monitor
	)
	{
		// Given a layout with two windows and two minimized windows
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		IWindow[] windows = [.. Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>())];
		IWindow[] minimizedWindows = [.. Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>())];

		IWindowState[] expectedWindowStates =
		[
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 50,
					Height = 100,
				},
				Window = windows[0],
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 50,
					Y = 0,
					Width = 50,
					Height = 100,
				},
				Window = windows[1],
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = minimizedWindows[0],
				WindowSize = WindowSize.Minimized,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = minimizedWindows[1],
				WindowSize = WindowSize.Minimized,
			},
		];

		// When the windows are added, and DoLayout is called
		for (int idx = 0; idx < windows.Length; idx++)
		{
			engine = engine.AddWindow(windows[idx]);
		}

		for (int idx = 0; idx < minimizedWindows.Length; idx++)
		{
			engine = engine.MinimizeWindowStart(minimizedWindows[idx]);
		}

		IWindowState[] windowStates = [.. engine.DoLayout(rect, monitor)];

		// Then there will be 4 windows from the result of DoLayout
		Assert.Equal(4, windowStates.Length);
		Assert.Equal(4, engine.Count);
		expectedWindowStates.Should().BeEquivalentTo(windowStates);
	}
}
