using NSubstitute;

namespace Whim.Yaml.Tests;

public static class YamlLoaderTestUtils
{
	public static void SetupFileConfig(IContext ctx, string config, bool isYaml)
	{
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(config);
	}

	public static ILayoutEngine[] GetLayoutEngines(IContext ctx)
	{
		SetCreateLayoutEnginesTransform transform = (SetCreateLayoutEnginesTransform)
			ctx.Store.ReceivedCalls().First().GetArguments()[0]!;
		return transform.CreateLayoutEnginesFn().Select(x => x(new())).ToArray();
	}
}
