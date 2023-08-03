using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class WindowNodeTests
{
	[Fact]
	public void Focus()
	{
		// Given
		Mock<IWindow> window = new();
		WindowNode windowNode = new(window.Object);

		// When
		windowNode.Focus();

		// Then
		window.Verify(w => w.Focus(), Times.Once);
	}

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
