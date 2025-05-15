using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadKeybindsTests
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
					    - command: whim.test.command_id_4
					      keybind: LWin + LShift + LCtrl + D
					    - command: whim.test.command_id_5
					      keybind: RWin + RAlt + E
					    - command: whim.test.command_id_6
					      keybind: LShift + Space + F
					    - command: whim.test.command_id_7
					      keybind: RAlt + Tab + G
					    - command: whim.test.command_id_8
					      keybind: LWin + Escape + H
					    - command: whim.test.command_id_9
					      keybind: OEM_Comma + OEM_Period + I
					    - command: whim.test.command_id_10
					      keybind: F1 + F2 + J
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
					            },
					            {
					                "command": "whim.test.command_id_4",
					                "keybind": "LWin + LShift + LCtrl + D"
					            },
					            {
					                "command": "whim.test.command_id_5",
					                "keybind": "RWin + RAlt + E"
					            },
					            {
					                "command": "whim.test.command_id_6",
					                "keybind": "LShift + Space + F"
					            },
					            {
					                "command": "whim.test.command_id_7",
					                "keybind": "RAlt + Tab + G"
					            },
					            {
					                "command": "whim.test.command_id_8",
					                "keybind": "LWin + Escape + H"
					            },
					            {
					                "command": "whim.test.command_id_9",
					                "keybind": "OEM_Comma + OEM_Period + I"
					            },
					            {
					                "command": "whim.test.command_id_10",
					                "keybind": "F1 + F2 + J"
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
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

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

		// Non-standard combinations from KeybindHookTests
		ctx.KeybindManager.Received()
			.SetKeybind(
				"whim.test.command_id_4",
				new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LControl, VIRTUAL_KEY.VK_D)
			);
		ctx.KeybindManager.Received()
			.SetKeybind("whim.test.command_id_5", new Keybind(KeyModifiers.RWin | KeyModifiers.RAlt, VIRTUAL_KEY.VK_E));

		// Non-standard with regular keys as modifiers
		ctx.KeybindManager.Received()
			.SetKeybind(
				"whim.test.command_id_6",
				new Keybind([VIRTUAL_KEY.VK_LSHIFT, VIRTUAL_KEY.VK_SPACE], VIRTUAL_KEY.VK_F)
			);
		ctx.KeybindManager.Received()
			.SetKeybind(
				"whim.test.command_id_7",
				new Keybind([VIRTUAL_KEY.VK_RMENU, VIRTUAL_KEY.VK_TAB], VIRTUAL_KEY.VK_G)
			);
		ctx.KeybindManager.Received()
			.SetKeybind(
				"whim.test.command_id_8",
				new Keybind([VIRTUAL_KEY.VK_LWIN, VIRTUAL_KEY.VK_ESCAPE], VIRTUAL_KEY.VK_H)
			);
		ctx.KeybindManager.Received()
			.SetKeybind(
				"whim.test.command_id_9",
				new Keybind([VIRTUAL_KEY.VK_OEM_COMMA, VIRTUAL_KEY.VK_OEM_PERIOD], VIRTUAL_KEY.VK_I)
			);
		ctx.KeybindManager.Received()
			.SetKeybind(
				"whim.test.command_id_10",
				new Keybind([VIRTUAL_KEY.VK_F1, VIRTUAL_KEY.VK_F2], VIRTUAL_KEY.VK_J)
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
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

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
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then the result is true, and unifyKeyModifiers is set
		Assert.True(result);
		ctx.KeybindManager.Received().UnifyKeyModifiers = config.Contains("true");
	}
}
