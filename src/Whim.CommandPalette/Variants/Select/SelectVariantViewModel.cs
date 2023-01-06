using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.System;

namespace Whim.CommandPalette;

internal class SelectVariantViewModel : IVariantViewModel
{
	private readonly ICommandPaletteWindowViewModel _commandPaletteWindowViewModel;

	private SelectVariantConfig? _activationConfig;

	/// <summary>
	/// The rows which are currently unused and can be reused for new matches.
	/// Keeping these around avoids the need to create new rows every time the palette is shown.
	/// </summary>
	private readonly List<IVariantRow<SelectOption>> _unusedRows = new();

	/// <summary>
	/// The current commands from which the matches shown in <see cref="SelectRows"/> are drawn.
	/// </summary>
	internal readonly List<SelectVariantItem> _allItems = new();

	/// <summary>
	/// Factory to create select rows to make it possible to use xunit.
	/// It turns out it's annoying to test the Windows App SDK with xunit.
	/// </summary>
	private readonly Func<IVariantItem<SelectOption>, IVariantRow<SelectOption>> _selectRowFactory;

	public readonly ObservableCollection<IVariantRow<SelectOption>> SelectRows = new();

	public bool ShowSaveButton => false;

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

	private Visibility _noMatchingOptionsTextBlockVisibility = Visibility.Collapsed;
	public Visibility NoMatchingOptionsTextBlockVisibility
	{
		get => _noMatchingOptionsTextBlockVisibility;
		set
		{
			if (NoMatchingOptionsTextBlockVisibility != value)
			{
				_noMatchingOptionsTextBlockVisibility = value;
				OnPropertyChanged(nameof(NoMatchingOptionsTextBlockVisibility));
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

	public SelectVariantViewModel(
		ICommandPaletteWindowViewModel commandPaletteWindowViewModel,
		Func<IVariantItem<SelectOption>, IVariantRow<SelectOption>>? selectRowFactory = null
	)
	{
		_commandPaletteWindowViewModel = commandPaletteWindowViewModel;
		_selectRowFactory = selectRowFactory ?? ((IVariantItem<SelectOption> item) => new SelectRow(item));
	}

	public void Activate(BaseVariantConfig activationConfig)
	{
		if (activationConfig is SelectVariantConfig selectVariantConfig)
		{
			_activationConfig = selectVariantConfig;
			PopulateItems(selectVariantConfig.Options);
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

		int matchesCount = LoadSelectMatches(_commandPaletteWindowViewModel.Text, _activationConfig);

		ListViewItemsWrapperVisibility = Visibility.Visible;
		if (matchesCount == 0)
		{
			NoMatchingOptionsTextBlockVisibility = Visibility.Visible;
			ListViewItemsVisibility = Visibility.Collapsed;
		}
		else
		{
			NoMatchingOptionsTextBlockVisibility = Visibility.Collapsed;
			ListViewItemsVisibility = Visibility.Visible;
		}

		RemoveUnusedRows(matchesCount);
		Logger.Verbose($"Command palette match count: {SelectRows.Count}");
		SelectedIndex = SelectRows.Count > 0 ? 0 : -1;
	}

	public void OnKeyDown(VirtualKey key)
	{
		bool scrollIntoView = false;

		switch (key)
		{
			case VirtualKey.Down when SelectRows.Count > 0:
				// Go down the command palette's list.
				SelectedIndex = (SelectedIndex + 1) % SelectRows.Count;
				scrollIntoView = true;
				break;

			case VirtualKey.Up when SelectRows.Count > 0:
				// Go up the command palette's list.
				SelectedIndex = (SelectedIndex + SelectRows.Count - 1) % SelectRows.Count;
				scrollIntoView = true;
				break;

			case VirtualKey.Enter:
			case VirtualKey.Space:
				ToggleSelectedItem();
				break;

			default:
				break;
		}

		if (scrollIntoView)
		{
			ScrollIntoViewRequested?.Invoke(this, EventArgs.Empty);
		}
	}

	public void ToggleSelectedItem()
	{
		if (_activationConfig == null)
		{
			return;
		}

		IVariantItem<SelectOption> paletteItem = SelectRows[SelectedIndex].Item;
		SelectOption match = paletteItem.Data;

		match.IsSelected = !match.IsSelected;
		_activationConfig.Matcher.OnMatchExecuted(paletteItem);

		// Since the palette window is reused, there's a chance that the _activationConfig
		// will have been wiped by a free form child command.
		if (_commandPaletteWindowViewModel.IsVariantActive(_activationConfig))
		{
			_commandPaletteWindowViewModel.RequestHide();
		}
	}

	/// <summary>
	/// Populate <see cref="_allItems"/> with all the current commands.
	/// </summary>
	internal void PopulateItems(IEnumerable<SelectOption> items)
	{
		Logger.Debug($"Populating the current list of all commands.");

		int idx = 0;
		foreach (SelectOption selectOption in items)
		{
			if (idx < _allItems.Count)
			{
				if (_allItems[idx].Data != selectOption)
				{
					_allItems[idx] = new SelectVariantItem(selectOption);
				}
			}
			else
			{
				_allItems.Add(new SelectVariantItem(selectOption));
			}

			idx++;
		}

		for (; idx < _allItems.Count; idx++)
		{
			_allItems.RemoveAt(_allItems.Count - 1);
		}
	}

	/// <summary>
	/// Load the matches into the command palette rows.
	/// </summary>
	/// <param name="query">The query text string.</param>
	/// <param name="activationConfig"></param>
	/// <returns>The number of processed matches.</returns>
	internal int LoadSelectMatches(string query, SelectVariantConfig activationConfig)
	{
		int matchesCount = 0;

		foreach (IVariantItem<SelectOption> item in activationConfig.Matcher.Match(query, _allItems))
		{
			Logger.Verbose($"Matched {item.Title}");
			if (matchesCount < SelectRows.Count)
			{
				// Update the existing row.
				SelectRows[matchesCount].Update(item);
			}
			else if (_unusedRows.Count > 0)
			{
				// Restoring the unused row.
				IVariantRow<SelectOption> row = _unusedRows[^1];
				row.Update(item);

				SelectRows.Add(row);
				_unusedRows.RemoveAt(_unusedRows.Count - 1);
			}
			else
			{
				// Add a new row.
				IVariantRow<SelectOption> row = _selectRowFactory(item);
				SelectRows.Add(row);
				row.Initialize();
			}
			matchesCount++;

			Logger.Verbose($"Finished updating {item.Title}");
		}

		return matchesCount;
	}

	/// <summary>
	/// If there are more items than we have space for, remove the last ones.
	/// </summary>
	/// <param name="usedRowsCount">The currently used rows.</param>
	private void RemoveUnusedRows(int usedRowsCount)
	{
		int count = SelectRows.Count;
		for (; usedRowsCount < count; usedRowsCount++)
		{
			_unusedRows.Add(SelectRows[^1]);
			SelectRows.RemoveAt(SelectRows.Count - 1);
		}
	}

	public void Hide()
	{
		NoMatchingOptionsTextBlockVisibility = Visibility.Collapsed;
		ListViewItemsWrapperVisibility = Visibility.Collapsed;
	}

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
