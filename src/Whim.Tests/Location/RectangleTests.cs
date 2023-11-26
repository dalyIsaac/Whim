using Xunit;

namespace Whim.Tests;

public class RectangleTests
{
	[Fact]
	public void IsPointInside_ReturnsTrue_WhenPointIsInside()
	{
		Rectangle<int> rect = new() { Width = 10, Height = 10 };
		Assert.True(rect.ContainsPoint(new Point<int>() { X = 5, Y = 5 }));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutside()
	{
		Rectangle<int> rect = new() { Width = 10, Height = 10 };
		Assert.False(rect.ContainsPoint(new Point<int>() { X = 15, Y = 15 }));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutsideX()
	{
		Rectangle<int> rect = new() { Width = 10, Height = 10 };
		Assert.False(rect.ContainsPoint(new Point<int>() { X = -5, Y = 5 }));
	}

	[Fact]
	public void IsPointInside_ReturnsFalse_WhenPointIsOutsideY()
	{
		Rectangle<int> rect = new() { Width = 10, Height = 10 };
		Assert.False(rect.ContainsPoint(new Point<int>() { X = 5, Y = -5 }));
	}

	[Fact]
	public void IsPointInside_ReturnsTrue_WhenPointIsOnEdge()
	{
		Rectangle<int> rect = new() { Width = 10, Height = 10 };

		// Extreme boundaries.
		Assert.True(rect.ContainsPoint(new Point<int>()));
		Assert.False(rect.ContainsPoint(new Point<int>() { X = 10, Y = 10 }));
		Assert.False(rect.ContainsPoint(new Point<int>() { X = 0, Y = 10 }));
		Assert.False(rect.ContainsPoint(new Point<int>() { X = 10, Y = 0 }));

		// Other boundaries.
		Assert.True(rect.ContainsPoint(new Point<int>() { X = 5, Y = 0 }));
		Assert.True(rect.ContainsPoint(new Point<int>() { X = 0, Y = 5 }));

		// Internal points.
		Assert.True(rect.ContainsPoint(new Point<int>() { X = 5, Y = 5 }));

		// External points.
		Assert.False(rect.ContainsPoint(new Point<int>() { X = 5, Y = -5 }));
		Assert.False(rect.ContainsPoint(new Point<int>() { X = -5, Y = 5 }));
		Assert.False(rect.ContainsPoint(new Point<int>() { X = -5, Y = -5 }));
	}

	[Fact]
	public void Add_ReturnsNewRectangle_WhenRectanglesAreAdded()
	{
		Rectangle<int> rect1 = new() { Width = 10, Height = 10 };
		Rectangle<int> rect2 =
			new()
			{
				X = 5,
				Y = 5,
				Width = 5,
				Height = 5
			};
		IRectangle<int> rect3 = rect1.Add(rect2);
		Assert.StrictEqual(
			new Rectangle<int>()
			{
				X = 5,
				Y = 5,
				Width = 15,
				Height = 15
			},
			rect3
		);
	}

	[Fact]
	public void GetHashCode_NotEqual()
	{
		// Given
		Rectangle<int> rect1 = new() { Width = 10, Height = 10 };
		Rectangle<int> rect2 =
			new()
			{
				X = 5,
				Y = 5,
				Width = 5,
				Height = 5
			};

		// When
		int hashCode1 = rect1.GetHashCode();
		int hashCode2 = rect2.GetHashCode();

		// Then
		Assert.NotEqual(hashCode1, hashCode2);
	}

	[Fact]
	public void GetHashCode_Equal()
	{
		// Given
		Rectangle<int> rect1 = new() { Width = 10, Height = 10 };
		Rectangle<int> rect2 = new() { Width = 10, Height = 10 };

		// When
		int hashCode1 = rect1.GetHashCode();
		int hashCode2 = rect2.GetHashCode();

		// Then
		Assert.Equal(hashCode1, hashCode2);
	}
}
