using Moq;
using Xunit;

namespace Whim.Bar.Tests;

public class ImmutableBarLayoutEngineTests
{
	[Fact]
	public void GetLayout()
	{
		// Given
		BarConfig config =
			new(
				leftComponents: new List<BarComponent>(),
				centerComponents: new List<BarComponent>(),
				rightComponents: new List<BarComponent>()
			)
			{
				Height = 30
			};

		Mock<IImmutableLayoutEngine> layoutEngine = new();
		Mock<IMonitor> monitor = new();
		monitor.SetupGet(m => m.ScaleFactor).Returns(100);

		ILocation<int> location = new Location<int>() { Width = 1920, Height = 1080 };

		ImmutableBarLayoutEngine engine = new(config, layoutEngine.Object);

		// When
		IEnumerable<IWindowState> layout = engine.DoLayout(location, monitor.Object);

		// Then
		ILocation<int> expectedLocation = new Location<int>()
		{
			X = 0,
			Y = 30,
			Width = 1920,
			Height = 1050
		};
		layoutEngine.Verify(m => m.DoLayout(expectedLocation, monitor.Object), Times.Once);
	}
}
