using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetWeightAndIndex
{
	private readonly TestTree _tree = new();

	[Fact]
	public void GetWeightAndIndex_Left()
	{
		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(_tree.Root, _tree.Left);
		Assert.Equal(0.5, weight);
		Assert.Equal(0, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopLeftBottomRightTop()
	{
		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(_tree.RightTopLeftBottomRight, _tree.RightTopLeftBottomRightTop);
		Assert.Equal(0.7, weight);
		Assert.Equal(0, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopLeftBottomRightBottom()
	{
		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(_tree.RightTopLeftBottomRight, _tree.RightTopLeftBottomRightBottom);
		Assert.Equal(0.3, weight);
		Assert.Equal(0.7, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopRight1()
	{
		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(_tree.RightTopRight, _tree.RightTopRight1);
		Assert.Equal(1d / 3, weight);
		Assert.Equal(0, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopRight2()
	{
		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(_tree.RightTopRight, _tree.RightTopRight2);
		Assert.Equal(1d / 3, weight);
		Assert.Equal(1d / 3, precedingWeight);
	}

	[Fact]
	public void GetWeightAndIndex_RightTopRight3()
	{
		(double weight, double precedingWeight) = TreeLayoutEngine.GetWeightAndIndex(_tree.RightTopRight, _tree.RightTopRight3);
		Assert.Equal(1d / 3, weight);
		Assert.Equal(2d / 3, precedingWeight);
	}
}
