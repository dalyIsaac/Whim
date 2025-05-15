using System.Collections.Immutable;
using NSubstitute;

namespace Whim.Yaml.Tests;

public static class YamlLoaderTestUtils
{
	public static void SetupFileConfig(IContext ctx, string config, bool isYaml)
	{
		ctx.FileManager.FileExists(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Is<string>(s => s.EndsWith(isYaml ? "yaml" : "json"))).Returns(config);
	}

	public static ILayoutEngine[]? GetLayoutEngines(IContext ctx)
	{
		var arguments = ctx.Store.ReceivedCalls().FirstOrDefault()?.GetArguments();
		if (arguments is null)
		{
			return null;
		}

		SetCreateLayoutEnginesTransform? transform = (SetCreateLayoutEnginesTransform?)arguments[0];
		return transform?.CreateLayoutEnginesFn().Select(x => x(new())).ToArray();
	}

	public static IWorkspace[]? GetWorkspaces(IContext ctx)
	{
		List<IWorkspace> workspaces = [];

		foreach (var call in ctx.Store.ReceivedCalls())
		{
			var arguments = call.GetArguments();
			if (arguments.Length == 1 && arguments[0] is AddWorkspaceTransform transform)
			{
				IWorkspace workspace = Substitute.For<IWorkspace>();

				workspace.Id.Returns(transform.WorkspaceId);

#pragma warning disable CS0618 // Type or member is obsolete
				workspace.Name.Returns(transform.Name);
#pragma warning restore CS0618 // Type or member is obsolete

				var engines = transform.CreateLeafLayoutEngines?.Select(c => c(new()));
				workspace.LayoutEngines.Returns(engines?.ToImmutableList() ?? []);

				workspaces.Add(workspace);
			}
		}

		return [.. workspaces];
	}
}
