using FluentAssertions;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestDoLayout
{
	[Fact]
	public void DoLayout()
	{
		TestTree tree = new();

		ILocation<int> screen = new Location(0, 0, 1920, 1080);

		TreeLayoutWindowLocation[] locations = TreeLayoutEngine.GetWindowLocations(tree.Root, screen).ToArray();
		ILocation<int>[] actual = locations.Select(x => x.Location).ToArray();

		ILocation<int>[] expected = TestTreeWindowLocations.All.Select(x => x.ToLocation(screen)).ToArray();

		actual.Should().Equal(expected);
	}
}
