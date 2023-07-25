using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class AddTests
{
	[Fact]
	public void AddWindow_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		Mock<IWindow> window = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity);

		// When
		ILayoutEngine result = engine.Add(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void AddPhantom_RootIsNull()
	{
		// Given
		Mock<IWindow> window = new();
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsPhantom(window.Object);
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity);

		// When
		ILayoutEngine result = engine.Add(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void AddWindow_RootIsPhantom()
	{
		// Given
		Mock<IWindow> phantomWindow = new();
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsPhantom(phantomWindow.Object);
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).Add(phantomWindow.Object);

		Mock<IWindow> window = new();

		// When
		ILayoutEngine result = engine.Add(window.Object);

		// Then
		Assert.NotSame(engine, result);
		Assert.False(result.Contains(phantomWindow.Object));
		Assert.True(result.Contains(window.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void AddWindow_RootIsWindow()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).Add(window1.Object);
		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.Add(window2.Object);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 50,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 50,
							Y = 0,
							Width = 50,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Fact]
	public void AddWindow_RootIsSplitNode_LastFocusedWindowIsNull()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.Add(window1.Object)
			.Add(window2.Object);

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.Add(window3.Object);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window3.Object));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 33,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3.Object,
						Location = new Location<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Fact]
	public void AddWindow_RootIsSplitNode_CannotFindLastFocusedWindow()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window3.Object);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.Add(window1.Object)
			.Add(window2.Object);

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.Add(window3.Object);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window3.Object));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 33,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3.Object,
						Location = new Location<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Fact]
	public void AddWindow_RootIsSplitNode_LastFocusedWindowIsLeft()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.Add(window1.Object)
			.Add(window2.Object);

		wrapper.SetAsLastFocusedWindow(window1.Object);

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.Add(window3.Object);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window3.Object));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3.Object,
						Location = new Location<int>()
						{
							X = 33,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Fact]
	public void AddWindow_RootIsSplitNode_AddInDifferentDirection()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.Add(window1.Object)
			.Add(window2.Object);

		wrapper.Plugin.Setup(p => p.GetAddWindowDirection((TreeLayoutEngine)engine)).Returns(Direction.Down);

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.Add(window3.Object);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window3.Object));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 50,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 50,
							Y = 0,
							Width = 50,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 50,
							Width = 100,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}
}
