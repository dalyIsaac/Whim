using Microsoft.UI.Xaml;
using Moq;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class SelectVariantViewModelTests
{
	private static (
		Func<IVariantItem<SelectOption>, SelectVariantConfig, IVariantRow<SelectOption>>,
		List<Mock<IVariantRow<SelectOption>>>
	) SelectRowFactoryWithMocks()
	{
		List<Mock<IVariantRow<SelectOption>>> variantRowMocks = new();
		IVariantRow<SelectOption> selectRowFactory(IVariantItem<SelectOption> item, SelectVariantConfig config)
		{
			Mock<IVariantRow<SelectOption>> variantRowMock = new();
			variantRowMock.Setup(v => v.Item).Returns(item);
			variantRowMocks.Add(variantRowMock);
			return variantRowMock.Object;
		}

		return (selectRowFactory, variantRowMocks);
	}

	private static Mock<ICommandPaletteWindowViewModel> CreateStubs()
	{
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = new();
		commandPaletteWindowViewModelMock.Setup(c => c.Text).Returns(string.Empty);

		return commandPaletteWindowViewModelMock;
	}

	[Fact]
	public void Activate_SelectVariantConfig()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = CreateStubs();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, selectRowFactory) { RowHeight = 0, };

		Mock<IEnumerable<SelectOption>> optionsMock = new();
		optionsMock.Setup(x => x.GetEnumerator()).Returns(new List<SelectOption>().GetEnumerator());

		SelectVariantConfig activationConfig =
			new()
			{
				Callback = (items) => { },
				Options = optionsMock.Object,
				AllowMultiSelect = false
			};

		// When
		selectVariantViewModel.Activate(activationConfig);

		// Then
		optionsMock.Verify(x => x.GetEnumerator(), Times.Once);
	}

	[Fact]
	public void Activate_BaseVariantConfig()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = CreateStubs();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, selectRowFactory) { RowHeight = 0, };

		// When
		// Then it doesn't throw
		selectVariantViewModel.Activate(new Mock<BaseVariantConfig>().Object);
	}

	[Fact]
	public void Update_NotActivated()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = CreateStubs();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, selectRowFactory) { RowHeight = 0, };

		// When
		// Then it doesn't throw
		selectVariantViewModel.Update();
	}

	[Fact]
	public void Update_Activated_NoMatches()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = CreateStubs();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, selectRowFactory) { RowHeight = 0, };

		Mock<IEnumerable<SelectOption>> optionsMock = new();
		optionsMock.Setup(x => x.GetEnumerator()).Returns(new List<SelectOption>().GetEnumerator());

		SelectVariantConfig activationConfig =
			new()
			{
				Callback = (items) => { },
				Options = optionsMock.Object,
				AllowMultiSelect = false
			};

		selectVariantViewModel.Activate(activationConfig);

		// When
		selectVariantViewModel.Update();

		// Then
		Assert.Equal(-1, selectVariantViewModel.SelectedIndex);
		Assert.Equal(Visibility.Collapsed, selectVariantViewModel.SelectRowsItemsVisibility);
		Assert.Equal(Visibility.Visible, selectVariantViewModel.NoMatchingOptionsTextBlockVisibility);
	}

	[Fact]
	public void Update_Activated_Matches()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = CreateStubs();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, selectRowFactory) { RowHeight = 0, };

		Mock<IEnumerable<SelectOption>> optionsMock = new();
		optionsMock
			.Setup(x => x.GetEnumerator())
			.Returns(
				new List<SelectOption>
				{
					new()
					{
						Id = "id",
						Title = "title",
						IsSelected = false,
						IsEnabled = false,
					}
				}.GetEnumerator()
			);

		SelectVariantConfig activationConfig =
			new()
			{
				Callback = (items) => { },
				Options = optionsMock.Object,
				AllowMultiSelect = false
			};

		selectVariantViewModel.Activate(activationConfig);

		// When
		selectVariantViewModel.Update();

		// Then
		Assert.Equal(0, selectVariantViewModel.SelectedIndex);
		Assert.Equal(Visibility.Visible, selectVariantViewModel.SelectRowsItemsVisibility);
		Assert.Equal(Visibility.Collapsed, selectVariantViewModel.NoMatchingOptionsTextBlockVisibility);
	}

	/// <summary>
	/// Create stubs and return the options.
	/// </summary>
	private static (
		SelectVariantViewModel,
		SelectVariantConfig,
		Mock<SelectVariantCallback>,
		Mock<ICommandPaletteWindowViewModel>,
		List<SelectOption>,
		Mock<IMatcher<SelectOption>>,
		List<Mock<IVariantRow<SelectOption>>>
	) CreateOptionsStubs()
	{
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = CreateStubs();
		commandPaletteWindowViewModelMock.Setup(c => c.Text).Returns("ti");

		List<SelectOption> options =
			new()
			{
				new()
				{
					Id = "id",
					Title = "title",
					IsSelected = false,
					IsEnabled = false,
				},
				new()
				{
					Id = "id2",
					Title = "title2",
					IsSelected = false,
					IsEnabled = false,
				},
				new SelectOption()
				{
					Id = "id3",
					Title = "title3",
					IsSelected = false,
					IsEnabled = false,
				}
			};

		(var selectRowFactory, var selectRowFactoryResults) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, selectRowFactory) { RowHeight = 0, };

		Mock<SelectVariantCallback> callbackMock = new();

		Mock<IMatcher<SelectOption>> matcherMock = new();
		matcherMock
			.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<IReadOnlyList<IVariantItem<SelectOption>>>()))
			.Returns(options.Select(o => new SelectVariantItem(o)));

		SelectVariantConfig activationConfig =
			new()
			{
				Callback = callbackMock.Object,
				Options = options,
				Matcher = matcherMock.Object,
				AllowMultiSelect = false
			};

		return (
			selectVariantViewModel,
			activationConfig,
			callbackMock,
			commandPaletteWindowViewModelMock,
			options,
			matcherMock,
			selectRowFactoryResults
		);
	}

	[Fact]
	public void OnKeyDown_NoRows()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			_,
			_,
			Mock<IMatcher<SelectOption>> matcherMock,
			_
		) = CreateOptionsStubs();
		matcherMock
			.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<IReadOnlyList<IVariantItem<SelectOption>>>()))
			.Returns(new List<IVariantItem<SelectOption>>());

		selectVariantViewModel.Activate(activationConfig);

		// When
		selectVariantViewModel.OnKeyDown(VirtualKey.Down);

		// Then
		Assert.Equal(-1, selectVariantViewModel.SelectedIndex);
	}

	[Fact]
	public void OnKeyDown_Down()
	{
		// Given
		(SelectVariantViewModel selectVariantViewModel, SelectVariantConfig activationConfig, _, _, _, _, _) =
			CreateOptionsStubs();
		selectVariantViewModel.Activate(activationConfig);

		// When
		Assert.Raises<EventArgs>(
			h => selectVariantViewModel.ScrollIntoViewRequested += h,
			h => selectVariantViewModel.ScrollIntoViewRequested -= h,
			() => selectVariantViewModel.OnKeyDown(VirtualKey.Down)
		);

		// Then
		Assert.Equal(1, selectVariantViewModel.SelectedIndex);
	}

	[Fact]
	public void OnKeyDown_Up()
	{
		// Given
		(SelectVariantViewModel selectVariantViewModel, SelectVariantConfig activationConfig, _, _, _, _, _) =
			CreateOptionsStubs();
		selectVariantViewModel.Activate(activationConfig);

		// When
		Assert.Raises<EventArgs>(
			h => selectVariantViewModel.ScrollIntoViewRequested += h,
			h => selectVariantViewModel.ScrollIntoViewRequested -= h,
			() => selectVariantViewModel.OnKeyDown(VirtualKey.Up)
		);

		// Then
		Assert.Equal(2, selectVariantViewModel.SelectedIndex);
	}

	[Fact]
	public void OnKeyDown_Enter()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			List<SelectOption> options,
			Mock<IMatcher<SelectOption>> matcherMock,
			_
		) = CreateOptionsStubs();
		selectVariantViewModel.Activate(activationConfig);

		selectVariantViewModel.SelectedIndex = 2;

		// When
		selectVariantViewModel.OnKeyDown(VirtualKey.Enter);

		// Then
		Assert.False(options[0].IsSelected);
		Assert.False(options[1].IsSelected);
		Assert.True(options[2].IsSelected);
		matcherMock.Verify(m => m.OnMatchExecuted(It.IsAny<IVariantItem<SelectOption>>()), Times.Once);
		commandPaletteWindowViewModelMock.Verify(c => c.RequestFocusTextBox(), Times.Once);
	}

	[Fact]
	public void UpdateSelectedItem_NotActivated()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = CreateStubs();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, selectRowFactory) { RowHeight = 0, };

		// When
		// Then it should not throw
		selectVariantViewModel.UpdateSelectedItem();
	}

	[Fact]
	public void UpdateSelectedItem_SingleSelect_NotSelected()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			List<SelectOption> options,
			Mock<IMatcher<SelectOption>> matcherMock,
			_
		) = CreateOptionsStubs();
		selectVariantViewModel.Activate(activationConfig);

		selectVariantViewModel.SelectedIndex = 1;

		// When
		selectVariantViewModel.UpdateSelectedItem();

		// Then
		Assert.False(options[0].IsSelected);
		Assert.True(options[1].IsSelected);
		Assert.False(options[2].IsSelected);
		matcherMock.Verify(m => m.OnMatchExecuted(It.IsAny<IVariantItem<SelectOption>>()), Times.Once);
		commandPaletteWindowViewModelMock.Verify(c => c.RequestFocusTextBox(), Times.Once);
	}

	[Fact]
	public void VariantRow_OnClick_NotIVariantRow()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			_,
			Mock<IMatcher<SelectOption>> matcherMock,
			_
		) = CreateOptionsStubs();
		selectVariantViewModel.Activate(activationConfig);

		// When
		selectVariantViewModel.VariantRow_OnClick(new Mock<IVariantRow<SelectOption>>().Object);

		// Then
		Assert.Equal(0, selectVariantViewModel.SelectedIndex);
		matcherMock.Verify(m => m.OnMatchExecuted(It.IsAny<IVariantItem<SelectOption>>()), Times.Never);
		commandPaletteWindowViewModelMock.Verify(c => c.RequestFocusTextBox(), Times.Never);
	}

	[Fact]
	public void VariantRow_OnClick_NotActivated()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			_,
			Mock<IMatcher<SelectOption>> matcherMock,
			_
		) = CreateOptionsStubs();

		// When
		selectVariantViewModel.VariantRow_OnClick(new Mock<IVariantRow<SelectOption>>().Object);

		// Then
		Assert.Equal(0, selectVariantViewModel.SelectedIndex);
		matcherMock.Verify(m => m.OnMatchExecuted(It.IsAny<IVariantItem<SelectOption>>()), Times.Never);
		commandPaletteWindowViewModelMock.Verify(c => c.RequestFocusTextBox(), Times.Never);
	}

	[Fact]
	public void VariantRow_OnClick_UnknownVariantRow()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			_,
			Mock<IMatcher<SelectOption>> matcherMock,
			_
		) = CreateOptionsStubs();
		selectVariantViewModel.Activate(activationConfig);

		// When
		selectVariantViewModel.VariantRow_OnClick(new Mock<IVariantRow<SelectOption>>().Object);

		// Then
		Assert.Equal(0, selectVariantViewModel.SelectedIndex);
		matcherMock.Verify(m => m.OnMatchExecuted(It.IsAny<IVariantItem<SelectOption>>()), Times.Never);
		commandPaletteWindowViewModelMock.Verify(c => c.RequestFocusTextBox(), Times.Never);
	}

	[Fact]
	public void VariantRow_OnClick_Success()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			List<SelectOption> options,
			Mock<IMatcher<SelectOption>> matcherMock,
			_
		) = CreateOptionsStubs();
		selectVariantViewModel.Activate(activationConfig);

		// When
		selectVariantViewModel.VariantRow_OnClick(selectVariantViewModel.SelectRows[1]);

		// Then
		Assert.Equal(1, selectVariantViewModel.SelectedIndex);
		Assert.False(options[0].IsSelected);
		Assert.True(options[1].IsSelected);
		Assert.False(options[2].IsSelected);
		matcherMock.Verify(m => m.OnMatchExecuted(It.IsAny<IVariantItem<SelectOption>>()), Times.Once);
		commandPaletteWindowViewModelMock.Verify(c => c.RequestFocusTextBox(), Times.Once);
	}

	[Fact]
	public void PopulateItems_Populate()
	{
		// Given
		(SelectVariantViewModel selectVariantViewModel, _, _, _, List<SelectOption> options, _, _) =
			CreateOptionsStubs();

		// When
		selectVariantViewModel.PopulateItems(options);

		// Then
		Assert.Equal(3, selectVariantViewModel._allItems.Count);

		Assert.Equal("title", selectVariantViewModel._allItems[0].Title);
		Assert.Equal("title2", selectVariantViewModel._allItems[1].Title);
		Assert.Equal("title3", selectVariantViewModel._allItems[2].Title);
	}

	[Fact]
	public void PopulateItems_UpdateWithNewRow()
	{
		// Given
		(SelectVariantViewModel selectVariantViewModel, _, _, _, List<SelectOption> options, _, _) =
			CreateOptionsStubs();
		selectVariantViewModel.PopulateItems(options);

		// When
		selectVariantViewModel.PopulateItems(
			new List<SelectOption>
			{
				new SelectOption()
				{
					Id = "4",
					Title = "title4",
					IsSelected = true,
					IsEnabled = false
				},
				new SelectOption()
				{
					Id = "5",
					Title = "title5",
					IsSelected = false,
					IsEnabled = false
				},
				new SelectOption()
				{
					Id = "6",
					Title = "title6",
					IsSelected = false,
					IsEnabled = false
				},
			}
		);

		// Then
		Assert.Equal(3, selectVariantViewModel._allItems.Count);

		Assert.Equal("title4", selectVariantViewModel._allItems[0].Title);
		Assert.Equal("title5", selectVariantViewModel._allItems[1].Title);
		Assert.Equal("title6", selectVariantViewModel._allItems[2].Title);
	}

	[Fact]
	public void PopulateItems_UpdateSomeRows()
	{
		// Given
		(SelectVariantViewModel selectVariantViewModel, _, _, _, List<SelectOption> options, _, _) =
			CreateOptionsStubs();
		selectVariantViewModel.PopulateItems(options);

		// When
		selectVariantViewModel.PopulateItems(
			new List<SelectOption>
			{
				options[0],
				new SelectOption()
				{
					Id = "5",
					Title = "title5",
					IsSelected = false,
					IsEnabled = false
				},
			}
		);

		// Then
		Assert.Equal(2, selectVariantViewModel._allItems.Count);

		Assert.Equal("title", selectVariantViewModel._allItems[0].Title);
		Assert.Equal("title5", selectVariantViewModel._allItems[1].Title);
	}

	[Fact]
	public void LoadSelectMatches_AddNewRows()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			_,
			List<SelectOption> options,
			_,
			_
		) = CreateOptionsStubs();

		// When
		selectVariantViewModel.LoadSelectMatches("test", activationConfig);

		// Then
		Assert.Equal(3, selectVariantViewModel.SelectRows.Count);
		Assert.Equal(options[0], selectVariantViewModel.SelectRows[0].Item.Data);
		Assert.Equal(options[1], selectVariantViewModel.SelectRows[1].Item.Data);
		Assert.Equal(options[2], selectVariantViewModel.SelectRows[2].Item.Data);
	}

	[Fact]
	public void LoadSelectMatches_UpdateRows()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			_,
			List<SelectOption> options,
			Mock<IMatcher<SelectOption>> matcherMock,
			var createdRows
		) = CreateOptionsStubs();
		selectVariantViewModel.LoadSelectMatches("test", activationConfig);

		SelectOption newOption =
			new()
			{
				Id = "5",
				Title = "title5",
				IsSelected = false,
				IsEnabled = false
			};

		List<SelectOption> updatedOptions = new() { options[0], newOption, options[2], };

		List<SelectVariantItem> updatedVariantItems = updatedOptions.Select(o => new SelectVariantItem(o)).ToList();

		matcherMock
			.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<IReadOnlyList<IVariantItem<SelectOption>>>()))
			.Returns(updatedVariantItems);

		// When
		selectVariantViewModel.LoadSelectMatches(
			"test",
			new SelectVariantConfig()
			{
				Options = updatedOptions,
				AllowMultiSelect = activationConfig.AllowMultiSelect,
				Callback = activationConfig.Callback,
				Matcher = activationConfig.Matcher,
			}
		);

		// Then
		Assert.Equal(3, selectVariantViewModel.SelectRows.Count);
		createdRows[1].Verify(r => r.Update(updatedVariantItems[1]));
	}

	/// <summary>
	/// Tests <see cref="SelectVariantViewModel.LoadSelectMatches"/> via <see cref="SelectVariantViewModel.Update"/>.
	/// This test verifies that the unused row is restored, and <see cref="SelectVariantViewModel.RemoveUnusedRows"/> is called.
	/// </summary>
	[Fact]
	public void Update_LoadSelectMatches_RestoreUnusedRow()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			_,
			List<SelectOption> options,
			Mock<IMatcher<SelectOption>> matcherMock,
			var createdRows
		) = CreateOptionsStubs();

		// First load
		selectVariantViewModel.Activate(activationConfig);
		selectVariantViewModel.Update();

		// Second load
		SelectOption secondNewOption =
			new()
			{
				Id = "5",
				Title = "title5",
				IsSelected = false,
				IsEnabled = false
			};
		List<SelectOption> secondOptions = new() { secondNewOption };
		List<SelectVariantItem> secondVariantItems = secondOptions.Select(o => new SelectVariantItem(o)).ToList();

		matcherMock
			.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<IReadOnlyList<IVariantItem<SelectOption>>>()))
			.Returns(secondVariantItems);
		selectVariantViewModel.Activate(
			new SelectVariantConfig()
			{
				Options = secondOptions,
				AllowMultiSelect = activationConfig.AllowMultiSelect,
				Callback = activationConfig.Callback,
				Matcher = activationConfig.Matcher,
			}
		);
		selectVariantViewModel.Update();

		// When third load
		SelectOption thirdNewOption =
			new()
			{
				Id = "6",
				Title = "title6",
				IsSelected = false,
				IsEnabled = false
			};
		List<SelectOption> thirdOptions = new() { secondNewOption, thirdNewOption };
		List<SelectVariantItem> thirdVariantItems = thirdOptions.Select(o => new SelectVariantItem(o)).ToList();

		matcherMock
			.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<IReadOnlyList<IVariantItem<SelectOption>>>()))
			.Returns(thirdVariantItems);
		selectVariantViewModel.Activate(
			new SelectVariantConfig()
			{
				Options = thirdOptions,
				AllowMultiSelect = activationConfig.AllowMultiSelect,
				Callback = activationConfig.Callback,
				Matcher = activationConfig.Matcher,
			}
		);
		selectVariantViewModel.Update();

		// Then
		// NOTE: RemoveUnusedRows is called by Update
		Assert.Equal(2, selectVariantViewModel.SelectRows.Count);
		createdRows[0].Verify(r => r.Update(thirdVariantItems[0]));
		createdRows[1].Verify(r => r.Update(thirdVariantItems[1]));
		Assert.Single(selectVariantViewModel._unusedRows);
	}

	[Fact]
	public void Save()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			Mock<SelectVariantCallback> callbackMock,
			_,
			List<SelectOption> options,
			_,
			_
		) = CreateOptionsStubs();

		// When
		selectVariantViewModel.Activate(activationConfig);
		selectVariantViewModel.Update();
		selectVariantViewModel.Save();

		// Then
		callbackMock.Verify(c => c.Invoke(It.IsAny<IEnumerable<SelectOption>>()));
	}

	[Fact]
	public void GetViewMaxHeight()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			_,
			List<SelectOption> options,
			_,
			_
		) = CreateOptionsStubs();

		// When
		selectVariantViewModel.Activate(activationConfig);
		selectVariantViewModel.Update();

		// Then
		Assert.Equal(options.Count * selectVariantViewModel.RowHeight, selectVariantViewModel.GetViewMaxHeight());
	}
}
