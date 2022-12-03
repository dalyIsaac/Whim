using Xunit;

namespace Whim.Tests;

public class LocationTests
{
	[Fact]
	public void IsPointInside_ReturnsTrue_WhenPointIsInside()
	{
		Location<int> location = new(0, 0, 10, 10);
		Assert.True(location.IsPointInside(new Point<int>(5, 5)));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutside()
	{
		Location<int> location = new(0, 0, 10, 10);
		Assert.False(location.IsPointInside(new Point<int>(15, 15)));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutsideX()
	{
		Location<int> location = new(0, 0, 10, 10);
		Assert.False(location.IsPointInside(new Point<int>(-5, 5)));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutsideY()
	{
		Location<int> location = new(0, 0, 10, 10);
		Assert.False(location.IsPointInside(new Point<int>(5, -5)));
	}

	[Fact]
	public void IsPointInside_ReturnsTrue_WhenPointIsOnEdge()
	{
		Location<int> location = new(0, 0, 10, 10);

		// Extreme boundaries.
		Assert.True(location.IsPointInside(new Point<int>(0, 0)));
		Assert.False(location.IsPointInside(new Point<int>(10, 10)));
		Assert.False(location.IsPointInside(new Point<int>(0, 10)));
		Assert.False(location.IsPointInside(new Point<int>(10, 0)));

		// Other boundaries.
		Assert.True(location.IsPointInside(new Point<int>(5, 0)));
		Assert.True(location.IsPointInside(new Point<int>(0, 5)));

		// Internal points.
		Assert.True(location.IsPointInside(new Point<int>(5, 5)));

		// External points.
		Assert.False(location.IsPointInside(new Point<int>(5, -5)));
		Assert.False(location.IsPointInside(new Point<int>(-5, 5)));
		Assert.False(location.IsPointInside(new Point<int>(-5, -5)));
	}

	[Fact]
	public void Add_ReturnsNewLocation_WhenLocationsAreAdded()
	{
		Location<int> location1 = new(0, 0, 10, 10);
		Location<int> location2 = new(5, 5, 5, 5);
		ILocation<int> location3 = Location<int>.Add(location1, location2);
		Assert.StrictEqual(new Location<int>(5, 5, 15, 15), location3);
	}
}
