using AutoFixture;
using NSubstitute;
using Whim.CommandPalette;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.TreeLayout.CommandPalette.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class TreeLayoutCommandPaletteCommandsTests
{
	private class Customization : StoreCustomization
	{
		protected override void PostCustomize(IFixture fixture)
		{
			MutableRootSector root = _store._root.MutableRootSector;

			IMonitor monitor = CreateMonitor((HMONITOR)1234);
			fixture.Inject(monitor);
			AddMonitorsToSector(root, monitor);

			IPlugin treeLayoutCommandPalettePlugin = fixture.Freeze<IPlugin>();
			treeLayoutCommandPalettePlugin.Name.Returns("whim.tree_layout.command_palette");

			ITreeLayoutPlugin treeLayoutPlugin = fixture.Freeze<ITreeLayoutPlugin>();
			ICommandPalettePlugin commandPalettePlugin = fixture.Freeze<ICommandPalettePlugin>();

			TreeLayoutCommandPalettePluginCommands commands = new(
				_ctx,
				treeLayoutCommandPalettePlugin,
				treeLayoutPlugin,
				commandPalettePlugin
			);
			fixture.Inject(commands);
		}
	}

	[Theory, AutoSubstituteData<Customization>]
	public void SetDirectionCommand_Activates(
		ICommandPalettePlugin commandPalettePlugin,
		TreeLayoutCommandPalettePluginCommands commands,
		ITreeLayoutPlugin treeLayoutPlugin,
		IMonitor monitor
	)
	{
		// Given
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand(
			"whim.tree_layout.command_palette.set_direction"
		);
		treeLayoutPlugin.GetAddWindowDirection(monitor).Returns(Direction.Left);

		// When
		command.TryExecute();

		// Then
		commandPalettePlugin.Received(1).Activate(Arg.Any<MenuVariantConfig>());
	}

	[Theory, AutoSubstituteData<Customization>]
	public void SetDirectionCommand_Activates_Fails(
		ICommandPalettePlugin commandPalettePlugin,
		TreeLayoutCommandPalettePluginCommands commands,
		ITreeLayoutPlugin treeLayoutPlugin,
		IMonitor monitor
	)
	{
		// Given
		ICommand command = new TestUtils.PluginCommandsTestUtils(commands).GetCommand(
			"whim.tree_layout.command_palette.set_direction"
		);
		treeLayoutPlugin.GetAddWindowDirection(monitor).Returns((Direction?)null);

		// When
		command.TryExecute();

		// Then
		commandPalettePlugin.DidNotReceive().Activate(Arg.Any<MenuVariantConfig>());
	}

	[Theory, AutoSubstituteData<Customization>]
	public void CreateSetDirectionCommandItems(TreeLayoutCommandPalettePluginCommands commands)
	{
		// When
		ICommand[] commandItems = commands.CreateSetDirectionCommands();

		// Then
		Assert.Equal(4, commandItems.Length);
	}

	[InlineAutoSubstituteData<Customization>("Left", Direction.Left)]
	[InlineAutoSubstituteData<Customization>("Right", Direction.Right)]
	[InlineAutoSubstituteData<Customization>("Up", Direction.Up)]
	[InlineAutoSubstituteData<Customization>("Down", Direction.Down)]
	[Theory]
	public void SetDirection(
		string direction,
		Direction expectedDirection,
		TreeLayoutCommandPalettePluginCommands commands,
		ITreeLayoutPlugin treeLayoutPlugin,
		IMonitor monitor
	)
	{
		// When
		commands.SetDirection(direction);

		// Then
		treeLayoutPlugin.Received(1).SetAddWindowDirection(monitor, expectedDirection);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void SetDirectionCommand_FailsWhenNoActiveTreeLayoutEngine(
		TreeLayoutCommandPalettePluginCommands commands,
		ITreeLayoutPlugin treeLayoutPlugin,
		IMonitor monitor,
		MutableRootSector root
	)
	{
		// Given
		Workspace workspace = CreateWorkspace();
		PopulateMonitorWorkspaceMap(root, monitor, workspace);

		// When
		commands.SetDirection("welp");

		// Then
		treeLayoutPlugin.DidNotReceive().SetAddWindowDirection(Arg.Any<IMonitor>(), Arg.Any<Direction>());
	}
}
