using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using NSubstitute;
using Whim.CommandPalette;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.CommandPalette.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
internal class TreeLayoutCommandPaletteCommandsCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();
		IMonitor monitor = fixture.Freeze<IMonitor>();

		ctx.MonitorManager.ActiveMonitor.Returns(monitor);

		IPlugin treeLayoutCommandPalettePlugin = fixture.Freeze<IPlugin>();
		treeLayoutCommandPalettePlugin.Name.Returns("whim.tree_layout.command_palette");

		ITreeLayoutPlugin treeLayoutPlugin = fixture.Freeze<ITreeLayoutPlugin>();
		ICommandPalettePlugin commandPalettePlugin = fixture.Freeze<ICommandPalettePlugin>();

		TreeLayoutCommandPalettePluginCommands commands =
			new(ctx, treeLayoutCommandPalettePlugin, treeLayoutPlugin, commandPalettePlugin);
		fixture.Inject(commands);

		// Set up the store.
		Store store = new(ctx, internalCtx);
		ctx.Store.Returns(store);

		fixture.Inject(store._root);
		fixture.Inject(store._root.MutableRootSector);
	}
}

public class TreeLayoutCommandPaletteCommandsTests
{
	[Theory, AutoSubstituteData<TreeLayoutCommandPaletteCommandsCustomization>]
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

	[Theory, AutoSubstituteData<TreeLayoutCommandPaletteCommandsCustomization>]
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

	[Theory, AutoSubstituteData<TreeLayoutCommandPaletteCommandsCustomization>]
	public void CreateSetDirectionCommandItems(TreeLayoutCommandPalettePluginCommands commands)
	{
		// When
		ICommand[] commandItems = commands.CreateSetDirectionCommands();

		// Then
		Assert.Equal(4, commandItems.Length);
	}

	[InlineAutoSubstituteData<TreeLayoutCommandPaletteCommandsCustomization>("Left", Direction.Left)]
	[InlineAutoSubstituteData<TreeLayoutCommandPaletteCommandsCustomization>("Right", Direction.Right)]
	[InlineAutoSubstituteData<TreeLayoutCommandPaletteCommandsCustomization>("Up", Direction.Up)]
	[InlineAutoSubstituteData<TreeLayoutCommandPaletteCommandsCustomization>("Down", Direction.Down)]
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

	[Theory, AutoSubstituteData<TreeLayoutCommandPaletteCommandsCustomization>]
	internal void SetDirectionCommand_FailsWhenNoActiveTreeLayoutEngine(
		TreeLayoutCommandPalettePluginCommands commands,
		ITreeLayoutPlugin treeLayoutPlugin,
		IMonitor monitor,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		mutableRootSector.Maps.MonitorWorkspaceMap = mutableRootSector.Maps.MonitorWorkspaceMap.SetItem(
			monitor,
			workspace
		);
		workspace.ActiveLayoutEngine.Returns((ILayoutEngine?)null);

		// When
		commands.SetDirection("welp");

		// Then
		treeLayoutPlugin.DidNotReceive().SetAddWindowDirection(Arg.Any<IMonitor>(), Arg.Any<Direction>());
	}
}
