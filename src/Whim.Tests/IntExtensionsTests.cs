using Xunit;

namespace Whim.Tests;

public class IntExtensions
{
	[Theory]
	[InlineData(1, 2, 1)]
	[InlineData(2, 2, 0)]
	[InlineData(3, 2, 1)]
	[InlineData(-1, 2, 1)]
	[InlineData(1, -2, -1)]
	public void Mod(int a, int b, int expected)
	{
		Assert.Equal(expected, a.Mod(b));
	}
}

