using NSubstitute;

namespace Whim.Yaml.Tests;

public static class YamlLoaderTestUtils
{
	public static void SetupFileConfig(IContext ctx, string config, bool isYaml)
	{
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(config);
	}
}
