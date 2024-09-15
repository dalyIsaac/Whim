using Xunit;

namespace Whim.Yaml;

public class YamlLoaderUtilsTests
{
	[Theory]
	[InlineData("route_to_launched_workspace", "RouteToLaunchedWorkspace")]
	[InlineData("route_to_active_workspace", "RouteToActiveWorkspace")]
	[InlineData(" ", " ")]
	public void SnakeToPascal(string snake, string expected)
	{
		// Given a snake case string
		// When converting it to Pascal case
		string result = snake.SnakeToPascal();

		// Then the string is converted to camel case
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("none", BackdropType.None)]
	[InlineData("acrylic", BackdropType.Acrylic)]
	[InlineData("acrylic_thin", BackdropType.AcrylicThin)]
	[InlineData("mica", BackdropType.Mica)]
	[InlineData("mica_alt", BackdropType.MicaAlt)]
	[InlineData(" ", BackdropType.None)]
	[InlineData("", BackdropType.None)]
	public void ParseBackdropType(string backdropType, BackdropType expected)
	{
		// Given a backdrop type string
		// When parsing it
		BackdropType result = backdropType.ParseBackdropType();

		// Then the string is converted to a backdrop type
		Assert.Equal(expected, result);
	}
}
