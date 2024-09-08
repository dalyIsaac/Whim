using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.UI.Input.KeyboardAndMouse;
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

	public static TheoryData<string, bool> KeybindConfig =>
		new()
		{
			{
				"""
					keybinds:
					  - command: whim.test.command_id_1
					    keybind: LCtrl + A
					  - command: whim.test.command_id_2
					    keybind: LCtrl + LShift + B
					  - command: whim.test.command_id_3
					    keybind: ctrl+shift+alt+c
					""",
				true
			},
			{
				"""
					{
						"keybinds": [
							{
								"command": "whim.test.command_id_1",
								"keybind": "LCtrl + A"
							},
							{
								"command": "whim.test.command_id_2",
								"keybind": "LCtrl + LShift + B"
							},
							{
								"command": "whim.test.command_id_3",
								"keybind": "ctrl+shift+alt+c"
							}
						]
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(KeybindConfig))]
	public void Load_Keybinds(string config, bool isYaml, IContext ctx)
	{
		// Given a valid config with keybinds set
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(config);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and keybinds are set
		Assert.True(result);
		ctx.KeybindManager.Received()
			.SetKeybind("whim.test.command_id_1", new Keybind(KeyModifiers.LControl, VIRTUAL_KEY.VK_A));
		ctx.KeybindManager.Received()
			.SetKeybind(
				"whim.test.command_id_2",
				new Keybind(KeyModifiers.LControl | KeyModifiers.LShift, VIRTUAL_KEY.VK_B)
			);
		ctx.KeybindManager.Received()
			.SetKeybind(
				"whim.test.command_id_3",
				new Keybind(KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt, VIRTUAL_KEY.VK_C)
			);
	}

	public static TheoryData<string, bool> InvalidKeybindConfig =>
		new()
		{
			{
				"""
					keybinds:
					  - command: whim.test.command_id_1
					    keybind: this is a string but not a keybind
					""",
				true
			},
			{
				"""
					keybinds:
					- command: whim.test.command_id_1
					  keybind: 
					""",
				true
			},
			{
				"""
					keybinds:
					- command: whim.test.command_id_1
					  keybind: []
					""",
				true
			},
			{
				"""
					{
						"keybinds": [
							{
								"command": "whim.test.command_id_1",
								"keybind": "this is a string but not a keybind"
							}
						]
					}
					""",
				false
			},
			{
				"""
					{
						"keybinds": [
							{
								"command": "whim.test.command_id_1",
								"keybind": []
							}
						]
					}
					""",
				false
			},
			{
				"""
					{
						"keybinds": [
							{
								"command": "whim.test.command_id_1",
								"keybind": ""
							}
						]
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(InvalidKeybindConfig))]
	public void Load_InvalidKeybinds(string config, bool isYaml, IContext ctx)
	{
		// Given an invalid config with keybinds set
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(config);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and no keybinds are set
		Assert.True(result);
		ctx.KeybindManager.DidNotReceive().SetKeybind(Arg.Any<string>(), Arg.Any<IKeybind>());
	}
}
