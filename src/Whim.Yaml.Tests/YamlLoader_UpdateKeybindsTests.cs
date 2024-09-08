using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_UpdateKeybindsTests
{
	public static TheoryData<string, bool> KeybindConfig =>
		new()
		{
			{
				"""
					keybinds:
					  entries:
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
						"keybinds": {
							"entries": [
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
					  entries:
					  - command: whim.test.command_id_1
					    keybind: this is a string but not a keybind
					""",
				true
			},
			{
				"""
					keybinds:
					  entries:
					  - command: whim.test.command_id_1
					    keybind:
					""",
				true
			},
			{
				"""
					keybinds:
					  entries:
					  - command: whim.test.command_id_1
					    keybind: []
					""",
				true
			},
			{
				"""
					{
						"keybinds": {
							"entries": [
								{
									"command": "whim.test.command_id_1",
									"keybind": "this is a string but not a keybind"
								}
							]
						}
					}
					""",
				false
			},
			{
				"""
					{
						"keybinds": {
							"entries": [
								{
									"command": "whim.test.command_id_1",
									"keybind": []
								}
							]
						}
					}
					""",
				false
			},
			{
				"""
					{
						"keybinds": {
							"entries": [
								{
									"command": "whim.test.command_id_1",
									"keybind": ""
								}
							]
						}
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

	public static TheoryData<string, bool> UnifyKeyModifiersConfig =>
		new()
		{
			{
				"""
					keybinds:
					  unify_key_modifiers: true
					""",
				true
			},
			{
				"""
					keybinds:
					  unify_key_modifiers: false
					""",
				true
			},
			{
				"""
					{
						"keybinds": {
							"unify_key_modifiers": true
						}
					}
					""",
				false
			},
			{
				"""
					{
						"keybinds": {
							"unify_key_modifiers": false
						}
					}
					""",
				false
			},
		};

	[Theory, MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(UnifyKeyModifiersConfig))]
	public void Load_UnifyKeyModifiers(string config, bool isYaml, IContext ctx)
	{
		// Given a valid config with unifyKeyModifiers set
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(config);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and unifyKeyModifiers is set
		Assert.True(result);
		ctx.KeybindManager.Received().UnifyKeyModifiers = config.Contains("true");
	}
}
