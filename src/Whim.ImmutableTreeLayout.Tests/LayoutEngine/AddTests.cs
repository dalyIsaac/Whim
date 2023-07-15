using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class AddTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IImmutableInternalTreePlugin> Plugin { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();

		public Wrapper()
		{
			Plugin.Setup(x => x.PhantomWindows).Returns(new HashSet<IWindow>());

			WorkspaceManager.Setup(x => x.ActiveWorkspace).Returns(Workspace.Object);
			Context.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
		}

		public Wrapper SetAsPhantom(IWindow window)
		{
			Plugin.Setup(x => x.PhantomWindows).Returns(new HashSet<IWindow> { window });
			return this;
		}

		public Wrapper SetAsLastFocusedWindow(IWindow? window)
		{
			Workspace.Setup(x => x.LastFocusedWindow).Returns(window);
			return this;
		}
	}

	[Fact]
	public void AddWindow_RootIsNull()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object);

		// When
		IImmutableLayoutEngine result = engine.Add(window.Object);

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
		Wrapper wrapper = new Wrapper().SetAsPhantom(window.Object);
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object);

		// When
		IImmutableLayoutEngine result = engine.Add(window.Object);

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
		Wrapper wrapper = new Wrapper().SetAsPhantom(phantomWindow.Object);
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(
			phantomWindow.Object
		);

		Mock<IWindow> window = new();

		// When
		IImmutableLayoutEngine result = engine.Add(window.Object);

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
		Wrapper wrapper = new();
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(
			window1.Object
		);
		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.Add(window2.Object);
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

		Wrapper wrapper = new Wrapper().SetAsLastFocusedWindow(null);

		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.Add(window3.Object);
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

		Wrapper wrapper = new Wrapper().SetAsLastFocusedWindow(window3.Object);

		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.Add(window3.Object);
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

		Wrapper wrapper = new Wrapper().SetAsLastFocusedWindow(window1.Object);

		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.Add(window3.Object);
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
}
