using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class LeafNodeTests
{
	private class TestNode : LeafNode
	{
		public TestNode(IWindow window)
			: base(window) { }
	}

	[Fact]
	public void Focus()
	{
		// Given
		Mock<IWindow> window = new();
		TestNode leafNode = new(window.Object);

		// When
		leafNode.Focus();

		// Then
		window.Verify(w => w.Focus(), Times.Once);
	}
}
