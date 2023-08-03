using Xunit;

namespace Whim.Tests;

public class LocationTests
{
	[Fact]
	public void IsPointInside_ReturnsTrue_WhenPointIsInside()
	{
		Location<int> location = new() { Width = 10, Height = 10 };
		Assert.True(location.ContainsPoint(new Point<int>() { X = 5, Y = 5 }));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutside()
	{
		Location<int> location = new() { Width = 10, Height = 10 };
		Assert.False(location.ContainsPoint(new Point<int>() { X = 15, Y = 15 }));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutsideX()
	{
		Location<int> location = new() { Width = 10, Height = 10 };
		Assert.False(location.ContainsPoint(new Point<int>() { X = -5, Y = 5 }));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutsideY()
	{
		Location<int> location = new() { Width = 10, Height = 10 };
		Assert.False(location.ContainsPoint(new Point<int>() { X = 5, Y = -5 }));
	}

	[Fact]
	public void IsPointInside_ReturnsTrue_WhenPointIsOnEdge()
	{
		Location<int> location = new() { Width = 10, Height = 10 };

		// Extreme boundaries.
		Assert.True(location.ContainsPoint(new Point<int>()));
		Assert.False(location.ContainsPoint(new Point<int>() { X = 10, Y = 10 }));
		Assert.False(location.ContainsPoint(new Point<int>() { X = 0, Y = 10 }));
		Assert.False(location.ContainsPoint(new Point<int>() { X = 10, Y = 0 }));

		// Other boundaries.
		Assert.True(location.ContainsPoint(new Point<int>() { X = 5, Y = 0 }));
		Assert.True(location.ContainsPoint(new Point<int>() { X = 0, Y = 5 }));

		// Internal points.
		Assert.True(location.ContainsPoint(new Point<int>() { X = 5, Y = 5 }));

		// External points.
		Assert.False(location.ContainsPoint(new Point<int>() { X = 5, Y = -5 }));
		Assert.False(location.ContainsPoint(new Point<int>() { X = -5, Y = 5 }));
		Assert.False(location.ContainsPoint(new Point<int>() { X = -5, Y = -5 }));
	}

	[Fact]
	public void Add_ReturnsNewLocation_WhenLocationsAreAdded()
	{
		Location<int> location1 = new() { Width = 10, Height = 10 };
		Location<int> location2 =
			new()
			{
				X = 5,
				Y = 5,
				Width = 5,
				Height = 5
			};
		ILocation<int> location3 = location1.Add(location2);
		Assert.StrictEqual(
			new Location<int>()
			{
				X = 5,
				Y = 5,
				Width = 15,
				Height = 15
			},
			location3
		);
	}

	[Fact]
	public void GetHashCode_NotEqual()
	{
		// Given
		Location<int> location1 = new() { Width = 10, Height = 10 };
		Location<int> location2 =
			new()
			{
				X = 5,
				Y = 5,
				Width = 5,
				Height = 5
			};

		// When
		int hashCode1 = location1.GetHashCode();
		int hashCode2 = location2.GetHashCode();

		// Then
		Assert.NotEqual(hashCode1, hashCode2);
	}

	[Fact]
	public void GetHashCode_Equal()
	{
		// Given
		Location<int> location1 = new() { Width = 10, Height = 10 };
		Location<int> location2 = new() { Width = 10, Height = 10 };

		// When
		int hashCode1 = location1.GetHashCode();
		int hashCode2 = location2.GetHashCode();

		// Then
		Assert.Equal(hashCode1, hashCode2);
	}
}
