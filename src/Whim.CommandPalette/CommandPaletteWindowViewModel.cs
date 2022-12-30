using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Whim.CommandPalette;

internal class CommandPaletteWindowViewModel : INotifyPropertyChanged
{
	private readonly IConfigContext _configContext;
	private BaseCommandPaletteActivationConfig _activationConfig;

	/// <summary>
	/// The rows which are currently unused and can be reused for new matches.
	/// Keeping these around avoids the need to create new rows every time the palette is shown.
	/// </summary>
	private readonly List<IPaletteRow> _unusedRows = new();

	/// <summary>
	/// The current commands from which the matches shown in <see cref="PaletteRows"/> are drawn.
	/// </summary>
	private readonly List<CommandItem> _allCommands = new();

	/// <summary>
	/// Factory to create palette rows to make it possible to use xunit.
	/// It turns out it's annoying to test the Windows App SDK with xunit.
	/// </summary>
	private Func<PaletteRowItem, IPaletteRow> _paletteRowFactory;

	public CommandPalettePlugin Plugin { get; }

	public int MaxHeight { get; set; }

	private IMonitor? _monitor;
	public IMonitor? Monitor
	{
		get => _monitor;
		private set
		{
			if (Monitor != value)
			{
				_monitor = value;
				OnPropertyChanged(nameof(IsVisible));
			}
		}
	}

	private string _text = "";
	public string Text
	{
		get => _text;
		set
		{
			if (Text != value)
			{
				_text = value;
				OnPropertyChanged(nameof(Text));
			}
		}
	}

	private string _placeholderText = "";
	public string PlaceholderText
	{
		get => _placeholderText;
		set
		{
			if (PlaceholderText != value)
			{
				_placeholderText = value;
				OnPropertyChanged(nameof(PlaceholderText));
			}
		}
	}

	private int _selectedIndex;
	public int SelectedIndex
	{
		get => _selectedIndex;
		set
		{
			if (SelectedIndex != value)
			{
				_selectedIndex = value;
				OnPropertyChanged(nameof(SelectedIndex));
			}
		}
	}

	private Visibility _listViewItemsWrapperVisibility = Visibility.Visible;
	public Visibility ListViewItemsWrapperVisibility
	{
		get => _listViewItemsWrapperVisibility;
		set
		{
			if (ListViewItemsWrapperVisibility != value)
			{
				_listViewItemsWrapperVisibility = value;
				OnPropertyChanged(nameof(ListViewItemsWrapperVisibility));
			}
		}
	}

	private Visibility _listViewItemsVisibility = Visibility.Visible;
	public Visibility ListViewItemsVisibility
	{
		get => _listViewItemsVisibility;
		set
		{
			if (ListViewItemsVisibility != value)
			{
				_listViewItemsVisibility = value;
				OnPropertyChanged(nameof(ListViewItemsVisibility));
			}
		}
	}

	private Visibility _noMatchingCommandsTextBlockVisibility = Visibility.Collapsed;
	public Visibility NoMatchingCommandsTextBlockVisibility
	{
		get => _noMatchingCommandsTextBlockVisibility;
		set
		{
			if (NoMatchingCommandsTextBlockVisibility != value)
			{
				_noMatchingCommandsTextBlockVisibility = value;
				OnPropertyChanged(nameof(NoMatchingCommandsTextBlockVisibility));
			}
		}
	}

	public bool IsVisible => Monitor != null;

	public readonly ObservableCollection<IPaletteRow> PaletteRows = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	public event EventHandler? HideRequested;

	public event EventHandler? SetWindowPosRequested;

	public CommandPaletteWindowViewModel(
		IConfigContext configContext,
		CommandPalettePlugin plugin,
		Func<PaletteRowItem, IPaletteRow>? paletteRowFactory = null
	)
	{
		_configContext = configContext;
		Plugin = plugin;
		_activationConfig = Plugin.Config.ActivationConfig;

		_paletteRowFactory = paletteRowFactory ?? ((PaletteRowItem item) => new PaletteRow(item));

		// Populate the commands to reduce the first render time.
		PopulateItems(_configContext.CommandManager);
		UpdateMatches();
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
		ResetState();

		_activationConfig = config;
		Monitor = monitor ?? _configContext.MonitorManager.FocusedMonitor;

		Text = _activationConfig.InitialText ?? "";
		PlaceholderText = _activationConfig.Hint ?? "Start typing...";
		MaxHeight = (int)(Monitor.WorkingArea.Height * Plugin.Config.MaxHeightPercent / 100.0);

		PopulateItems(items ?? Array.Empty<CommandItem>());
		UpdateMatches();
	}

