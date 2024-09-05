using System.Text.Json;
using System.Text.Json.Nodes;
using Whim.TestUtils;
using Xunit;
using Yaml2JsonNode;
using YamlDotNet.RepresentationModel;

namespace Whim.Json.Tests;

public class SampleTests
{
	[Fact]
	public void TestSample()
	{
		var schema = Sample.GetSample();
		Assert.NotNull(schema);
	}

	[Fact]
	public void TestSampleYaml()
	{
		// Read the YAML file from sample.yaml
		using StreamReader stringReader = new("C:\\Users\\dalyisaac\\Repos\\Whim\\src\\Whim.Json.Tests\\sample.yaml");
		YamlStream stream = [];

		stream.Load(stringReader);
		JsonNode? root = stream.ToJsonNode().First();
		JsonElement element = JsonSerializer.Deserialize<JsonElement>(root);

		// Convert the nodes to the schema.
		var schema = Schema.FromJson(element);
		Assert.NotNull(schema);
	}
}
