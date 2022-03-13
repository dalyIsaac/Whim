using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetWeightAndPrecedingWeight
{
	private readonly TestTree _tree = new();

	[Fact]
	public void GetWeightAndPrecedingWeight_Left()
	{
		var result = _tree.Root.GetWeightAndPrecedingWeight(_tree.Left);
		Assert.Equal(0.5, result?.weight);
		Assert.Equal(0, result?.precedingWeight);
	}

	[Fact]
	public void GetWeightAndPrecedingWeight_RightTopLeftBottomRightTop()
	{
		var result = _tree.RightTopLeftBottomRight.GetWeightAndPrecedingWeight(_tree.RightTopLeftBottomRightTop);
		Assert.Equal(0.7, result?.weight);
		Assert.Equal(0, result?.precedingWeight);
	}

	[Fact]
	public void GetWeightAndPrecedingWeight_RightTopLeftBottomRightBottom()
	{
		var result = _tree.RightTopLeftBottomRight.GetWeightAndPrecedingWeight(_tree.RightTopLeftBottomRightBottom);
		Assert.Equal(0.3, result?.weight);
		Assert.Equal(0.7, result?.precedingWeight);
	}

	[Fact]
	public void GetWeightAndPrecedingWeight_RightTopRight1()
	{
		var result = _tree.RightTopRight.GetWeightAndPrecedingWeight(_tree.RightTopRight1);
		Assert.Equal(1d / 3, result?.weight);
		Assert.Equal(0, result?.precedingWeight);
	}

	[Fact]
	public void GetWeightAndPrecedingWeight_RightTopRight2()
	{
		var result = _tree.RightTopRight.GetWeightAndPrecedingWeight(_tree.RightTopRight2);
		Assert.Equal(1d / 3, result?.weight);
		Assert.Equal(1d / 3, result?.precedingWeight);
	}

	[Fact]
	public void GetWeightAndPrecedingWeight_RightTopRight3()
	{
		var result = _tree.RightTopRight.GetWeightAndPrecedingWeight(_tree.RightTopRight3);
		Assert.Equal(1d / 3, result?.weight);
		Assert.Equal(2d / 3, result?.precedingWeight);
	}
}
