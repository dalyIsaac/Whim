using Xunit;

namespace Whim.Tests;

public class TestMonitor
{
	[Theory]
	[InlineData(0, 0, 1920, 1080, 192, 108, 0.1, 0.1)]
	[InlineData(0, 0, 1920, 1080, 960, 270, 0.5, 0.25)]
	[InlineData(100, 100, 1920, 1080, 192, 108, 0.1, 0.1)]
	[InlineData(100, 100, 1920, 1080, 960, 270, 0.5, 0.25)]
	[InlineData(-100, 100, 1920, 1080, 192, 108, 0.1, 0.1)]
	[InlineData(-100, 100, 1920, 1080, 960, 270, 0.5, 0.25)]
	public void IntToUnitSquare(int monX, int monY, int monWidth, int monHeight, int pointX, int pointY, double expectedX, double expectedY)
	{
		// Given
		ILocation<int> monitor = new Location(monX, monY, monWidth, monHeight);

		// When
		IPoint<double> point = monitor.ToUnitSquare(new Point<int>(pointX, pointY));

		// Then
		Assert.Equal(expectedX, point.X);
		Assert.Equal(expectedY, point.Y);
	}

	[Theory]
	[InlineData(0, 0, 1920, 1080, 192, 108, 192, 108, 0.1, 0.1, 0.1, 0.1)]
	[InlineData(100, 100, 1920, 1080, 192, 108, 192, 108, 0.1, 0.1, 0.1, 0.1)]
	[InlineData(-100, -100, 1920, 1080, 192, 108, 192, 108, 0.1, 0.1, 0.1, 0.1)]
	public void DoubleToUnitSquare(int monX, int monY, int monWidth, int height, int locX, int locY, int locWidth, int locHeight, double expectedX, double expectedY, double expectedWidth, double expectedHeight)
	{
		// Given
		ILocation<int> monitor = new Location(monX, monY, monWidth, height);

		// When
		ILocation<double> location = monitor.ToUnitSquare(new Location(locX, locY, locWidth, locHeight));

		// Then
		Assert.Equal(expectedX, location.X);
		Assert.Equal(expectedY, location.Y);
		Assert.Equal(expectedWidth, location.Width);
		Assert.Equal(expectedHeight, location.Height);
	}

	[Theory]
	[InlineData(0, 0, 1920, 1080, 0.1, 0.1, 0.1, 0.1, 192, 108, 192, 108)]
	[InlineData(100, 100, 1920, 1080, 0.1, 0.1, 0.1, 0.1, 192 + 100, 108 + 100, 192, 108)]
	public void ToMonitor(int monX, int monY, int monWidth, int monHeight, double locX, double locY, double locWidth, double locHeight, double expectedX, double expectedY, double expectedWidth, double expectedHeight)
	{
		// Given
		ILocation<int> monitor = new Location(monX, monY, monWidth, monHeight);

		// When
		ILocation<int> location = monitor.ToMonitor(new DoubleLocation(locX, locY, locWidth, locHeight));

		// Then
		Assert.Equal(expectedX, location.X);
		Assert.Equal(expectedY, location.Y);
		Assert.Equal(expectedWidth, location.Width);
		Assert.Equal(expectedHeight, location.Height);
	}
}
