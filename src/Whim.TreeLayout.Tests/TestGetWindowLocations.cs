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

		ILocation<int> screen = new Location(0, 0, 1920, 1080);

		TreeLayoutWindowLocation[] locations = TreeLayoutEngine.GetWindowLocations(tree.Root, screen).ToArray();
		ILocation<int>[] actual = locations.Select(x => x.Location).ToArray();

		ILocation<int>[] expected = TestTreeWindowLocations.All.Select(x => x.Scale(screen)).ToArray();

		actual.Should().Equal(expected);
	}

	[Fact]
	public void DoLayout_NullRoot()
	{
		TestTreeEngineEmpty testTreeEngine = new();
		ILocation<int> screen = new Location(0, 0, 1920, 1080);

		IEnumerable<IWindowLocation> actual = testTreeEngine.Engine.DoLayout(screen);

		Assert.Empty(actual);
	}

	[Fact]
	public void DoLayout_TestTreeEngine()
	{
		ILocation<int> screen = new Location(0, 0, 1920, 1080);
		TestTreeEngine testTreeEngine = new();

		IEnumerable<IWindowLocation> actual = testTreeEngine.Engine.DoLayout(screen);
		IWindowLocation[] expected = TestTreeWindowLocations.GetAllWindowLocations(screen,
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
		TestTreeEngine testTreeEngine = new();

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
		TreeLayoutEngine engine = new(new Mock<IConfigContext>().Object);
		Assert.Empty(engine);
	}
}
