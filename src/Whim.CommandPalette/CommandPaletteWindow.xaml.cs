using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace Whim.CommandPalette;

public sealed partial class CommandPaletteWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly CommandPalettePlugin _plugin;

	private readonly IWindow _window;
	private IMonitor? _monitor;
	public bool IsVisible => _monitor != null;

	private ItemCollection _matches => ListViewItems.Items;

	/// <summary>
	/// The current commands from which <see cref="Matches"/> is derived.
	/// </summary>
	private readonly List<CommandPaletteMatch> _allCommands = new();

	public CommandPaletteWindow(IConfigContext configContext, CommandPalettePlugin plugin)
	{
		_configContext = configContext;
		_plugin = plugin;
		_window = this.InitializeBorderlessWindow("Whim.CommandPalette", "CommandPaletteWindow", _configContext);

		Title = CommandPaletteConfig.Title;
	}

	public void Activate(IEnumerable<(ICommand, IKeybind?)>? items = null, IMonitor? monitor = null)
	{
		Logger.Debug("Activating command palette");
		TextEntry.Text = "";
		_allCommands.Clear();
		foreach ((ICommand command, IKeybind? keybind) in items ?? _configContext.CommandManager)
		{
			if (command.CanExecute())
			{
				_allCommands.Add(new CommandPaletteMatch(command, keybind));
			}
		}

		UpdateMatches();

		monitor ??= _configContext.MonitorManager.FocusedMonitor;
		if (monitor == _monitor)
		{
			return;
		}

		_monitor = monitor;

		int width = 800;
		int height = 800;

		ILocation<int> windowLocation = new Location(
			x: monitor.X + (monitor.Width / 2) - (width / 2),
			y: monitor.Y + (height / 4),
			width: width,
			height: height
		);

		Activate();
		TextEntry.Focus(FocusState.Programmatic);
		Win32Helper.SetWindowPos(
			new WindowLocation(_window, windowLocation, WindowState.Normal),
			_window.Handle
		);
		_window.FocusForceForeground();
	}

	public void Hide()
	{
		Logger.Debug("Hiding command palette");
		_window.Hide();
		_monitor = null;
		TextEntry.Text = "";
		_allCommands.Clear();
	}

	public void Toggle()
	{
		Logger.Debug("Toggling command palette");
		if (IsVisible)
		{
			Hide();
		}
		else
		{
			Activate();
		}
	}

	private void TextEntry_KeyDown(object _sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		Logger.Debug("Command palette key down: {0}", e.Key.ToString());
		int selectedIndex = ListViewItems.SelectedIndex;
		switch (e.Key)
		{
			case Windows.System.VirtualKey.Down:
				ListViewItems.SelectedIndex = (selectedIndex + 1) % _matches.Count;
				break;
			case Windows.System.VirtualKey.Up:
				ListViewItems.SelectedIndex = (selectedIndex + _matches.Count - 1) % _matches.Count;
				break;
			case Windows.System.VirtualKey.Enter:
				ExecuteCommand();
				break;
			case Windows.System.VirtualKey.Escape:
				Hide();
				break;
			default:
				break;
		}
	}

	private void TextEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		UpdateMatches();
	}

	private void UpdateMatches()
	{
		Logger.Debug("Updating command palette matches");
		string query = TextEntry.Text;
		int idx = 0;

		foreach (CommandPaletteMatch match in _plugin.Config.Matcher.Match(query, _allCommands))
		{
			if (idx < _matches.Count)
			{
				_matches[idx] = new PaletteItem(match);
			}
			else
			{
				_matches.Add(new PaletteItem(match));
			}
			idx++;
		}

		// If there are more matches than we have space for, remove the last ones.
		int count = _matches.Count;
		for (; idx < count; idx++)
		{
			_matches.RemoveAt(_matches.Count - 1);
		}

		Logger.Debug($"Command palette match count: {_matches.Count}");

		ListViewItems.SelectedIndex = _matches.Count > 0 ? 0 : -1;
	}

	private void CommandListItems_ItemClick(object sender, ItemClickEventArgs e)
	{
		Logger.Debug("Command palette item clicked");
		ListViewItems.SelectedItem = e.ClickedItem;
		ExecuteCommand();
	}

	private void ExecuteCommand()
	{
		Logger.Debug("Executing command");
		if (ListViewItems.SelectedIndex < 0)
		{
			Hide();
		}

		if (_matches[ListViewItems.SelectedIndex] is PaletteItem item)
		{
			CommandPaletteMatch match = item.Match;

			match.Command.TryExecute();
			_plugin.Config.Matcher.OnMatchExecuted(match);
			Hide();
		}
	}
}
