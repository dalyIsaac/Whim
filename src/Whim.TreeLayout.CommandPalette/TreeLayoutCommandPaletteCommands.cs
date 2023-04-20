using System;
using System.Collections;
using System.Collections.Generic;
using Whim.CommandPalette;

namespace Whim.TreeLayout.CommandPalette;

/// <summary>
/// Commands to interact with the tree layout via the command palette.
/// </summary>
public class TreeLayoutCommandPalettePluginCommands : IEnumerable<CommandItem>
{
	private readonly IPlugin _treeLayoutCommandPalettePlugin;
	private readonly ITreeLayoutPlugin _treeLayoutPlugin;
	private readonly ICommandPalettePlugin _commandLayoutPlugin;

	private string Name => _treeLayoutCommandPalettePlugin.Name;

	private readonly Direction[] _directions = new Direction[]
	{
		Direction.Left,
		Direction.Right,
		Direction.Up,
		Direction.Down,
	};

	/// <summary>
	/// Creates a new instance of the tree layout's command palette commands.
	/// </summary>
	/// <param name="treeLayoutCommandPalettePlugin"></param>
	/// <param name="treeLayoutPlugin"></param>
	/// <param name="commandLayoutPlugin"></param>
	public TreeLayoutCommandPalettePluginCommands(
		IPlugin treeLayoutCommandPalettePlugin,
		ITreeLayoutPlugin treeLayoutPlugin,
		ICommandPalettePlugin commandLayoutPlugin
	)
	{
		_treeLayoutCommandPalettePlugin = treeLayoutCommandPalettePlugin;
		_treeLayoutPlugin = treeLayoutPlugin;
		_commandLayoutPlugin = commandLayoutPlugin;
	}

	internal CommandItem[] CreateSetDirectionCommandItems()
	{
		CommandItem[] setDirectionCommandItems = new CommandItem[_directions.Length];
		ITreeLayoutEngine? activeTreeLayoutEngine = _treeLayoutPlugin.GetTreeLayoutEngine();

		for (int i = 0; i < _directions.Length; i++)
		{
			Direction currentDirection = _directions[i];
			string currentStr = currentDirection.ToString();

			setDirectionCommandItems[i] = new CommandItem()
			{
				Command = new Command(
					identifier: $"{Name}.set_direction.{currentStr}",
					title: currentStr,
					callback: () => SetDirection(currentStr),
					condition: SetDirectionCondition
				)
			};
		}

		return setDirectionCommandItems;
	}

	private bool SetDirectionCondition() => _treeLayoutPlugin.GetTreeLayoutEngine() is not null;

	internal void SetDirection(string directionString)
	{
		if (!Enum.TryParse(directionString, out Direction directionEnum))
		{
			Logger.Error($"{Name}: Could not parse direction '{directionString}'.");
			return;
		}

		_treeLayoutPlugin.SetAddWindowDirection(directionEnum);
	}

	/// <summary>
	/// Set the direction of the tree layout, using a radio button command palette menu.
	/// </summary>
	public CommandItem SetDirectionCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.set_direction",
				title: "Set tree layout direction",
				callback: () =>
					_commandLayoutPlugin.Activate(
						new MenuVariantConfig()
						{
							Hint = "Select tree layout direction",
							Commands = CreateSetDirectionCommandItems(),
						}
					),
				condition: SetDirectionCondition
			)
		};

	/// <inheritdoc/>
	public IEnumerator<CommandItem> GetEnumerator()
	{
		yield return SetDirectionCommand;
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
