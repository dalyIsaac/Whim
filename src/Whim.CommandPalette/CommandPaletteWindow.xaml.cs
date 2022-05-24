using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whim.CommandPalette;

public sealed partial class CommandPaletteWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly CommandPalettePlugin _plugin;

	private readonly IWindow _window;
	private IMonitor? _monitor;
	public bool IsVisible => _monitor != null;

	private readonly ObservableCollection<PaletteRow> _paletteRows = new();

	/// <summary>
	/// The current commands from which <see cref="Matches"/> is derived.
	/// </summary>
	private readonly List<Match> _allCommands = new();

	public CommandPaletteWindow(IConfigContext configContext, CommandPalettePlugin plugin)
	{
		_configContext = configContext;
		_plugin = plugin;
		_window = this.InitializeBorderlessWindow("Whim.CommandPalette", "CommandPaletteWindow", _configContext);

		Title = CommandPaletteConfig.Title;
		ListViewItems.ItemsSource = _paletteRows;

		// Populate the commands to reduce the first render time.
		Populate();
		UpdateMatches();
	}

	/// <summary>
	/// Populate <see cref="_allCommands"/> with all the current commands.
	/// </summary>
	private void Populate(IEnumerable<(ICommand, IKeybind?)>? items = null)
	{
		Logger.Debug($"Populating the current list of all commands.");
		int idx = 0;
		foreach ((ICommand command, IKeybind? keybind) in items ?? _configContext.CommandManager)
		{
			if (!command.CanExecute())
			{
				continue;
			}

			if (idx < _allCommands.Count)
			{
				if (_allCommands[idx].Command != command)
				{
					_allCommands[idx] = new Match(command, keybind);
				}
			}
			else
			{
				_allCommands.Add(new Match(command, keybind));
			}

			idx++;
		}

		for (; idx < _allCommands.Count; idx++)
		{
			_allCommands.RemoveAt(_allCommands.Count - 1);
		}
	}

	public void Activate(IEnumerable<(ICommand, IKeybind?)>? items = null, IMonitor? monitor = null)
	{
		Logger.Debug("Activating command palette");

		monitor ??= _configContext.MonitorManager.FocusedMonitor;
		if (monitor == _monitor)
		{
			return;
		}
		_monitor = monitor;
		TextEntry.Text = "";

		Populate(items);
		UpdateMatches();

		int width = 800;
		int height = 800;

		ILocation<int> windowLocation = new Location(
			x: monitor.X + (monitor.Width / 2) - (width / 2),
			y: monitor.Y + (height / 4),
			width: width,
			height: height
		);

		base.Activate();
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

	private void TextEntry_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		Logger.Debug("Command palette key down: {0}", e.Key.ToString());
		int selectedIndex = ListViewItems.SelectedIndex;
		switch (e.Key)
		{
			case Windows.System.VirtualKey.Down:
				ListViewItems.SelectedIndex = (selectedIndex + 1) % _paletteRows.Count;
				break;
			case Windows.System.VirtualKey.Up:
				ListViewItems.SelectedIndex = (selectedIndex + _paletteRows.Count - 1) % _paletteRows.Count;
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

		foreach (PaletteItem item in _plugin.Config.Matcher.Match(query, _allCommands))
		{
			Logger.Verbose($"Matched {item.Match.Command.Title}");
			if (idx < _paletteRows.Count)
			{
				_paletteRows[idx].Update(item);
			}
			else
			{
				Logger.Verbose("1");
				PaletteRow row = new(item);
				Logger.Verbose("2");
				_paletteRows.Add(row);
				Logger.Verbose("3");
				row.Initialize();
				Logger.Verbose("4");
			}
			idx++;

			Logger.Verbose($"Finished updating {item.Match.Command.Title}");
		}

		// If there are more items than we have space for, remove the last ones.
		int count = _paletteRows.Count;
		for (; idx < count; idx++)
		{
			_paletteRows.RemoveAt(_paletteRows.Count - 1);
		}

		Logger.Debug($"Command palette match count: {_paletteRows.Count}");

		ListViewItems.SelectedIndex = _paletteRows.Count > 0 ? 0 : -1;
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

		Match match = _paletteRows[ListViewItems.SelectedIndex].Model.Match;

		match.Command.TryExecute();
		_plugin.Config.Matcher.OnMatchExecuted(match);
		Hide();
	}
}
