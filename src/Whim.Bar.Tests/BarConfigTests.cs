using Xunit;

namespace Whim.Bar.Tests;

public class BarConfigTests
{
	[Fact]
	public void Height_PropertyChanged()
	{
		// Given
		BarConfig config = new(leftComponents: [], centerComponents: [], rightComponents: []);

		// When
		// Then
		Assert.PropertyChanged(config, nameof(config.Height), () => config.Height = 1);
		Assert.Equal(1, config.Height);
	}
}
