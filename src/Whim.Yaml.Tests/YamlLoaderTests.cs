using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoaderCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();

		ctx.FileManager.GetWhimFileDir(Arg.Any<string>()).Returns(v => v.Arg<string>());
	}
}

public class YamlLoaderTests
{
	private static string CreateEmptyYamlConfig()
	{
		return """
			# yaml-language-server: $schema=./schema.json
			keybinds:
			""".Replace("\t", "");
	}

	private static string CreateEmptyJsonConfig()
	{
		return """
			{
				"keybinds": []
			}
			""";
	}

	[Theory, AutoSubstituteData<YamlLoaderCustomization>]
	public void Load_NoConfigFile_ReturnsNull(IContext ctx)
	{
		// Given no config file exists
		// When
		bool result = YamlLoader.Load(ctx);

		// Then
		Assert.False(result);
		ctx.KeybindManager.DidNotReceive().SetKeybind(Arg.Any<string>(), Arg.Any<IKeybind>());
	}

	[Theory, AutoSubstituteData<YamlLoaderCustomization>]
	public void Load_EmptyYamlConfig(IContext ctx)
	{
		// Given an empty YAML config file exists
		string yaml = CreateEmptyYamlConfig();
		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(yaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and no keybinds are set
		Assert.True(result);
		ctx.KeybindManager.DidNotReceive().SetKeybind(Arg.Any<string>(), Arg.Any<IKeybind>());
	}

	[Theory, AutoSubstituteData<YamlLoaderCustomization>]
	public void Load_EmptyJsonConfig(IContext ctx)
	{
		// Given an empty JSON config file exists
		string json = CreateEmptyJsonConfig();
		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(json);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and no keybinds are set
		Assert.True(result);
		ctx.KeybindManager.DidNotReceive().SetKeybind(Arg.Any<string>(), Arg.Any<IKeybind>());
	}

	[Theory, AutoSubstituteData<YamlLoaderCustomization>]
	public void Load_InvalidYamlConfig(IContext ctx)
	{
		// Given an invalid YAML config file exists
		string yaml = "";
		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(yaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is false, and no keybinds are set
		Assert.False(result);
		ctx.KeybindManager.DidNotReceive().SetKeybind(Arg.Any<string>(), Arg.Any<IKeybind>());
	}

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
