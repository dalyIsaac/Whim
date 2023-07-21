using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class PropertiesTests
{
	[InlineData(Direction.Up)]
	[InlineData(Direction.Right)]
	[InlineData(Direction.Down)]
	[InlineData(Direction.Left)]
	[Theory]
	public void AddNodeDirection_Success(Direction direction)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object) { AddNodeDirection = direction };

		// When
		Direction result = engine.AddNodeDirection;

		// Then
		Assert.Equal(direction, result);
	}

	[InlineData(Direction.LeftUp)]
	[InlineData(Direction.RightUp)]
	[InlineData(Direction.LeftDown)]
	[InlineData(Direction.RightDown)]
	[InlineData((Direction)128000)]
	[Theory]
	public void AddNodeDirection_Invalid(Direction direction)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object) { AddNodeDirection = direction };

		// When
		Direction result = engine.AddNodeDirection;

		// Then
		Assert.Equal(Direction.Right, result);
	}

	[Fact]
	public void Name()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object) { Name = "Test" };

		// When
		string result = engine.Name;

		// Then
		Assert.Equal("Test", result);
	}
}
