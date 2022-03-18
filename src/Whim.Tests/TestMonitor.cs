using Moq;
using System.Drawing;
using System.Windows.Forms;
using Xunit;

namespace Whim.Tests;

public class TestMonitor
{
	[InlineData(1920, 1080, 192, 108, 0.1, 0.1)]
	[InlineData(1920, 1080, 960, 270, 0.5, 0.25)]
	[Theory]
	public void ToUnitSquare(int width, int height, int pointX, int pointY, int expectedX, int expectedY)
	{
		// Given
		Mock<Screen> screen = new();
		screen.Setup(s => s.WorkingArea).Returns(new Rectangle(0, 0, width, height));

		Monitor monitor = new(screen.Object);

		// When
		IPoint<double> point = monitor.ToUnitSquare(new Point<int>(pointX, pointY));

		// Then
		Assert.Equal(expectedX, point.X);
		Assert.Equal(expectedY, point.Y);
	}
}
