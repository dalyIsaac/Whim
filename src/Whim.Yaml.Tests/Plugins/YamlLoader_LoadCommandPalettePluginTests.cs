using NSubstitute;
using Whim.CommandPalette;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlPluginLoader_LoadCommandPalettePluginTests
{
	private const int _defaultMaxHeightPercent = 40;
	private const int _defaultMaxWidthPixels = 800;
	private const int _defaultYPositionPercent = 20;

	public static TheoryData<string, bool, int, int, int> CommandPaletteConfig =>
		new()
		{
			// YAML, all values set
			{
				"""
					plugins:
					  command_palette:
					    is_enabled: true
					    max_height_percent: 10
					    max_width_pixels: 20
					    y_position_percent: 30
					""",
				true,
				10,
				20,
				30
			},
			// JSON, all values set
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"is_enabled": true,
								"max_height_percent": 10,
								"max_width_pixels": 20,
								"y_position_percent": 30
							}
						}
					}
					""",
				false,
				10,
				20,
				30
			},
			// YAML, no values set
			{
				"""
					plugins:
					  command_palette: {}
					""",
				true,
				_defaultMaxHeightPercent,
				_defaultMaxWidthPixels,
				_defaultYPositionPercent
			},
			// JSON, no values set
			{
				"""
						{
							"plugins": {
								"command_palette": {}
							}
						}
					""",
				false,
				_defaultMaxHeightPercent,
				_defaultMaxWidthPixels,
				_defaultYPositionPercent
			},
			// YAML, is_enabled not set
			{
				"""
					plugins:
					  command_palette:
					    max_height_percent: 10
					    max_width_pixels: 20
					    y_position_percent: 30
					""",
				true,
				10,
				20,
				30
			},
			// JSON, is_enabled not set
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"max_height_percent": 10,
								"max_width_pixels": 20,
								"y_position_percent": 30
							}
						}
					}
					""",
				false,
				10,
				20,
				30
			},
		};

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(CommandPaletteConfig))]
	public void LoadCommandPalettePlugin_ConfigIsSet_PluginIsLoaded(
		string schema,
		bool isYaml,
		int maxHeightPercent,
		int maxWidthPixels,
		int yPositionPercent,
		IContext ctx
	)
	{
		// Given a valid config with the command palette plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the command palette plugin is set
		Assert.True(result);
		ctx.PluginManager.Received(1)
			.AddPlugin(
				Arg.Is<CommandPalettePlugin>(p =>
					p.Config.MaxHeightPercent == maxHeightPercent
					&& p.Config.MaxWidthPixels == maxWidthPixels
					&& p.Config.YPositionPercent == yPositionPercent
				)
			);
	}

	public static TheoryData<string, bool, BackdropType, bool> CommandPalettePluginBackdropConfig =>
		new()
		{
			// YAML, backdrop acrylic, always show
			{
				"""
					plugins:
					  command_palette:
					    backdrop:
					      type: acrylic
					      always_show_backdrop: true
					""",
				true,
				BackdropType.Acrylic,
				true
			},
			// JSON, backdrop acrylic, always show
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"backdrop": {
									"type": "acrylic",
									"always_show_backdrop": true
								}
							}
						}
					}
					""",
				false,
				BackdropType.Acrylic,
				true
			},
			// YAML, backdrop acrylic thin, always show
			{
				"""
					plugins:
					  command_palette:
					    backdrop:
					      type: acrylic_thin
					      always_show_backdrop: true
					""",
				true,
				BackdropType.AcrylicThin,
				true
			},
			// JSON, backdrop acrylic thin, always show
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"backdrop": {
									"type": "acrylic_thin",
									"always_show_backdrop": true
								}
							}
						}
					}
					""",
				false,
				BackdropType.AcrylicThin,
				true
			},
			// YAML, backdrop mica, do not always show
			{
				"""
					plugins:
					  command_palette:
					    backdrop:
					      type: mica
					      always_show_backdrop: false
					""",
				true,
				BackdropType.Mica,
				false
			},
			// JSON, backdrop mica, do not always show
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"backdrop": {
									"type": "mica",
									"always_show_backdrop": false
								}
							}
						}
					}
					""",
				false,
				BackdropType.Mica,
				false
			},
			// YAML, backdrop mica alt, default always show
			{
				"""
					plugins:
					  command_palette:
					    backdrop:
					      type: mica_alt
					""",
				true,
				BackdropType.MicaAlt,
				true
			},
			// JSON, backdrop mica alt, default always show
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"backdrop": {
									"type": "mica_alt"
								}
							}
						}
					}
					""",
				false,
				BackdropType.MicaAlt,
				true
			},
			// YAML, backdrop none, always show
			{
				"""
					plugins:
					  command_palette:
					    backdrop:
					      type: none
					      always_show_backdrop: true
					""",
				true,
				BackdropType.None,
				true
			},
			// JSON, backdrop none, always show
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"backdrop": {
									"type": "none",
									"always_show_backdrop": true
								}
							}
						}
					}
					""",
				false,
				BackdropType.None,
				true
			},
			// YAML, nothing specified
			{
				"""
					plugins:
					  command_palette: {}
					""",
				true,
				BackdropType.Mica,
				true
			},
			// JSON, nothing specified
			{
				"""
					{
						"plugins": {
							"command_palette": {}
						}
					}
					""",
				false,
				BackdropType.Mica,
				true
			},
		};

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(CommandPalettePluginBackdropConfig))]
	public void LoadCommandPalettePlugin_ConfigWithBackdrop_PluginIsLoaded(
		string schema,
		bool isYaml,
		BackdropType backdropType,
		bool alwaysShowBackdrop,
		IContext ctx
	)
	{
		// Given a valid config with the command palette plugin and backdrop
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the command palette plugin is set
		Assert.True(result);
		ctx.PluginManager.Received(1)
			.AddPlugin(
				Arg.Is<CommandPalettePlugin>(p =>
					p.Config.Backdrop.Backdrop == backdropType
					&& p.Config.Backdrop.AlwaysShowBackdrop == alwaysShowBackdrop
				)
			);
	}

	public static TheoryData<string, bool> CommandPalettePluginNotEnabledConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  command_palette:
					    is_enabled: false
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"is_enabled": false
							}
						}
					}
					""",
				false
			},
		};

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(CommandPalettePluginNotEnabledConfig))]
	public void LoadCommandPalettePlugin_ConfigIsNotEnabled_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given a valid config with the command palette plugin not enabled
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the command palette plugin is not set
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<CommandPalettePlugin>());
	}

	public static TheoryData<string, bool> CommandPalettePluginInvalidConfig =>
		new()
		{
			// YAML
			{
				"""
					plugins:
					  command_palette:
					    max_height_percent: 123.5
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"max_height_percent": 123.5
							}
						}
					}
					""",
				false
			},
			// YAML
			{
				"""
					plugins:
					  command_palette:
					    is_enabled: "very true"
					""",
				true
			},
			// JSON
			{
				"""
					{
						"plugins": {
							"command_palette": {
								"is_enabled": "very true"
							}
						}
					}
					""",
				false
			},
		};

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(CommandPalettePluginInvalidConfig))]
	public void LoadCommandPalettePlugin_ConfigIsInvalid_PluginIsNotLoaded(string schema, bool isYaml, IContext ctx)
	{
		// Given an invalid config with the command palette plugin
		YamlLoaderTestUtils.SetupFileConfig(ctx, schema, isYaml);

		// When loading the config
		bool result = YamlLoader.Load(ctx);

		// Then the result is true, and the command palette plugin is not set
		Assert.True(result);
		ctx.PluginManager.DidNotReceive().AddPlugin(Arg.Any<CommandPalettePlugin>());
	}
}
