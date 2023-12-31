using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class DoLayoutTests
{
	[Theory, AutoSubstituteData]
	public void DoLayout_RootIsNull(IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		IWindowState[] windowStates = engine.DoLayout(rect, monitor).ToArray();

		// Then
		Assert.Empty(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_MinimizedWindows(IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		IWindow[] windows = Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>()).ToArray();
		IWindow[] minimizedWindows = Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>()).ToArray();

		IWindowState[] expectedWindowStates = new IWindowState[]
		{
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 50,
					Height = 100
				},
				Window = windows[0],
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 50,
					Y = 0,
					Width = 50,
					Height = 100
				},
				Window = windows[1],
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = minimizedWindows[0],
				WindowSize = WindowSize.Minimized
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = minimizedWindows[1],
				WindowSize = WindowSize.Minimized
			}
		};

		// When
		for (int idx = 0; idx < windows.Length; idx++)
		{
			engine = engine.AddWindow(windows[idx]);
		}

		for (int idx = 0; idx < minimizedWindows.Length; idx++)
		{
			engine = engine.MinimizeWindowStart(minimizedWindows[idx]);
		}

		IWindowState[] windowStates = engine.DoLayout(rect, monitor).ToArray();

		// Then
		Assert.Equal(4, windowStates.Length);
		expectedWindowStates.Should().BeEquivalentTo(windowStates);
	}
}
