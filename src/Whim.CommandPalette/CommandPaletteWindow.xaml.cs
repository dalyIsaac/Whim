using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
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

	private int _maxHeight;

	private readonly ObservableCollection<PaletteRow> _paletteRows = new();
	private readonly List<PaletteRow> _unusedRows = new();

	/// <summary>
	/// The current commands from which the matches shown in <see cref="ListViewItems"/> are drawn.
	/// </summary>
	private readonly List<CommandItem> _allCommands = new();

	private BaseCommandPaletteActivationConfig? _activationConfig;

	public CommandPaletteWindow(IConfigContext configContext, CommandPalettePlugin plugin)
	{
		_configContext = configContext;
		_plugin = plugin;
		_activationConfig = _plugin.Config.ActivationConfig;

		_window = this.InitializeBorderlessWindow("Whim.CommandPalette", "CommandPaletteWindow", _configContext);
		this.SetIsShownInSwitchers(false);

		Activated += CommandPaletteWindow_Activated;

		Title = CommandPaletteConfig.Title;
		ListViewItems.ItemsSource = _paletteRows;

		// Populate the commands to reduce the first render time.
		PopulateItems(_configContext.CommandManager);
		UpdateMatches();
	}

	private void CommandPaletteWindow_Activated(object sender, WindowActivatedEventArgs e)
	{
		if (e.WindowActivationState == WindowActivationState.Deactivated)
		{
			// Hide the window when it loses focus.
			Hide();
		}
	}

	/// <summary>
	/// Activate the command palette.
	/// </summary>
	/// <param name="config">The configuration for activation.</param>
	/// <param name="items">
	/// The items to activate the command palette with. These items will be passed to the
	/// <see cref="ICommandPaletteMatcher"/> to filter the results.
	/// When the text is empty, typically all items are shown.
	/// </param>
	/// <param name="monitor">The monitor to display the command palette on.</param>
	public void Activate(
		BaseCommandPaletteActivationConfig config,
		IEnumerable<CommandItem>? items = null,
		IMonitor? monitor = null
	)
	{
		Logger.Debug("Activating command palette");
		ResetState();

		_activationConfig = config;
		_monitor = monitor ?? _configContext.MonitorManager.FocusedMonitor;

		TextEntry.Text = _activationConfig.InitialText;
		TextEntry.SelectAll();
		TextEntry.PlaceholderText = _activationConfig.Hint ?? "Start typing...";
		_maxHeight = (int)(_monitor.Height * _plugin.Config.MaxHeightPercent / 100.0);

		PopulateItems(items ?? Array.Empty<CommandItem>());
		UpdateMatches();

		Activate();
		TextEntry.Focus(FocusState.Programmatic);
		_window.FocusForceForeground();
	}

	/// <summary>
	/// Hide the command palette. Wipe the query text and clear the matches.
	/// </summary>
	public void Hide()
	{
		Logger.Debug("Hiding command palette");
		_window.Hide();
		ResetState();
	}

	/// <summary>
	/// Reset the window's state.
	/// </summary>
	public void ResetState()
	{
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
					_allCommands[idx] = new CommandItem() { Command = command, Keybind = keybind };
				}
			}
			else
			{
				_allCommands.Add(new CommandItem() { Command = command, Keybind = keybind });
			}

			idx++;
		}

		for (; idx < _allCommands.Count; idx++)
		{
			_allCommands.RemoveAt(_allCommands.Count - 1);
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
			case Windows.System.VirtualKey.Down when ListViewItems.Items.Count > 0:
				// Go down the command palette's list.
				ListViewItems.SelectedIndex = (selectedIndex + 1) % _paletteRows.Count;
				ListViewItems.ScrollIntoView(ListViewItems.SelectedItem);
				break;

			case Windows.System.VirtualKey.Up when ListViewItems.Items.Count > 0:
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
	/// </summary>
	private void UpdateMatches()
	{
		Logger.Debug("Updating command palette matches");
		string query = TextEntry.Text;
		int matchesCount = 0;

		if (_activationConfig is CommandPaletteMenuActivationConfig menuActivationConfig)
		{
			matchesCount = LoadMatches(query, menuActivationConfig);

			ListViewItemsWrapper.Visibility = Visibility.Visible;
			if (matchesCount == 0)
			{
				NoMatchingCommandsTextBlock.Visibility = Visibility.Visible;
				ListViewItems.Visibility = Visibility.Collapsed;
			}
			else
			{
				NoMatchingCommandsTextBlock.Visibility = Visibility.Collapsed;
				ListViewItems.Visibility = Visibility.Visible;
			}
		}
		else
		{
			NoMatchingCommandsTextBlock.Visibility = Visibility.Collapsed;
			ListViewItemsWrapper.Visibility = Visibility.Collapsed;
		}

		RemoveUnusedRows(matchesCount);

		Logger.Verbose($"Command palette match count: {_paletteRows.Count}");
		ListViewItems.SelectedIndex = _paletteRows.Count > 0 ? 0 : -1;
		SetWindowPos();
	}

	/// <summary>
	/// Load the matches into the command palette rows.
	/// </summary>
	/// <param name="query">The query text string.</param>
	/// <param name="menuActivationConfig"></param>
	/// <returns>The number of processed matches.</returns>
	private int LoadMatches(string query, CommandPaletteMenuActivationConfig menuActivationConfig)
	{
		int matchesCount = 0;

		foreach (PaletteRowItem item in menuActivationConfig.Matcher.Match(query, _allCommands))
		{
			Logger.Verbose($"Matched {item.CommandItem.Command.Title}");
			if (matchesCount < _paletteRows.Count)
			{
				// Update the existing row.
				_paletteRows[matchesCount].Update(item);
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
			matchesCount++;

			Logger.Verbose($"Finished updating {item.CommandItem.Command.Title}");
		}

		return matchesCount;
	}

	/// <summary>
	/// If there are more items than we have space for, remove the last ones.
	/// </summary>
	/// <param name="idx"></param>
	private void RemoveUnusedRows(int idx)
	{
		int count = _paletteRows.Count;
		for (; idx < count; idx++)
		{
			_unusedRows.Add(_paletteRows[^1]);
			_paletteRows.RemoveAt(_paletteRows.Count - 1);
		}
	}

	/// <summary>
	/// Sets the position of the command palette window.
	/// </summary>
	private void SetWindowPos()
	{
		if (_monitor == null)
		{
			Logger.Error("Attempted to activate the command palette without a monitor.");
			return;
		}

		int width = _plugin.Config.MaxWidthPixels;
		int height = _maxHeight;

		if (NoMatchingCommandsTextBlock.Visibility == Visibility.Visible)
		{
			height = (int)(TextEntry.ActualHeight * 2) + 12;
		}
		else if (ListViewItems.Items.Count > 0)
		{
			DependencyObject? container = ListViewItems.ContainerFromIndex(0);
			if (container is ListViewItem item)
			{
				int fullHeight = (int)(TextEntry.ActualHeight + (item.ActualHeight * ListViewItems.Items.Count));
				height = Math.Min(_maxHeight, fullHeight);
			}
		}

		int scaleFactor = _monitor.ScaleFactor;
		double scale = scaleFactor / 100.0;
		height = (int)(height * scale);

		int x = (_monitor.Width / 2) - (width / 2);
		int y = (int)(_monitor.Height * _plugin.Config.YPositionPercent / 100.0);

		ILocation<int> windowLocation = new Location<int>()
		{
			X = _monitor.X + x,
			Y = _monitor.Y + y,
			Width = width,
			Height = height
		};

		WindowContainer.MaxHeight = height;

		WindowDeferPosHandle.SetWindowPosFixScaling(
			windowState: new WindowState()
			{
				Window = _window,
				Location = windowLocation,
				WindowSize = WindowSize.Normal
			},
			monitorManager: _configContext.MonitorManager,
			monitor: _monitor,
			hwndInsertAfter: _window.Handle
		);
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

		if (_activationConfig is CommandPaletteFreeTextActivationConfig freeTextActivationConfig)
		{
			freeTextActivationConfig.Callback(TextEntry.Text);
			Hide();
			return;
		}
		else if (_activationConfig is CommandPaletteMenuActivationConfig menuActivationConfig)
		{
			CommandItem match = _paletteRows[ListViewItems.SelectedIndex].Model.CommandItem;

			// Since the palette window is reused, there's a chance that the _activationConfig
			// will have been wiped by a free form child command.
			BaseCommandPaletteActivationConfig currentConfig = _activationConfig;
			match.Command.TryExecute();
			menuActivationConfig.Matcher.OnMatchExecuted(match);

			if (_activationConfig == currentConfig)
			{
				Hide();
			}
		}
	}
}
