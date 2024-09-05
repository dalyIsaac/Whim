using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using Yaml2JsonNode;
using YamlDotNet.RepresentationModel;

namespace Whim.Json.Tests;

public class SampleTests
{
	private const string JsonSchema = """
		{
			"workspaces": [
				{
					"name": "Browser",
					"layoutEngines": [
						{
							"type": "FocusLayoutEngine"
						},
						{
							"type": "SliceLayoutEngine",
							"variant": "ColumnLayout"
						}
					]
				},
				{
					"name": "Chat"
				},
				{
					"name": "Code"
				},
				{
					"name": "Music"
				}
			],
			"layoutEngines": [
				{
					"type": "SliceLayoutEngine",
					"variant": "ColumnLayout"
				},
				{
					"type": "SliceLayoutEngine",
					"variant": "PrimaryStackLayout"
				}
			],
			"monitors": [
				{
					"workspaces": [
						"Browser",
						"Chat",
						"Music"
					]
				},
				{
					"workspaces": [
						"Code"
					]
				},
				{
					"workspaces": [
						"*"
					]
				}
			],
			"keybinds": [
				{
					"command": "whim.core.activate_previous_workspace",
					"keybind": "LWin+LCtrl+Left"
				}
			],
			"filters": [
				{
					"type": "title",
					"value": "whim"
				}
			],
			"routers": [
				{
					"type": "processFileName",
					"value": "Discord.exe",
					"workspace": "Chat"
				}
			],
			"plugins": [
				{
					"type": "GapsPlugin",
					"isEnabled": true,
					"outerGapSize": 10
				},
				{
					"type": "FocusIndicatorPlugin",
					"isEnabled": true,
					"color": "#ff0000",
					"borderSize": 2
				},
				{
					"type": "BarPlugin",
					"isEnabled": true,
					"leftComponents": [
						{
							"type": "DateTimeWidget",
							"format": "HH:mm",
							"interval": 1000
						}
					]
				}
			]
		}
		""";

	[Fact]
	public void TestSample()
	{
		Schema schema = Schema.Parse(JsonSchema);
		Assert.NotNull(schema);
	}

	private const string YamlSchema = """
		# yaml-language-server: $schema=./schema.json
		workspaces:
		- name: Browser
			layoutEngines:
			- type: FocusLayoutEngine
			- type: SliceLayoutEngine
				variant: ColumnLayout
		- name: Chat
		- name: Code
		- name: Music

		layoutEngines:
		- type: SliceLayoutEngine
			variant: ColumnLayout
		- type: SliceLayoutEngine
			variant: PrimaryStackLayout

		monitors:
		- workspaces:
			- Browser
			- Chat
			- Music
		- workspaces:
			- Code
		- workspaces:
			- "*"

		keybinds:
		- command: whim.core.activate_previous_workspace
			keybind: LWin+LCtrl+Left

		filters:
		- type: title
			value: whim

		routers:
		- type: processFileName
			value: Discord.exe
			workspace: Chat

		plugins:
		- type: GapsPlugin
			isEnabled: true
			outerGapSize: 10

		- type: FocusIndicatorPlugin
			isEnabled: true
			color: "#ff0000"
			borderSize: 2

		- type: BarPlugin
			isEnabled: true
			leftComponents:
			- type: DateTimeWidget
				format: HH:mm
				interval: 1000
		""";

	[Fact]
	public void TestSampleYaml()
	{
		// Read the YAML string.
		YamlStream stream = [];

		stream.Load(new StringReader(YamlSchema));
		JsonNode? root = stream.ToJsonNode().First();
		JsonElement element = JsonSerializer.Deserialize<JsonElement>(root);

		// Convert the nodes to the schema.
		var schema = Schema.FromJson(element);
		Assert.NotNull(schema);
	}
}
