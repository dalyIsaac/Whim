using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.TreeLayout;

/// <summary>
/// The commands for the tree layout plugin.
/// </summary>
public class TreeLayoutCommands : PluginCommands
{
	private readonly IContext _context;
	private readonly ITreeLayoutPlugin _treeLayoutPlugin;

	/// <summary>
	/// Creates a new instance of the tree layout commands.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="treeLayoutPlugin"></param>
	public TreeLayoutCommands(IContext context, ITreeLayoutPlugin treeLayoutPlugin)
		: base(treeLayoutPlugin.Name)
	{
		_context = context;
		_treeLayoutPlugin = treeLayoutPlugin;

		Add(
				identifier: "add_tree_direction_left",
				title: "Add windows to the left of the current window",
				callback: () =>
					_treeLayoutPlugin.SetAddWindowDirection(
						_context.Store.Pick(Pickers.PickActiveMonitor()),
						Direction.Left
					),
				keybind: new Keybind(IKeybind.WinCtrlShift, VIRTUAL_KEY.VK_LEFT)
			)
			.Add(
				identifier: "add_tree_direction_right",
				title: "Add windows to the right of the current window",
				callback: () =>
					_treeLayoutPlugin.SetAddWindowDirection(
						_context.Store.Pick(Pickers.PickActiveMonitor()),
						Direction.Right
					),
				keybind: new Keybind(IKeybind.WinCtrlShift, VIRTUAL_KEY.VK_RIGHT)
			)
			.Add(
				identifier: "add_tree_direction_up",
				title: "Add windows above the current window",
				callback: () =>
					_treeLayoutPlugin.SetAddWindowDirection(
						_context.Store.Pick(Pickers.PickActiveMonitor()),
						Direction.Up
					),
				keybind: new Keybind(IKeybind.WinCtrlShift, VIRTUAL_KEY.VK_UP)
			)
			.Add(
				identifier: "add_tree_direction_down",
				title: "Add windows below the current window",
				callback: () =>
					_treeLayoutPlugin.SetAddWindowDirection(
						_context.Store.Pick(Pickers.PickActiveMonitor()),
						Direction.Down
					),
				keybind: new Keybind(IKeybind.WinCtrlShift, VIRTUAL_KEY.VK_DOWN)
			);
	}
}
