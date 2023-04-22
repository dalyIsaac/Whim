using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetWindowLocations
{
	[Fact]
	public void GetWindowLocations()
	{
		TestTree tree = new();

		ILocation<int> screen = new Location<int>()
		{
			X = 0,
			Y = 0,
			Width = 1920,
			Height = 1080
		};

		NodeState[] locations = TreeLayoutEngine.GetWindowLocations(tree.Root, screen).ToArray();
		ILocation<int>[] actual = locations.Select(x => x.Location).ToArray();

		ILocation<int>[] expected = TestTreeWindowState.All.Select(x => x.Scale(screen)).ToArray();

		actual.Should().Equal(expected);
	}

	[Fact]
	public void DoLayout_NullRoot()
	{
		TestTreeEngineEmptyMocks testTreeEngine = new();
		ILocation<int> screen = new Location<int>()
		{
			X = 0,
			Y = 0,
			Width = 1920,
			Height = 1080
		};

		IEnumerable<IWindowState> actual = testTreeEngine.Engine.DoLayout(screen, new Mock<IMonitor>().Object);

		Assert.Empty(actual);
	}

	[Fact]
	public void DoLayout_TestTreeEngine()
	{
		ILocation<int> screen = new Location<int>()
		{
			X = 0,
			Y = 0,
			Width = 1920,
			Height = 1080
		};
		TestTreeEngineMocks testTreeEngine = new();

		IEnumerable<IWindowState> actual = testTreeEngine.Engine.DoLayout(screen, new Mock<IMonitor>().Object);
		IWindowState[] expected = TestTreeWindowState.GetAllWindowStates(
			screen,
			testTreeEngine.LeftWindow.Object,
			testTreeEngine.RightTopLeftTopWindow.Object,
			testTreeEngine.RightTopLeftBottomLeftWindow.Object,
			testTreeEngine.RightTopLeftBottomRightTopWindow.Object,
			testTreeEngine.RightTopLeftBottomRightBottomWindow.Object,
			testTreeEngine.RightTopRight1Window.Object,
			testTreeEngine.RightTopRight2Window.Object,
			testTreeEngine.RightTopRight3Window.Object,
			testTreeEngine.RightBottomWindow.Object
		);

		actual.Should().Equal(expected);
	}

	[Fact]
	public void IWindow_GetEnumerator()
	{
		TestTreeEngineMocks testTreeEngine = new();

		IEnumerable<IWindow> actual = testTreeEngine.Engine;

		IWindow[] expected = new IWindow[]
		{
			testTreeEngine.LeftWindow.Object,
			testTreeEngine.RightTopLeftTopWindow.Object,
			testTreeEngine.RightTopLeftBottomLeftWindow.Object,
			testTreeEngine.RightTopLeftBottomRightTopWindow.Object,
			testTreeEngine.RightTopLeftBottomRightBottomWindow.Object,
			testTreeEngine.RightTopRight1Window.Object,
			testTreeEngine.RightTopRight2Window.Object,
			testTreeEngine.RightTopRight3Window.Object,
			testTreeEngine.RightBottomWindow.Object
		};

		actual.Should().Equal(expected);
	}

	[Fact]
	public void IWindow_GetEnumerator_Null()
	{
		TreeLayoutEngine engine = new(new Mock<IContext>().Object);
		Assert.Empty(engine);
	}
}
