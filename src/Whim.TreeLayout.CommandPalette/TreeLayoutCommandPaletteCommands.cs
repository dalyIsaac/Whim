using System;
using Whim.CommandPalette;

namespace Whim.TreeLayout.CommandPalette;

/// <summary>
/// Commands to interact with the tree layout via the command palette.
/// </summary>
public class TreeLayoutCommandPalettePluginCommands : PluginCommands
{
	private readonly IContext _context;
	private readonly ITreeLayoutPlugin _treeLayoutPlugin;

	private readonly Direction[] _directions = new Direction[]
	{
		Direction.Left,
		Direction.Right,
		Direction.Up,
		Direction.Down,
	};

	/// <summary>
	/// Create the tree layout command palette plugin commands.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="treeLayoutCommandPalettePlugin"></param>
	/// <param name="treeLayoutPlugin"></param>
	/// <param name="commandLayoutPlugin"></param>
	public TreeLayoutCommandPalettePluginCommands(
		IContext context,
		IPlugin treeLayoutCommandPalettePlugin,
		ITreeLayoutPlugin treeLayoutPlugin,
		ICommandPalettePlugin commandLayoutPlugin
	)
		: base(treeLayoutCommandPalettePlugin.Name)
	{
		_context = context;
		_treeLayoutPlugin = treeLayoutPlugin;

		Add(
			identifier: "set_direction",
			title: "Set tree layout direction",
			callback: () =>
				commandLayoutPlugin.Activate(
					new MenuVariantConfig()
					{
						Hint = "Select tree layout direction",
						Commands = CreateSetDirectionCommands(),
					}
				),
			condition: SetDirectionCondition
		);
	}

	internal ICommand[] CreateSetDirectionCommands()
	{
		ICommand[] setDirectionCommands = new ICommand[_directions.Length];

		for (int i = 0; i < _directions.Length; i++)
		{
			Direction currentDirection = _directions[i];
			string currentStr = currentDirection.ToString();

			setDirectionCommands[i] = new Command(
				identifier: $"set_direction.{currentStr}",
				title: currentStr,
				callback: () => SetDirection(currentStr),
				condition: SetDirectionCondition
			);
		}

		return setDirectionCommands;
	}

	private bool SetDirectionCondition() =>
		_treeLayoutPlugin.GetAddWindowDirection(_context.MonitorManager.FocusedMonitor) != null;

	internal void SetDirection(string directionString)
	{
		if (!Enum.TryParse(directionString, out Direction directionEnum))
		{
			Logger.Error($"Could not parse direction '{directionString}'.");
			return;
		}

		_treeLayoutPlugin.SetAddWindowDirection(_context.MonitorManager.FocusedMonitor, directionEnum);
	}
}
