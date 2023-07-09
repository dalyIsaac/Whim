using Moq;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class WindowNodeTests
{
	[Fact]
	public void ToString_ReturnsWindowToString()
	{
		// Given
		Mock<IWindow> window = new();
		window.Setup(x => x.ToString()).Returns("window");
		WindowNode node = new(window.Object);

		// When
		string? result = node.ToString();

		// Then
		Assert.Equal("window", result);
	}
}
