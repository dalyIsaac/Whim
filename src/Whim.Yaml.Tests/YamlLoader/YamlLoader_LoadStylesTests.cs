using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoader_LoadStylesTests
{
	public static TheoryData<string, bool> ValidStylesConfig =>
		new()
		{
			{
				"""
					styles:
					  user_dictionaries:
					    - "path/to/dict1.xaml"
					    - "path/to/dict2.xaml"
					""",
				true
			},
			{
				"""
					{
					    "styles": {
					        "user_dictionaries": [
					            "path/to/dict1.xaml",
					            "path/to/dict2.xaml"
					        ]
					    }
					}
					""",
				false
			},
		};

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(ValidStylesConfig))]
	public void Load_ValidStyles_AddsUserDictionaries(string config, bool isYaml, IContext ctx)
	{
		// Given
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);
		ctx.FileManager.FileExists(Arg.Is<string>(p => p.StartsWith("path"))).Returns(true);

		// When
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then
		Assert.True(result);
		ctx.ResourceManager.Received(2).AddUserDictionary(Arg.Any<string>());
	}

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(ValidStylesConfig))]
	public void Load_FallbackStyles_AddsUserDictionaries(string config, bool isYaml, IContext ctx)
	{
		// Given
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);
		ctx.FileManager.WhimDir.Returns("C:/Users/username/.whim");
		ctx.FileManager.FileExists(Arg.Is<string>(p => p.StartsWith("path"))).Returns(false);
		ctx.FileManager.FileExists(Arg.Is<string>(p => p.StartsWith("C:/Users/username/.whim"))).Returns(true);

		// When
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then
		Assert.True(result);
		ctx.ResourceManager.Received(2).AddUserDictionary(Arg.Any<string>());
	}

	public static TheoryData<string, bool> NoStylesConfig =>
		new()
		{
			{
				"""
					styles:
					  user_dictionaries: []
					""",
				true
			},
			{
				"""
					{
					    "styles": {
					        "user_dictionaries": []
					    }
					}
					""",
				false
			},
		};

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(NoStylesConfig))]
	public void Load_NoStyles_DoesNotAddUserDictionaries(string config, bool isYaml, IContext ctx)
	{
		// Given
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then
		Assert.True(result);
		ctx.ResourceManager.DidNotReceive().AddUserDictionary(Arg.Any<string>());
	}

	public static TheoryData<string, bool> InvalidStylesConfig =>
		new()
		{
			{
				"""
					styles:
					  user_dictionaries: "path/to/dict.xaml"
					""",
				true
			},
			{
				"""
					{
					    "styles": {
					        "user_dictionaries": "path/to/dict.xaml"
					    }
					}
					""",
				false
			},
		};

	[Theory]
	[MemberAutoSubstituteData<YamlLoaderCustomization>(nameof(InvalidStylesConfig))]
	public void Load_InvalidStyles_DoesNotAddUserDictionaries(string config, bool isYaml, IContext ctx)
	{
		// Given
		YamlLoaderTestUtils.SetupFileConfig(ctx, config, isYaml);

		// When
		bool result = YamlLoader.Load(ctx, showErrorWindow: false);

		// Then
		Assert.True(result);
		ctx.ResourceManager.DidNotReceive().AddUserDictionary(Arg.Any<string>());
	}
}
