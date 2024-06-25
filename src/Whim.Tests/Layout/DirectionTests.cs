namespace Whim.Tests;

public class DirectionTests
{
	[InlineData(Direction.None, false)]
	[InlineData(Direction.Left, true)]
	[InlineData(Direction.Right, true)]
	[InlineData(Direction.Up, false)]
	[InlineData(Direction.Down, false)]
	[InlineData(Direction.LeftUp, false)]
	[InlineData(Direction.LeftDown, false)]
	[InlineData(Direction.RightUp, false)]
	[InlineData(Direction.RightDown, false)]
	[Theory]
	public void IsHorizontal(Direction direction, bool expected)
	{
		Assert.Equal(expected, direction.IsHorizontal());
	}
}
