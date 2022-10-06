using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whim.CommandPalette;

internal sealed partial class CommandPaletteWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly CommandPalettePlugin _plugin;

	private readonly IWindow _window;
	private IMonitor? _monitor;
	public bool IsVisible => _monitor != null;

	private readonly ObservableCollection<PaletteRow> _paletteRows = new();
	private readonly List<PaletteRow> _unusedRows = new();

	/// <summary>
	/// The current commands from which the matches shown in <see cref="ListViewItems"/> are drawn.
	/// </summary>
	private readonly List<CommandItem> _allCommands = new();

	private CommandPaletteActivationConfig? _activationConfig;
	private CommandPaletteFreeTextCallback? _freeTextCallback;

	public CommandPaletteWindow(IConfigContext configContext, CommandPalettePlugin plugin)
	{
		_configContext = configContext;
		_plugin = plugin;
		_window = this.InitializeBorderlessWindow("Whim.CommandPalette", "CommandPaletteWindow", _configContext);

		Title = CommandPaletteConfig.Title;
		ListViewItems.ItemsSource = _paletteRows;

		// Populate the commands to reduce the first render time.
		_activationConfig = _plugin.Config.ActivationConfig;
		PopulateItems(_configContext.CommandManager);
		UpdateMatches();
	}

	/// <summary>
	/// Populate <see cref="_allCommands"/> with all the current commands.
	/// </summary>
	private void PopulateItems(IEnumerable<CommandItem> items)
	{
		Logger.Debug($"Populating the current list of all commands.");

		int idx = 0;
		foreach ((ICommand command, IKeybind? keybind) in items)
		{
			if (!command.CanExecute())
			{
				continue;
			}

			if (idx < _allCommands.Count)
			{
				if (_allCommands[idx].Command != command)
				{
					_allCommands[idx] = new CommandItem(command, keybind);
				}
			}
			else
			{
				_allCommands.Add(new CommandItem(command, keybind));
			}

			idx++;
		}

		for (; idx < _allCommands.Count; idx++)
		{
			_allCommands.RemoveAt(_allCommands.Count - 1);
		}
	}

	private void PreActivate(IMonitor? monitor)
	{
		_monitor = monitor ?? _configContext.MonitorManager.FocusedMonitor;
		TextEntry.Text = "";
	}

	private void PostActivate()
	{
		if (_monitor == null)
		{
			Logger.Error("Attempted to activate the command palette without a monitor.");
			return;
		}

		int width = 680;
		int height = 680;

		ILocation<int> windowLocation = new Location(
			x: _monitor.X + (_monitor.Width / 2) - (width / 2),
			y: _monitor.Y + (height / 4),
			width: width,
			height: height
		);

		Activate();
		TextEntry.Focus(FocusState.Programmatic);
		WindowDeferPosHandle.SetWindowPos(
			new WindowState(_window, windowLocation, WindowSize.Normal),
			_window.Handle
		);
		_window.FocusForceForeground();
	}

	/// <summary>
	/// Activate the command palette.
	/// </summary>
	/// <param name="activatePayload">The configuration for activation.</param>
	/// <param name="items">
	/// The items to activate the command palette with. These items will be passed to the
	/// <see cref="ICommandPaletteMatcher"/> to filter the results.
	/// When the text is empty, typically all items are shown.
	/// </param>
	/// <param name="monitor">The monitor to display the command palette on.</param>
	public void Activate(CommandPaletteActivationConfig activatePayload, IEnumerable<CommandItem> items, IMonitor? monitor = null)
	{
		Logger.Debug("Activating command palette");

		PreActivate(monitor);

		_activationConfig = activatePayload;
		_freeTextCallback = null;
		PopulateItems(items);
		UpdateMatches();

		PostActivate();
	}

	public void ActivateFreeForm(CommandPaletteFreeTextCallback callback, IMonitor? monitor = null)
	{
		Logger.Debug("Activating command palette in free form mode");

		PreActivate(monitor);

		_freeTextCallback = callback;
		_activationConfig = null;
		UpdateMatches();

		PostActivate();
	}

	/// <summary>
	/// Hide the command palette. Wipe the query text and clear the matches.
	/// </summary>
	public void Hide()
	{
		Logger.Debug("Hiding command palette");
		_window.Hide();
		_monitor = null;
		TextEntry.Text = "";
		_allCommands.Clear();
	}

	/// <summary>
	/// Toggle the visibility of the command palette.
	/// </summary>
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

	/// <summary>
	/// Handler for when the user presses down a key.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void TextEntry_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		Logger.Debug("Command palette key down: {0}", e.Key.ToString());
		int selectedIndex = ListViewItems.SelectedIndex;
		switch (e.Key)
		{
			case Windows.System.VirtualKey.Down:
				// Go down the command palette's list.
				ListViewItems.SelectedIndex = (selectedIndex + 1) % _paletteRows.Count;
				ListViewItems.ScrollIntoView(ListViewItems.SelectedItem);
				break;

			case Windows.System.VirtualKey.Up:
				// Go up the command palette's list.
				ListViewItems.SelectedIndex = (selectedIndex + _paletteRows.Count - 1) % _paletteRows.Count;
				ListViewItems.ScrollIntoView(ListViewItems.SelectedItem);
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

	/// <summary>
	/// Update the matches shown to the user.
	/// Effort has been made to reduce the amount of time spent executing this method.
	/// It can likely be improved (help is welcomed).
	/// </summary>
	private void UpdateMatches()
	{
		Logger.Debug("Updating command palette matches");
		string query = TextEntry.Text;
		int idx = 0;

		if (_activationConfig != null)
		{
			foreach (PaletteRowItem item in _activationConfig.Matcher.Match(query, _allCommands))
			{
				Logger.Verbose($"Matched {item.CommandItem.Command.Title}");
				if (idx < _paletteRows.Count)
				{
					// Update the existing row.
					_paletteRows[idx].Update(item);
				}
				else if (_unusedRows.Count > 0)
				{
					// Restoring the unused row.
					PaletteRow row = _unusedRows[^1];
					row.Update(item);

					_paletteRows.Add(row);

					_unusedRows.RemoveAt(_unusedRows.Count - 1);
				}
				else
				{
					// Add a new row.
					PaletteRow row = new(item);
					_paletteRows.Add(row);
					row.Initialize();
				}
				idx++;

				Logger.Verbose($"Finished updating {item.CommandItem.Command.Title}");
			}
		}

		// If there are more items than we have space for, remove the last ones.
		int count = _paletteRows.Count;
		for (; idx < count; idx++)
		{
			_unusedRows.Add(_paletteRows[^1]);
			_paletteRows.RemoveAt(_paletteRows.Count - 1);
		}

		Logger.Verbose($"Command palette match count: {_paletteRows.Count}");
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

		if (_freeTextCallback != null)
		{
			_freeTextCallback(TextEntry.Text);
			Hide();
			return;
		}
		else if (_activationConfig == null)
		{
			Hide();
			return;
		}

		if (ListViewItems.SelectedIndex < 0)
		{
			Hide();
		}

		CommandItem match = _paletteRows[ListViewItems.SelectedIndex].Model.CommandItem;

		// Since the palette window is reused, there's a chance that the _activationConfig
		// will have been wiped by a free form child command.
		CommandPaletteActivationConfig currentConfig = _activationConfig;
		match.Command.TryExecute();
		currentConfig.Matcher.OnMatchExecuted(match);

		if (_activationConfig == currentConfig)
		{
			Hide();
		}
	}
}
