using Microsoft.UI.Xaml;
using Moq;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class SelectVariantViewModelTests
{
	private static IVariantRow<SelectOption> SelectRowFactory(
		IVariantItem<SelectOption> variantItem,
		SelectVariantConfig selectVariantConfig
	)
	{
		Mock<IVariantRow<SelectOption>> variantRowMock = new();
		variantRowMock.Setup(v => v.Item).Returns(variantItem);
		return variantRowMock.Object;
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
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, SelectRowFactory) { RowHeight = 0, };

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
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, SelectRowFactory) { RowHeight = 0, };

		// When
		// Then it doesn't throw
		selectVariantViewModel.Activate(new Mock<BaseVariantConfig>().Object);
	}

	[Fact]
	public void Update_NotActivated()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = CreateStubs();
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, SelectRowFactory) { RowHeight = 0, };

		// When
		// Then it doesn't throw
		selectVariantViewModel.Update();
	}

	[Fact]
	public void Update_Activated_NoMatches()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock = CreateStubs();
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, SelectRowFactory) { RowHeight = 0, };

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
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, SelectRowFactory) { RowHeight = 0, };

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
						IsDisabled = false,
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
		Mock<ICommandPaletteWindowViewModel>,
		List<SelectOption>,
		Mock<IMatcher<SelectOption>>
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
					IsDisabled = false,
				},
				new()
				{
					Id = "id2",
					Title = "title2",
					IsSelected = false,
					IsDisabled = false,
				},
				new SelectOption()
				{
					Id = "id3",
					Title = "title3",
					IsSelected = false,
					IsDisabled = false,
				}
			};

		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, SelectRowFactory) { RowHeight = 0, };

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

		return (selectVariantViewModel, activationConfig, commandPaletteWindowViewModelMock, options, matcherMock);
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
			Mock<IMatcher<SelectOption>> matcherMock
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
		(SelectVariantViewModel selectVariantViewModel, SelectVariantConfig activationConfig, _, _, _) =
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
		(SelectVariantViewModel selectVariantViewModel, SelectVariantConfig activationConfig, _, _, _) =
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
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			List<SelectOption> options,
			Mock<IMatcher<SelectOption>> matcherMock
		) = CreateOptionsStubs();
		selectVariantViewModel.Activate(activationConfig);

		selectVariantViewModel.SelectedIndex = 2;

		// When
		selectVariantViewModel.UpdateSelectedItem();

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
		SelectVariantViewModel selectVariantViewModel =
			new(commandPaletteWindowViewModelMock.Object, SelectRowFactory) { RowHeight = 0, };

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
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			List<SelectOption> options,
			Mock<IMatcher<SelectOption>> matcherMock
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
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			_,
			Mock<IMatcher<SelectOption>> matcherMock
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
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			_,
			Mock<IMatcher<SelectOption>> matcherMock
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
	public void VariantRow_OnClick_UnknownVariantRow()
	{
		// Given
		(
			SelectVariantViewModel selectVariantViewModel,
			SelectVariantConfig activationConfig,
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			_,
			Mock<IMatcher<SelectOption>> matcherMock
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
			Mock<ICommandPaletteWindowViewModel> commandPaletteWindowViewModelMock,
			List<SelectOption> options,
			Mock<IMatcher<SelectOption>> matcherMock
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
}
