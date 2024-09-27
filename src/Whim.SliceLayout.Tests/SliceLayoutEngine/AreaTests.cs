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

	public static TheoryData<ParentArea, ParentArea> ParentArea_NotEqual =>
		new()
		{
			{
				new(isRow: true, (1, new OverflowArea(isRow: true))),
				new(isRow: false, (1, new OverflowArea(isRow: true)))
			},
			{
				new(isRow: true, (1, new OverflowArea(isRow: true))),
				new(isRow: true, (1, new OverflowArea(isRow: false)))
			},
			{
				new(isRow: true, (1, new OverflowArea(isRow: true))),
				new(isRow: true, (1, new OverflowArea(isRow: true)), (1, new OverflowArea(isRow: true)))
			},
		};

	[Theory, MemberData(nameof(ParentArea_NotEqual))]
	public void ParentArea_Equals_NotEqual(ParentArea area1, ParentArea area2)
	{
		// Then
		Assert.NotEqual(area1, area2);
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
