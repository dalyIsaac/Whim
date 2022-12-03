using System.Collections;
using System.Collections.Generic;

namespace Whim.TreeLayout;

/// <summary>
/// The commands for the tree layout plugin.
/// </summary>
public class TreeLayoutCommands : IEnumerable<CommandItem>
{
	private readonly ITreeLayoutPlugin _plugin;
	private string Name => _plugin.Name;

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeLayoutCommands"/> class.
	/// </summary>
	/// <param name="plugin">The plugin.</param>
	public TreeLayoutCommands(ITreeLayoutPlugin plugin)
	{
		_plugin = plugin;
	}

	/// <inheritdoc/>
	public IEnumerator<CommandItem> GetEnumerator()
	{
		yield return AddTreeDirectionLeftCommand;
		yield return AddTreeDirectionRightCommand;
		yield return AddTreeDirectionUpCommand;
		yield return AddTreeDirectionDownCommand;
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Add tree direction left command.
	/// </summary>
	public CommandItem AddTreeDirectionLeftCommand =>
		new(
			new Command(
				identifier: $"{Name}.add_tree_direction_left",
				title: "Add windows to the left of the current window",
				callback: () => _plugin.SetAddWindowDirection(Direction.Left)
			)
		);

	/// <summary>
	/// Add tree direction right command.
	/// </summary>
	public CommandItem AddTreeDirectionRightCommand =>
		new(
			new Command(
				identifier: $"{Name}.add_tree_direction_right",
				title: "Add windows to the right of the current window",
				callback: () => _plugin.SetAddWindowDirection(Direction.Right)
			)
		);

	/// <summary>
	/// Add tree direction up command.
	/// </summary>
	public CommandItem AddTreeDirectionUpCommand =>
		new(
			new Command(
				identifier: $"{Name}.add_tree_direction_up",
				title: "Add windows above the current window",
				callback: () => _plugin.SetAddWindowDirection(Direction.Up)
			)
		);

	/// <summary>
	/// Add tree direction down command.
	/// </summary>
	public CommandItem AddTreeDirectionDownCommand =>
		new(
			new Command(
				identifier: $"{Name}.add_tree_direction_down",
				title: "Add windows below the current window",
				callback: () => _plugin.SetAddWindowDirection(Direction.Down)
			)
		);
}
