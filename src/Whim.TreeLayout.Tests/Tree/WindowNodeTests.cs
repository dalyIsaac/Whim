using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class WindowNodeTests
{
	[Theory, AutoSubstituteData]
	public void Focus(IWindow window)
	{
		// Given
		WindowNode windowNode = new(window);

		// When
		windowNode.Focus();

		// Then
		window.Received(1).Focus();
	}
}
