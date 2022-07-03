using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestGetNodeLocation
{
	private readonly TestTree _tree = new();

	#region GetNodeLocation
	[Fact]
	public void GetNodeLocation_Left()
	{
		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(_tree.Left);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowState.Left, location);
	}

	[Fact]
	public void GetNodeLocation_RightBottom()
	{
		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(_tree.RightBottom);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowState.RightBottom, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftTop()
	{
		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(_tree.RightTopLeftTop);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowState.RightTopLeftTop, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftBottomLeft()
	{
		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(_tree.RightTopLeftBottomLeft);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowState.RightTopLeftBottomLeft, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftBottomRightTop()
	{
		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(_tree.RightTopLeftBottomRightTop);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowState.RightTopLeftBottomRightTop, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopLeftBottomRightBottom()
	{
		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(_tree.RightTopLeftBottomRightBottom);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowState.RightTopLeftBottomRightBottom, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopRight1()
	{
		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(_tree.RightTopRight1);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowState.RightTopRight1, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopRight2()
	{
		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(_tree.RightTopRight2);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowState.RightTopRight2, location);
	}

	[Fact]
	public void GetNodeLocation_RightTopRight3()
	{
		ILocation<double>? location = TreeLayoutEngine.GetNodeLocation(_tree.RightTopRight3);

		Assert.NotNull(location);
		Assert.Equal(TestTreeWindowState.RightTopRight3, location);
	}
	#endregion

}
