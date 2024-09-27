using Xunit;

namespace Whim.SliceLayout.Tests;

public class AreaTests
{
	[Fact]
	public void ParentArea_Equals()
	{
		// Given
		ParentArea area1 = new(isRow: true, (1, new OverflowArea(isRow: true)));
		ParentArea area2 = new(isRow: true, (1, new OverflowArea(isRow: true)));

		// Then
		Assert.Equal(area1, area2);
	}

	[Fact]
	public void ParentArea_GetHashCode()
	{
		// Given
		ParentArea area1 = new(isRow: true, (1, new OverflowArea(isRow: true)));
		ParentArea area2 = new(isRow: true, (1, new OverflowArea(isRow: true)));

		// Then
		Assert.NotEqual(0, area1.GetHashCode());
		Assert.Equal(area1.GetHashCode(), area2.GetHashCode());
	}
}
