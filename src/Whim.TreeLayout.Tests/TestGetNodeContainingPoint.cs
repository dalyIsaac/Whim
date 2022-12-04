using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetNodeContainingPoint
{
	private readonly TestTree _testTree = new();
	private readonly ILocation<double> rootLocation = new Location<double>()
	{
		X = 0,
		Y = 0,
		Width = 100,
		Height = 100
	};

	[InlineData(-5, 0)]
	[InlineData(0, -5)]
	[InlineData(100, 0)]
	[InlineData(0, 100)]
	[Theory]
	public void Outside(double x, double y)
	{
		IPoint<double> searchPoint = new Point<double>(x, y);

		Assert.Null(TreeLayoutEngine.GetNodeContainingPoint(root: _testTree.Root, rootLocation, searchPoint));
	}

	[InlineData(0, 0)]
	[InlineData(0, 49)]
	[InlineData(49, 49)]
	[InlineData(49, 0)]
	[InlineData(25, 25)]
	[Theory]
	public void Left(double x, double y)
	{
		IPoint<double> searchPoint = new Point<double>(x, y);

		Assert.Same(
			_testTree.Left,
			TreeLayoutEngine.GetNodeContainingPoint(root: _testTree.Root, rootLocation, searchPoint)
		);
	}

	[InlineData(50 + (25d / 2), 25)]
	[InlineData(50 + (25d / 2), 24 + (25 * 0.7))]
	[InlineData(74, 25)]
	[InlineData(74, 25 + (24 * 0.7))]
	[InlineData(65, 30)]
	[Theory]
	public void RightTopLeftBottomRightTop(double x, double y)
	{
		IPoint<double> searchPoint = new Point<double>(x, y);

		Assert.Same(
			_testTree.RightTopLeftBottomRightTop,
			TreeLayoutEngine.GetNodeContainingPoint(root: _testTree.Root, rootLocation, searchPoint)
		);
	}

	[InlineData(50 + (25d / 2), 25 + (25 * 0.7))]
	[InlineData(50 + (25d / 2), 49)]
	[InlineData(74, 25 + (25 * 0.7))]
	[InlineData(74, 49)]
	[InlineData(65, 45)]
	[Theory]
	public void RightTopLeftBottomRightBottom(double x, double y)
	{
		IPoint<double> searchPoint = new Point<double>(x, y);

		Assert.Same(
			_testTree.RightTopLeftBottomRightBottom,
			TreeLayoutEngine.GetNodeContainingPoint(root: _testTree.Root, rootLocation, searchPoint)
		);
	}
}
