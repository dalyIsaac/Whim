using FluentAssertions;
using Microsoft.UI.Xaml;
using Moq;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MenuVariantViewModelTests
{
	private static (Mock<IConfigContext>, Mock<ICommandManager>, Mock<ICommandPaletteWindowViewModel>) CreateStubs()
	{
		Mock<ICommandManager> commandManager = new();
		commandManager.Setup(cm => cm.GetEnumerator()).Returns(new List<CommandItem>().GetEnumerator());

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.CommandManager).Returns(commandManager.Object);

		Mock<ICommandPaletteWindowViewModel> windowViewModel = new();
		windowViewModel.Setup(wvm => wvm.IsConfigActive(It.IsAny<BaseVariantConfig>())).Returns(true);
		windowViewModel.Setup(wvm => wvm.Text).Returns("ti");

		return (configContext, commandManager, windowViewModel);
	}

	private static IVariantRowView<CommandItem, MenuVariantRowViewModel> MenuRowFactory(
		MatcherResult<CommandItem> item
	) => new MenuRowStub() { ViewModel = new MenuVariantRowViewModel(item) };

	[Fact]
	public void Constructor()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();
		commandManager
			.Setup(cm => cm.GetEnumerator())
			.Returns(
				new List<CommandItem>()
				{
					new CommandItem() { Command = new Command("id", "title", () => { }) }
				}.GetEnumerator()
			);

		// When
		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);

		// Then
		Assert.Single(vm._allItems);
	}

	[Theory]
	[InlineData(VirtualKey.Up, 2)]
	[InlineData(VirtualKey.Down, 1)]
	public void OnKeyDown_VerticalArrow(VirtualKey key, int expectedIndex)
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		IEnumerable<CommandItem> items = new List<CommandItem>()
		{
			new CommandItem() { Command = new Command("id", "title", () => { }) },
			new CommandItem() { Command = new Command("id2", "title2", () => { }) },
			new CommandItem() { Command = new Command("id3", "title3", () => { }) }
		};
		commandManager.Setup(cm => cm.GetEnumerator()).Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);
		vm.Activate(new MenuVariantConfig() { Commands = items });

		// When
		Assert.Raises<EventArgs>(
			h => vm.ScrollIntoViewRequested += h,
			h => vm.ScrollIntoViewRequested -= h,
			() => vm.OnKeyDown(key)
		);

		// Then
		Assert.Equal(expectedIndex, vm.SelectedIndex);
	}

	[Fact]
	public void OnKeyDown_UnhandledKeys()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		IEnumerable<CommandItem> items = new List<CommandItem>()
		{
			new CommandItem() { Command = new Command("id", "title", () => { }) },
			new CommandItem() { Command = new Command("id2", "title2", () => { }) },
			new CommandItem() { Command = new Command("id3", "title3", () => { }) }
		};
		commandManager.Setup(cm => cm.GetEnumerator()).Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);
		vm.Activate(new MenuVariantConfig() { Commands = items });

		// When
		vm.OnKeyDown(VirtualKey.Escape);

		// Then
		Assert.Equal(0, vm.SelectedIndex);
	}

	[Fact]
	public void PopulateItems_CannotExecute()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);

		// When
		vm.PopulateItems(
			new List<CommandItem>()
			{
				new CommandItem() { Command = new Command("id", "title", () => { }, () => false) },
			}
		);

		// Then
		Assert.Empty(vm._allItems);
	}

	[Fact]
	public void PopulateItems_AddNew()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);

		// When
		vm.PopulateItems(
			new List<CommandItem>() { new CommandItem() { Command = new Command("id", "title", () => { }) }, }
		);

		// Then
		Assert.Single(vm._allItems);
	}

	[Fact]
	public void PopulateItems_UpdateExisting()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);

		vm.PopulateItems(
			new List<CommandItem>() { new CommandItem() { Command = new Command("id", "title", () => { }) }, }
		);

		// When
		vm.PopulateItems(
			new List<CommandItem>() { new CommandItem() { Command = new Command("id", "new title", () => { }) }, }
		);

		// Then
		Assert.Single(vm._allItems);
		Assert.Equal("new title", vm._allItems[0].Data.Command.Title);
	}

	[Fact]
	public void PopulateItems_RemoveExtra()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);

		vm.PopulateItems(
			new List<CommandItem>() { new CommandItem() { Command = new Command("id", "title", () => { }) }, }
		);

		// When
		vm.PopulateItems(new List<CommandItem>());

		// Then
		Assert.Empty(vm._allItems);
	}

	[Fact]
	public void PopulateItems_SkipEqualCommand()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);

		Command command = new("id", "title", () => { });
		CommandItem commandItem = new() { Command = command };
		vm.PopulateItems(new List<CommandItem>() { commandItem, });

		// When
		vm.PopulateItems(new List<CommandItem>() { new CommandItem() { Command = command }, });

		// Then
		Assert.Single(vm._allItems);
		Assert.Equal(commandItem, vm._allItems[0].Data);
	}

	[Fact]
	public void ExecuteCommand()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		bool called = false;
		IEnumerable<CommandItem> items = new List<CommandItem>()
		{
			new CommandItem()
			{
				Command = new Command(
					"id",
					"title",
					() =>
					{
						called = true;
					}
				)
			},
		};

		commandManager.Setup(cm => cm.GetEnumerator()).Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);
		vm.Activate(new MenuVariantConfig() { Commands = items });

		// When
		vm.ExecuteCommand();

		// Then
		Assert.True(called);
		windowViewModel.Verify(wvm => wvm.RequestHide());
	}

	[Fact]
	public void ExecuteCommand_ReuseShouldNotHide()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();
		windowViewModel.Setup(wvm => wvm.IsConfigActive(It.IsAny<BaseVariantConfig>())).Returns(false);

		string callbackText = string.Empty;
		IEnumerable<CommandItem> items = new List<CommandItem>()
		{
			new CommandItem() { Command = new Command("id", "title", () => { }) },
		};

		commandManager.Setup(cm => cm.GetEnumerator()).Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);
		vm.Activate(new MenuVariantConfig() { Commands = items });
		windowViewModel.Setup(wvm => wvm.IsConfigActive(It.IsAny<BaseVariantConfig>())).Returns(false);

		// When
		vm.ExecuteCommand();

		// Then
		windowViewModel.Verify(wvm => wvm.RequestHide(), Times.Never);
	}

	[Fact]
	public void Update_NoMatches()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);

		vm.Activate(new MenuVariantConfig() { Commands = Array.Empty<CommandItem>() });

		// When
		vm.Update();

		// Then
		Assert.Equal(Visibility.Visible, vm.ListViewItemsWrapperVisibility);
		Assert.Equal(Visibility.Visible, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Collapsed, vm.ListViewItemsVisibility);
	}

	private static MenuVariantConfig CreateMenuActivationConfig(int itemCount)
	{
		List<MatcherResult<CommandItem>> items = new();

		for (int i = 0; i < itemCount; i++)
		{
			FilterTextMatch[] segments = new FilterTextMatch[] { new FilterTextMatch(0, 1) };
			items.Add(
				new(
					new MenuVariantRowModel(
						new CommandItem() { Command = new Command($"id{i}", $"title{i}", () => { }) }
					),
					segments,
					0
				)
			);
		}

		Mock<IMatcher<CommandItem>> matcher = new();
		matcher
			.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<IReadOnlyList<IVariantRowModel<CommandItem>>>()))
			.Returns(items);

		MenuVariantConfig config = new() { Matcher = matcher.Object, Commands = Array.Empty<CommandItem>() };
		return config;
	}

	[Fact]
	public void Update_SomeMatches()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);
		vm.Activate(CreateMenuActivationConfig(3));

		// When
		vm.Update();

		// Then
		Assert.Equal(Visibility.Visible, vm.ListViewItemsWrapperVisibility);
		Assert.Equal(Visibility.Collapsed, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Visible, vm.ListViewItemsVisibility);
		Assert.Equal(3, vm.MenuRows.Count);
	}

	[Fact]
	public void Update_RemoveUnused()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);

		vm.Activate(CreateMenuActivationConfig(3));
		vm.Activate(CreateMenuActivationConfig(2));

		// When
		vm.Activate(CreateMenuActivationConfig(3));
		vm.Update();

		// Then
		Assert.Equal(Visibility.Visible, vm.ListViewItemsWrapperVisibility);
		Assert.Equal(Visibility.Collapsed, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Visible, vm.ListViewItemsVisibility);
		Assert.Equal(3, vm.MenuRows.Count);

		// First and second element should have been updated
		Assert.True(vm.MenuRows[0] is MenuRowStub stub && stub.IsUpdated);
		Assert.True(vm.MenuRows[1] is MenuRowStub stub2 && stub2.IsUpdated);
	}

	[Fact]
	public void LoadMenuMatches_AddRows()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);
		string query = "ti";
		MenuVariantConfig config = CreateMenuActivationConfig(2);

		// When
		vm.LoadMenuMatches(query, config);

		// Then
		Assert.Equal(2, vm.MenuRows.Count);
		Assert.Equal("title0", vm.MenuRows[0].ViewModel.Model.Title);
		Assert.Equal("title1", vm.MenuRows[1].ViewModel.Model.Title);
	}

	[Fact]
	public void LoadMenuMatches_UpdateRows()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();

		MenuVariantViewModel vm = new(configContext.Object, windowViewModel.Object, MenuRowFactory);
		string query = "ti";

		vm.LoadMenuMatches(query, CreateMenuActivationConfig(2));

		// When
		vm.LoadMenuMatches(query, CreateMenuActivationConfig(2));

		// Then
		Assert.Equal(2, vm.MenuRows.Count);
		vm.MenuRows
			.Should()
			.AllSatisfy(r =>
			{
				Assert.True(r is MenuRowStub row && row.IsUpdated);
			});
	}
}
