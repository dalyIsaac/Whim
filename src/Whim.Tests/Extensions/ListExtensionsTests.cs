namespace Whim.Tests;

public class ListExtensionsTests
{
	[Fact]
	public void FindIndex_ReturnsCorrectIndex()
	{
		// Arrange
		IReadOnlyList<int> list = new List<int> { 1, 2, 3, 4, 5 };

		// Act
		int index = list.FindIndex(x => x == 3);

		// Then
		Assert.Equal(2, index);
	}

	[Fact]
	public void FindIndex_NotFound_ReturnsNegativeOne()
	{
		// Arrange
		IReadOnlyList<int> list = new List<int> { 1, 2, 3, 4, 5 };

		// Act
		int index = list.FindIndex(x => x == 6);

		// Then
		Assert.Equal(-1, index);
	}

	[Fact]
	public void FindIndex_EmptyList_ReturnsNegativeOne()
	{
		// Arrange
		IReadOnlyList<int> list = new List<int>();

		// Act
		int index = list.FindIndex(x => x == 1);

		// Then
		Assert.Equal(-1, index);
	}
}
