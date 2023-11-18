using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Windows.System;

namespace Whim.CommandPalette;

internal class MenuVariantViewModel : IVariantViewModel
{
	private readonly IContext _context;
	private readonly ICommandPaletteWindowViewModel _commandPaletteWindowViewModel;

	private MenuVariantConfig? _activationConfig;

	/// <summary>
	/// The rows which are currently unused and can be reused for new matches.
	/// Keeping these around avoids the need to create new rows every time the palette is shown.
	/// </summary>
	private readonly List<IVariantRowView<MenuVariantRowModelData, MenuVariantRowViewModel>> _unusedRows = new();

	/// <summary>
	/// The current commands from which the matches shown in <see cref="MenuRows"/> are drawn.
	/// </summary>
	internal readonly List<MenuVariantRowModel> _allItems = new();

	/// <summary>
	/// Factory to create menu rows to make it possible to use xunit.
	/// It turns out it's annoying to test the Windows App SDK with xunit.
	/// </summary>
	private readonly Func<
		MatcherResult<MenuVariantRowModelData>,
		IVariantRowView<MenuVariantRowModelData, MenuVariantRowViewModel>
	> _menuRowFactory;

	public readonly ObservableCollection<IVariantRowView<MenuVariantRowModelData, MenuVariantRowViewModel>> MenuRows =
		new();

	public string? ConfirmButtonText => _activationConfig?.ConfirmButtonText;

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

	public event PropertyChangedEventHandler? PropertyChanged;

	public event EventHandler<EventArgs>? ScrollIntoViewRequested;

	public MenuVariantViewModel(
		IContext context,
		ICommandPaletteWindowViewModel commandPaletteWindowViewModel,
		Func<
			MatcherResult<MenuVariantRowModelData>,
			IVariantRowView<MenuVariantRowModelData, MenuVariantRowViewModel>
		>? menuRowFactory = null
	)
	{
		_context = context;
		_commandPaletteWindowViewModel = commandPaletteWindowViewModel;
		_menuRowFactory =
			menuRowFactory ?? ((MatcherResult<MenuVariantRowModelData> item) => new MenuVariantRowView(_context, item));

		// Populate the commands to reduce the first render time.
		PopulateItems(context.CommandManager);
	}

	public void Activate(BaseVariantConfig activationConfig)
	{
		if (activationConfig is MenuVariantConfig menuVariantConfig)
		{
			Logger.Verbose("Activating menu variant");
			_activationConfig = menuVariantConfig;
			PopulateItems(menuVariantConfig.Commands);
			Update();
		}
		else
		{
			Logger.Error("Invalid activation config type");
		}
	}

	public void Update()
	{
		if (_activationConfig == null)
		{
			return;
		}

		int matchesCount = LoadMenuMatches(_commandPaletteWindowViewModel.Text, _activationConfig);

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

		RemoveUnusedRows(matchesCount);
		Logger.Verbose($"Command palette match count: {MenuRows.Count}");
		SelectedIndex = MenuRows.Count > 0 ? 0 : -1;
	}

	public void OnKeyDown(VirtualKey key)
	{
		bool scrollIntoView = false;

		switch (key)
		{
			case VirtualKey.Down when MenuRows.Count > 0:
				// Go down the command palette's list.
				SelectedIndex = (SelectedIndex + 1) % MenuRows.Count;
				scrollIntoView = true;
				break;

			case VirtualKey.Up when MenuRows.Count > 0:
				// Go up the command palette's list.
				SelectedIndex = (SelectedIndex + MenuRows.Count - 1) % MenuRows.Count;
				scrollIntoView = true;
				break;

			case VirtualKey.Enter:
				ExecuteCommand();
				break;

			default:
				break;
		}

		if (scrollIntoView)
		{
			ScrollIntoViewRequested?.Invoke(this, EventArgs.Empty);
		}
	}

	public void ExecuteCommand()
	{
		if (_activationConfig == null)
		{
			return;
		}

		if (SelectedIndex < 0 || SelectedIndex >= MenuRows.Count)
		{
			Logger.Error($"Invalid index {SelectedIndex}");
			return;
		}

		Logger.Verbose($"Executing command at index {SelectedIndex}");
		IVariantRowModel<MenuVariantRowModelData> paletteItem = MenuRows[SelectedIndex].ViewModel.Model;
		MenuVariantRowModelData match = paletteItem.Data;

		// Since the palette window is reused, there's a chance that the _activationConfig
		// will have been wiped by a free form child command.
		MenuVariantConfig currentConfig = _activationConfig;

		currentConfig.Matcher.OnMatchExecuted(paletteItem);
		match.Command.TryExecute();

		if (_commandPaletteWindowViewModel.IsConfigActive(currentConfig))
		{
			_commandPaletteWindowViewModel.RequestHide();
		}
	}

	/// <summary>
	/// Populate <see cref="_allItems"/> with all the current commands.
	/// </summary>
	internal void PopulateItems(IEnumerable<ICommand> commands)
	{
		Logger.Debug($"Populating the current list of all commands.");

		int idx = 0;
		foreach (ICommand command in commands)
		{
			if (!command.CanExecute())
			{
				continue;
			}

			if (idx < _allItems.Count)
			{
				if (_allItems[idx].Data.Command != command)
				{
					_allItems[idx] = new MenuVariantRowModel(
						command,
						_context.KeybindManager.TryGetKeybind(command.Id)
					);
				}
			}
			else
			{
				_allItems.Add(new MenuVariantRowModel(command, _context.KeybindManager.TryGetKeybind(command.Id)));
			}

			idx++;
		}

		_allItems.RemoveRange(idx, _allItems.Count - idx);
	}

	/// <summary>
	/// Load the matches into the command palette rows.
	/// </summary>
	/// <param name="query">The query text string.</param>
	/// <param name="activationConfig"></param>
	/// <returns>The number of processed matches.</returns>
	internal int LoadMenuMatches(string query, MenuVariantConfig activationConfig)
	{
		int matchesCount = 0;

		foreach (MatcherResult<MenuVariantRowModelData> item in activationConfig.Matcher.Match(query, _allItems))
		{
			Logger.Verbose($"Matched {item.Model.Title}");
			if (matchesCount < MenuRows.Count)
			{
				// Update the existing row.
				MenuRows[matchesCount].Update(item);
			}
			else if (_unusedRows.Count > 0)
			{
				// Restoring the unused row.
				IVariantRowView<MenuVariantRowModelData, MenuVariantRowViewModel> row = _unusedRows[^1];
				row.Update(item);

				MenuRows.Add(row);
				_unusedRows.RemoveAt(_unusedRows.Count - 1);
			}
			else
			{
				// Add a new row.
				IVariantRowView<MenuVariantRowModelData, MenuVariantRowViewModel> row = _menuRowFactory(item);
				MenuRows.Add(row);
				row.Initialize();
			}
			matchesCount++;

			Logger.Verbose($"Finished updating {item.Model.Title}");
		}

		return matchesCount;
	}

	/// <summary>
	/// If there are more items than we have space for, remove the last ones.
	/// </summary>
	/// <param name="usedRowsCount">The currently used rows.</param>
	private void RemoveUnusedRows(int usedRowsCount)
	{
		int count = MenuRows.Count;
		for (; usedRowsCount < count; usedRowsCount++)
		{
			_unusedRows.Add(MenuRows[^1]);
			MenuRows.RemoveAt(MenuRows.Count - 1);
		}
	}

	public void Confirm() => ExecuteCommand();

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
