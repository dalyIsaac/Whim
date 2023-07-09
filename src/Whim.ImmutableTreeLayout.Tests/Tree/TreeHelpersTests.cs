using Moq;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class TreeHelpersTests
{
	[InlineData(Direction.Right, true)]
	[InlineData(Direction.Down, true)]
	[InlineData(Direction.Up, false)]
	[InlineData(Direction.Left, false)]
	[Theory]
	public void InsertAfter_ReturnsExpected(Direction direction, bool expected)
	{
		// Given
		// When
		bool result = direction.InsertAfter();

		// Then
		Assert.Equal(expected, result);
	}

	#region GetNodeAtPath
	[Fact]
	public void GetNodeAtPath_WithEmptyPath_ReturnsRoot()
	{
		// Given
		Mock<INode> root = new();

		// When
		var result = root.Object.GetNodeAtPath(Array.Empty<int>());

		// Then
		Assert.NotNull(result);
		Assert.Empty(result.Value.Ancestors);
		Assert.Equal(root.Object, result.Value.Node);
		Assert.Equal(Location.UnitSquare<double>(), result.Value.Location);
	}

	[Fact]
	public void GetNodeAtPath_CurrentNodeIsNotSplitNode()
	{
		// Given
		Mock<INode> root = new();
		int[] path = { 0 };

		// When
		var result = root.Object.GetNodeAtPath(path);

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetNodeAtPath_ThrowsSplitNodeOutOfRangeException() { }
	#endregion
}