	public void RequestHide()
	{
		Logger.Debug("Request to hide the command palette window");
		HideRequested?.Invoke(this, EventArgs.Empty);
		ResetState();
	}

	/// <summary>
	/// Reset the window's state.
	/// </summary>
	private void ResetState()
	{
		_monitor = null;
		_text = "";
		_allCommands.Clear();
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
	/// Handles key presses.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <returns><see langword="true"/> when the selected item should scroll into view.</returns>
	public bool OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		Logger.Debug("Command palette key down: {0}", e.Key.ToString());

		switch (e.Key)
		{
			case Windows.System.VirtualKey.Down when PaletteRows.Count > 0:
				// Go down the command palette's list.
				SelectedIndex = (SelectedIndex + 1) % PaletteRows.Count;
				return true;

			case Windows.System.VirtualKey.Up when PaletteRows.Count > 0:
				// Go up the command palette's list.
				SelectedIndex = (SelectedIndex + PaletteRows.Count - 1) % PaletteRows.Count;
				return true;

			case Windows.System.VirtualKey.Enter:
				ExecuteCommand();
				break;

			case Windows.System.VirtualKey.Escape:
				RequestHide();
				break;

			default:
				break;
		}

		return false;
	}

	/// <summary>
	/// Update the matches shown to the user. <br />
	///
	/// This method has been optimized to reduce the execution time.
	/// </summary>
	public void UpdateMatches()
	{
		Logger.Debug("Updating command palette matches");
		string query = Text;
		int matchesCount = 0;

		if (_activationConfig is CommandPaletteMenuActivationConfig menuActivationConfig)
		{
			matchesCount = LoadMatches(query, menuActivationConfig);

			ListViewItemsWrapperVisibility = Visibility.Visible;
			if (matchesCount == 0)
			{
				NoMatchingCommandsTextBlockVisibility = Visibility.Visible;
				ListViewItemsVisibility = Visibility.Collapsed;
			}
			else
			{
				NoMatchingCommandsTextBlockVisibility = Visibility.Collapsed;
				ListViewItemsVisibility = Visibility.Visible;
			}
		}
		else
		{
			NoMatchingCommandsTextBlockVisibility = Visibility.Collapsed;
			ListViewItemsWrapperVisibility = Visibility.Collapsed;
		}

		RemoveUnusedRows(matchesCount);

		Logger.Verbose($"Command palette match count: {PaletteRows.Count}");
		SelectedIndex = PaletteRows.Count > 0 ? 0 : -1;
		SetWindowPosRequested?.Invoke(this, EventArgs.Empty);
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
			if (matchesCount < PaletteRows.Count)
			{
				// Update the existing row.
				PaletteRows[matchesCount].Update(item);
			}
			else if (_unusedRows.Count > 0)
			{
				// Restoring the unused row.
				IPaletteRow row = _unusedRows[^1];
				row.Update(item);

				PaletteRows.Add(row);
				_unusedRows.RemoveAt(_unusedRows.Count - 1);
			}
			else
			{
				// Add a new row.
				IPaletteRow row = _paletteRowFactory(item);
				PaletteRows.Add(row);
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
		int count = PaletteRows.Count;
		for (; idx < count; idx++)
		{
			_unusedRows.Add(PaletteRows[^1]);
			PaletteRows.RemoveAt(PaletteRows.Count - 1);
		}
	}

	public void ExecuteCommand()
	{
		Logger.Debug("Executing command");

		if (_activationConfig is CommandPaletteFreeTextActivationConfig freeTextActivationConfig)
		{
			freeTextActivationConfig.Callback(Text);
			RequestHide();
			return;
		}
		else if (_activationConfig is CommandPaletteMenuActivationConfig menuActivationConfig)
		{
			CommandItem match = PaletteRows[SelectedIndex].Model.CommandItem;

			// Since the palette window is reused, there's a chance that the _activationConfig
			// will have been wiped by a free form child command.
			BaseCommandPaletteActivationConfig currentConfig = _activationConfig;
			match.Command.TryExecute();
			menuActivationConfig.Matcher.OnMatchExecuted(match);

			if (_activationConfig == currentConfig)
			{
				RequestHide();
			}
		}
	}

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
