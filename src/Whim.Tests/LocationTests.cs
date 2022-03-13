using Xunit;

namespace Whim.Tests;

public class LocationTests
{
	[Fact]
	public void IsPointInside_ReturnsTrue_WhenPointIsInside()
	{
		Location location = new(0, 0, 10, 10);
		Assert.True(location.IsPointInside(5, 5));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutside()
	{
		Location location = new(0, 0, 10, 10);
		Assert.False(location.IsPointInside(15, 15));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutsideX()
	{
		Location location = new(0, 0, 10, 10);
		Assert.False(location.IsPointInside(-5, 5));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutsideY()
	{
		Location location = new(0, 0, 10, 10);
		Assert.False(location.IsPointInside(5, -5));
	}

	[Fact]
	public void IsPointInside_ReturnsTrue_WhenPointIsOnEdge()
	{
		Location location = new(0, 0, 10, 10);
		Assert.True(location.IsPointInside(0, 0));
		Assert.True(location.IsPointInside(10, 10));
		Assert.True(location.IsPointInside(0, 10));
		Assert.True(location.IsPointInside(10, 0));
	}

	[Fact]
	public void Add_ReturnsNewLocation_WhenLocationsAreAdded()
	{
		Location location1 = new(0, 0, 10, 10);
		Location location2 = new(5, 5, 5, 5);
		ILocation<int> location3 = Location.Add(location1, location2);
		Assert.StrictEqual(new Location(5, 5, 15, 15), location3);
	}
}
