using Microsoft.UI.Xaml;
using NSubstitute;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class SelectVariantViewModelTests
{
	private static IEnumerable<MatcherResult<SelectOption>> ConvertToMatcherResults(
		IEnumerable<IVariantRowModel<SelectOption>> variantRowModels
	)
	{
		return variantRowModels.Select(
			v => new MatcherResult<SelectOption>(v, new[] { new FilterTextMatch(0, v.Title.Length) }, 0)
		);
	}

	private static (
		Func<
			MatcherResult<SelectOption>,
			SelectVariantConfig,
			IVariantRowView<SelectOption, SelectVariantRowViewModel>
		>,
		List<IVariantRowView<SelectOption, SelectVariantRowViewModel>>
	) SelectRowFactoryWithMocks()
	{
		List<IVariantRowView<SelectOption, SelectVariantRowViewModel>> variantRows = new();

		IVariantRowView<SelectOption, SelectVariantRowViewModel> selectRowFactory(
			MatcherResult<SelectOption> matcherResult,
			SelectVariantConfig config
		)
		{
			IVariantRowView<SelectOption, SelectVariantRowViewModel> row = Substitute.For<
				IVariantRowView<SelectOption, SelectVariantRowViewModel>
			>();
			row.ViewModel.Returns(new SelectVariantRowViewModel(matcherResult));
			variantRows.Add(row);
			return row;
		}

		return (selectRowFactory, variantRows);
	}

	[Fact]
	public void Activate_SelectVariantConfig()
	{
		// Given
		ICommandPaletteWindowViewModel commandPaletteWindowViewModel = Substitute.For<ICommandPaletteWindowViewModel>();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel = new(commandPaletteWindowViewModel, selectRowFactory);

		IEnumerable<SelectOption> optionsMock = Substitute.For<IEnumerable<SelectOption>>();
		optionsMock.GetEnumerator().Returns((_) => new List<SelectOption>().GetEnumerator());

		SelectVariantConfig activationConfig = new() { Callback = (items) => { }, Options = optionsMock, };

		// When
		selectVariantViewModel.Activate(activationConfig);

		// Then
		optionsMock.Received(1).GetEnumerator();
	}

	[Fact]
	public void Activate_BaseVariantConfig()
	{
		// Given
		ICommandPaletteWindowViewModel commandPaletteWindowViewModel = Substitute.For<ICommandPaletteWindowViewModel>();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel = new(commandPaletteWindowViewModel, selectRowFactory);

		// When
		selectVariantViewModel.Activate(Substitute.For<BaseVariantConfig>());

		// Then it doesn't throw
		Assert.Null(selectVariantViewModel.ConfirmButtonText);
	}

	[Fact]
	public void Update_NotActivated()
	{
		// Given
		ICommandPaletteWindowViewModel commandPaletteWindowViewModel = Substitute.For<ICommandPaletteWindowViewModel>();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel = new(commandPaletteWindowViewModel, selectRowFactory);

		// When
		selectVariantViewModel.Update();

		// Then it doesn't throw
		Assert.Null(selectVariantViewModel.ConfirmButtonText);
	}

	[Fact]
	public void Update_Activated_NoMatches()
	{
		// Given
		ICommandPaletteWindowViewModel commandPaletteWindowViewModel = Substitute.For<ICommandPaletteWindowViewModel>();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel = new(commandPaletteWindowViewModel, selectRowFactory);

		IEnumerable<SelectOption> optionsMock = Substitute.For<IEnumerable<SelectOption>>();
		optionsMock.GetEnumerator().Returns(_ => new List<SelectOption>().GetEnumerator());

		SelectVariantConfig activationConfig =
			new()
			{
				Callback = (items) => { },
				Options = optionsMock,
				ConfirmButtonText = "Call"
			};

		selectVariantViewModel.Activate(activationConfig);

		// When
		selectVariantViewModel.Update();

		// Then
		Assert.Equal(-1, selectVariantViewModel.SelectedIndex);
		Assert.Equal(Visibility.Collapsed, selectVariantViewModel.SelectRowsItemsVisibility);
		Assert.Equal(Visibility.Visible, selectVariantViewModel.NoMatchingOptionsTextBlockVisibility);
		Assert.Equal("Call", selectVariantViewModel.ConfirmButtonText);
	}

	[Fact]
	public void Update_Activated_Matches()
	{
		// Given
		ICommandPaletteWindowViewModel commandPaletteWindowViewModel = Substitute.For<ICommandPaletteWindowViewModel>();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel = new(commandPaletteWindowViewModel, selectRowFactory);

		IEnumerable<SelectOption> optionsMock = Substitute.For<IEnumerable<SelectOption>>();
		optionsMock
			.GetEnumerator()
			.Returns(
				_ =>
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

		SelectVariantConfig activationConfig = new() { Callback = (items) => { }, Options = optionsMock, };

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
		SelectVariantCallback,
		ICommandPaletteWindowViewModel,
		List<SelectOption>,
		IMatcher<SelectOption>,
		List<IVariantRowView<SelectOption, SelectVariantRowViewModel>>
	) CreateOptionsStubs()
	{
		ICommandPaletteWindowViewModel commandPaletteWindowViewModel = Substitute.For<ICommandPaletteWindowViewModel>();
		commandPaletteWindowViewModel.Text.Returns("ti");

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
				new()
				{
					Id = "id3",
					Title = "title3",
					IsSelected = false,
					IsEnabled = false,
				}
			};

		(var selectRowFactory, var selectRowFactoryResults) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel = new(commandPaletteWindowViewModel, selectRowFactory);

		SelectVariantCallback callbackMock = Substitute.For<SelectVariantCallback>();
		IMatcher<SelectOption> matcherMock = Substitute.For<IMatcher<SelectOption>>();
		matcherMock
			.Match(Arg.Any<string>(), Arg.Any<IReadOnlyList<IVariantRowModel<SelectOption>>>())
			.Returns(
				options.Select(
					o =>
						new MatcherResult<SelectOption>(
							new SelectVariantRowModel(o),
							new[] { new FilterTextMatch(0, o.Title.Length) },
							0
						)
				)
			);

		SelectVariantConfig activationConfig =
			new()
			{
				Callback = callbackMock,
				Options = options,
				Matcher = matcherMock,
			};

		return (
			selectVariantViewModel,
			activationConfig,
			callbackMock,
			commandPaletteWindowViewModel,
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
			IMatcher<SelectOption> matcherMock,
			_
		) = CreateOptionsStubs();
		matcherMock
			.Match(Arg.Any<string>(), Arg.Any<IReadOnlyList<IVariantRowModel<SelectOption>>>())
			.Returns(new List<MatcherResult<SelectOption>>());

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
			ICommandPaletteWindowViewModel commandPaletteWindowViewModel,
			List<SelectOption> options,
			IMatcher<SelectOption> matcherMock,
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
		matcherMock.Received(1).OnMatchExecuted(Arg.Any<IVariantRowModel<SelectOption>>());
		commandPaletteWindowViewModel.Received(1).RequestFocusTextBox();
	}

	[Fact]
	public void UpdateSelectedItem_NotActivated()
	{
		// Given
		ICommandPaletteWindowViewModel commandPaletteWindowViewModel = Substitute.For<ICommandPaletteWindowViewModel>();
		(var selectRowFactory, _) = SelectRowFactoryWithMocks();
		SelectVariantViewModel selectVariantViewModel = new(commandPaletteWindowViewModel, selectRowFactory);

		// When
		// Then it should not throw
		selectVariantViewModel.UpdateSelectedItem();
	}

	[Fact]
	public void UpdateSelectedItem_SingleSelect()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			ICommandPaletteWindowViewModel commandPaletteWindowViewModel,
			List<SelectOption> options,
			IMatcher<SelectOption> matcherMock,
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
		matcherMock.Received(1).OnMatchExecuted(Arg.Any<IVariantRowModel<SelectOption>>());
		commandPaletteWindowViewModel.Received(1).RequestFocusTextBox();
	}

	[Fact]
	public void UpdateSelectedItem_MultiSelect()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			_,
			ICommandPaletteWindowViewModel commandPaletteWindowViewModel,
			List<SelectOption> options,
			IMatcher<SelectOption> matcherMock,
			_
		) = CreateOptionsStubs();
		selectVariantViewModel.Activate(activationConfig);

		// When
		selectVariantViewModel.SelectedIndex = 1;
		selectVariantViewModel.UpdateSelectedItem();

		selectVariantViewModel.SelectedIndex = 2;
		selectVariantViewModel.UpdateSelectedItem();

		// Then
		Assert.False(options[0].IsSelected);
		Assert.True(options[1].IsSelected);
		Assert.True(options[2].IsSelected);
		matcherMock.Received(2).OnMatchExecuted(Arg.Any<IVariantRowModel<SelectOption>>());
		commandPaletteWindowViewModel.Received(2).RequestFocusTextBox();
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
				new()
				{
					Id = "4",
					Title = "title4",
					IsSelected = true,
					IsEnabled = false
				},
				new()
				{
					Id = "5",
					Title = "title5",
					IsSelected = false,
					IsEnabled = false
				},
				new()
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
				new()
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
		Assert.Equal(options[0], selectVariantViewModel.SelectRows[0].ViewModel.Model.Data);
		Assert.Equal(options[1], selectVariantViewModel.SelectRows[1].ViewModel.Model.Data);
		Assert.Equal(options[2], selectVariantViewModel.SelectRows[2].ViewModel.Model.Data);
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
			IMatcher<SelectOption> matcherMock,
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

		List<SelectVariantRowModel> updatedVariantItems = updatedOptions
			.Select(o => new SelectVariantRowModel(o))
			.ToList();

		matcherMock
			.Match(Arg.Any<string>(), Arg.Any<IReadOnlyList<IVariantRowModel<SelectOption>>>())
			.Returns(ConvertToMatcherResults(updatedVariantItems));

		// When
		selectVariantViewModel.LoadSelectMatches(
			"test",
			new SelectVariantConfig()
			{
				Options = updatedOptions,
				Callback = activationConfig.Callback,
				Matcher = activationConfig.Matcher,
			}
		);

		// Then
		Assert.Equal(3, selectVariantViewModel.SelectRows.Count);
		createdRows[1].Update(
			Arg.Is<MatcherResult<SelectOption>>(m => (SelectVariantRowModel)m.Model == updatedVariantItems[1])
		);
	}

	/// <summary>
	/// Tests <see cref="SelectVariantViewModel.LoadSelectMatches"/> via <see cref="SelectVariantViewModel.Update"/> via
	/// <see cref="SelectVariantViewModel.Activate(BaseVariantConfig)"/>.
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
			IMatcher<SelectOption> matcherMock,
			var createdRows
		) = CreateOptionsStubs();

		// First load
		selectVariantViewModel.Activate(activationConfig);

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
		List<SelectVariantRowModel> secondVariantItems = secondOptions
			.Select(o => new SelectVariantRowModel(o))
			.ToList();

		matcherMock
			.Match(Arg.Any<string>(), Arg.Any<IReadOnlyList<IVariantRowModel<SelectOption>>>())
			.Returns(ConvertToMatcherResults(secondVariantItems));

		selectVariantViewModel.Activate(
			new SelectVariantConfig()
			{
				Options = secondOptions,
				Callback = activationConfig.Callback,
				Matcher = activationConfig.Matcher,
			}
		);

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
		List<SelectVariantRowModel> thirdVariantItems = thirdOptions.Select(o => new SelectVariantRowModel(o)).ToList();

		matcherMock
			.Match(Arg.Any<string>(), Arg.Any<IReadOnlyList<IVariantRowModel<SelectOption>>>())
			.Returns(ConvertToMatcherResults(thirdVariantItems));

		selectVariantViewModel.Activate(
			new SelectVariantConfig()
			{
				Options = thirdOptions,
				Callback = activationConfig.Callback,
				Matcher = activationConfig.Matcher,
			}
		);

		// Then
		// NOTE: RemoveUnusedRows is called by Update
		Assert.Equal(2, selectVariantViewModel.SelectRows.Count);
		createdRows[0]
			.Received(1)
			.Update(Arg.Is<MatcherResult<SelectOption>>(m => (SelectVariantRowModel)m.Model == thirdVariantItems[0]));
		createdRows[1]
			.Received(1)
			.Update(Arg.Is<MatcherResult<SelectOption>>(m => (SelectVariantRowModel)m.Model == thirdVariantItems[1]));
		Assert.Single(selectVariantViewModel._unusedRows);
	}

	[Fact]
	public void Save()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			SelectVariantCallback callbackMock,
			_,
			List<SelectOption> options,
			_,
			_
		) = CreateOptionsStubs();

		// When
		selectVariantViewModel.Activate(activationConfig);
		selectVariantViewModel.Update();
		selectVariantViewModel.Confirm();

		// Then
		callbackMock.Received(1).Invoke(Arg.Is<IEnumerable<SelectOption>>(o => o.SequenceEqual(options)));
	}
}
