using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class PhantomNodeTests
{
	[Fact]
	public void Focus()
	{
		// Given
		Mock<IWindow> window = new();
		PhantomNode node = new(window.Object);

		// When
		node.Focus();

		// Then
		window.Verify(x => x.Focus(), Times.Once);
	}
}
