using AutoFixture;
using FluentAssertions;
using Microsoft.UI.Xaml;
using NSubstitute;
using Whim.TestUtils;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MenuVariantViewModelCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		ICommandPaletteWindowViewModel windowViewModel = fixture.Freeze<ICommandPaletteWindowViewModel>();
		windowViewModel.IsConfigActive(Arg.Any<BaseVariantConfig>()).Returns(true);
		windowViewModel.Text.Returns("ti");
	}
}

public class MenuVariantViewModelTests
{
	private static IVariantRowView<MenuVariantRowModelData, MenuVariantRowViewModel> MenuRowFactory(
		MatcherResult<MenuVariantRowModelData> item
	) => new MenuRowStub() { ViewModel = new MenuVariantRowViewModel(item) };

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void Constructor(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		ctx.CommandManager.GetEnumerator()
			.Returns(new List<ICommand>() { new Command("id", "title", () => { }) }.GetEnumerator());

		// When
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		// Then
		Assert.Single(vm._allItems);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void Activate_NotMenuVariantConfig(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		// When
		vm.Activate(new UnknownConfig());

		// Then
		Assert.Empty(vm._allItems);
		Assert.Null(vm.ConfirmButtonText);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void Activate_MenuVariantConfig(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		IEnumerable<ICommand> items =
		[
			new Command("id", "title", () => { }),
			new Command("id2", "title2", () => { }),
			new Command("id3", "title3", () => { })
		];
		ctx.CommandManager.GetEnumerator().Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		// When
		vm.Activate(new MenuVariantConfig() { Commands = items, ConfirmButtonText = "Execute" });

		// Then
		Assert.Equal(3, vm._allItems.Count);
		Assert.Equal("Execute", vm.ConfirmButtonText);
	}

	[InlineAutoSubstituteData<MenuVariantViewModelCustomization>(VirtualKey.Up, 2)]
	[InlineAutoSubstituteData<MenuVariantViewModelCustomization>(VirtualKey.Down, 1)]
	[Theory]
	internal void OnKeyDown_VerticalArrow(
		VirtualKey key,
		int expectedIndex,
		IContext ctx,
		ICommandPaletteWindowViewModel windowViewModel
	)
	{
		// Given
		IEnumerable<ICommand> items =
		[
			new Command("id", "title", () => { }),
			new Command("id2", "title2", () => { }),
			new Command("id3", "title3", () => { })
		];
		ctx.CommandManager.GetEnumerator().Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
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

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void OnKeyDown_UnhandledKeys(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		IEnumerable<ICommand> items =
		[
			new Command("id", "title", () => { }),
			new Command("id2", "title2", () => { }),
			new Command("id3", "title3", () => { })
		];
		ctx.CommandManager.GetEnumerator().Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
		vm.Activate(new MenuVariantConfig() { Commands = items });

		// When
		vm.OnKeyDown(VirtualKey.Escape);

		// Then
		Assert.Equal(0, vm.SelectedIndex);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void OnKeyDown_Enter(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		bool called = false;
		IEnumerable<ICommand> items =
		[
			new Command(
				"id",
				"title",
				() =>
				{
					called = true;
				}
			)
		];
		ctx.CommandManager.GetEnumerator().Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
		vm.Activate(new MenuVariantConfig() { Commands = items });

		// When
		vm.OnKeyDown(VirtualKey.Enter);

		// Then
		Assert.Equal(0, vm.SelectedIndex);
		Assert.True(called);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void PopulateItems_CannotExecute(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		// When
		vm.PopulateItems([new Command("id", "title", () => { }, () => false)]);

		// Then
		Assert.Empty(vm._allItems);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void PopulateItems_AddNew(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		// When
		vm.PopulateItems([new Command("id", "title", () => { })]);

		// Then
		Assert.Single(vm._allItems);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void PopulateItems_UpdateExisting(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		vm.PopulateItems([new Command("id", "title", () => { })]);

		// When
		vm.PopulateItems([new Command("id", "new title", () => { })]);

		// Then
		Assert.Single(vm._allItems);
		Assert.Equal("new title", vm._allItems[0].Data.Command.Title);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void PopulateItems_RemoveExtra(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		vm.PopulateItems([new Command("id", "title", () => { })]);

		// When
		vm.PopulateItems([]);

		// Then
		Assert.Empty(vm._allItems);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void PopulateItems_SkipEqualCommand(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		Command command = new("id", "title", () => { });
		vm.PopulateItems([command]);

		// When
		vm.PopulateItems([command]);

		// Then
		Assert.Single(vm._allItems);
		Assert.Equal(command, vm._allItems[0].Data.Command);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void ExecuteCommand(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		bool called = false;
		IEnumerable<ICommand> items =
		[
			new Command(
				"id",
				"title",
				() =>
				{
					called = true;
				}
			)
		];
		ctx.CommandManager.GetEnumerator().Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
		vm.Activate(new MenuVariantConfig() { Commands = items });

		// When
		vm.ExecuteCommand();

		// Then
		Assert.True(called);
		windowViewModel.Received(1).RequestHide();
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void ExecuteCommand_ReuseShouldNotHide(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		string callbackText = string.Empty;
		IEnumerable<ICommand> items = [new Command("id", "title", () => { })];
		ctx.CommandManager.GetEnumerator().Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
		MenuVariantConfig activationConfig = new() { Commands = items };
		vm.Activate(activationConfig);
		windowViewModel.IsConfigActive(activationConfig).Returns(false);

		// When
		vm.ExecuteCommand();

		// Then
		windowViewModel.DidNotReceive().RequestHide();
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void ExecuteCommand_NotActivated(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		// When
		vm.ExecuteCommand();

		// Then
		windowViewModel.DidNotReceive().RequestHide();
	}

	[InlineAutoSubstituteData(-1)]
	[InlineAutoSubstituteData(1)]
	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void ExecuteCommand_InvalidSelectedIndex(
		int index,
		IContext ctx,
		ICommandPaletteWindowViewModel windowViewModel
	)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
		vm.Activate(new MenuVariantConfig() { Commands = [new Command("id", "title", () => { })] });
		vm.SelectedIndex = index;

		// When
		vm.ExecuteCommand();

		// Then
		windowViewModel.DidNotReceive().RequestHide();
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void Confirm(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		bool called = false;
		IEnumerable<ICommand> items =
		[
			new Command(
				"id",
				"title",
				() =>
				{
					called = true;
				}
			)
		];
		ctx.CommandManager.GetEnumerator().Returns(items.GetEnumerator());

		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
		vm.Activate(new MenuVariantConfig() { Commands = items });

		// When
		vm.Confirm();

		// Then
		Assert.True(called);
		windowViewModel.Received(1).RequestHide();
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void Update_NoMatches(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		vm.Activate(new MenuVariantConfig() { Commands = [] });

		// When
		vm.Update();

		// Then
		Assert.Equal(Visibility.Visible, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Collapsed, vm.ListViewItemsVisibility);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void Update_NotActivated(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		// When
		vm.Update();

		// Then
		Assert.Empty(vm._allItems);
	}

	private static MenuVariantConfig CreateMenuActivationConfig(int itemCount)
	{
		List<MatcherResult<MenuVariantRowModelData>> items = [];

		for (int i = 0; i < itemCount; i++)
		{
			FilterTextMatch[] segments = [new(0, 1)];
			items.Add(new(new MenuVariantRowModel(new Command($"id{i}", $"title{i}", () => { }), null), segments, 0));
		}

		IMatcher<MenuVariantRowModelData> matcher = Substitute.For<IMatcher<MenuVariantRowModelData>>();
		matcher
			.Match(Arg.Any<string>(), Arg.Any<IReadOnlyList<IVariantRowModel<MenuVariantRowModelData>>>())
			.Returns(items);

		MenuVariantConfig config = new() { Matcher = matcher, Commands = [] };
		return config;
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void Update_SomeMatches(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
		vm.Activate(CreateMenuActivationConfig(3));

		// When
		vm.Update();

		// Then
		Assert.Equal(Visibility.Collapsed, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Visible, vm.ListViewItemsVisibility);
		Assert.Equal(3, vm.MenuRows.Count);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void Update_RemoveUnused(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);

		vm.Activate(CreateMenuActivationConfig(3));
		vm.Activate(CreateMenuActivationConfig(2));

		// When
		vm.Activate(CreateMenuActivationConfig(3));
		vm.Update();

		// Then
		Assert.Equal(Visibility.Collapsed, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Visible, vm.ListViewItemsVisibility);
		Assert.Equal(3, vm.MenuRows.Count);

		// First and second element should have been updated
		Assert.True(vm.MenuRows[0] is MenuRowStub stub && stub.IsUpdated);
		Assert.True(vm.MenuRows[1] is MenuRowStub stub2 && stub2.IsUpdated);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void LoadMenuMatches_AddRows(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
		string query = "ti";
		MenuVariantConfig config = CreateMenuActivationConfig(2);

		// When
		vm.LoadMenuMatches(query, config);

		// Then
		Assert.Equal(2, vm.MenuRows.Count);
		Assert.Equal("title0", vm.MenuRows[0].ViewModel.Model.Title);
		Assert.Equal("title1", vm.MenuRows[1].ViewModel.Model.Title);
	}

	[Theory, AutoSubstituteData<MenuVariantViewModelCustomization>]
	internal void LoadMenuMatches_UpdateRows(IContext ctx, ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		MenuVariantViewModel vm = new(ctx, windowViewModel, MenuRowFactory);
		string query = "ti";

		vm.LoadMenuMatches(query, CreateMenuActivationConfig(2));

		// When
		vm.LoadMenuMatches(query, CreateMenuActivationConfig(2));

		// Then
		Assert.Equal(2, vm.MenuRows.Count);
		vm.MenuRows.Should()
			.AllSatisfy(r =>
			{
				Assert.True(r is MenuRowStub row && row.IsUpdated);
			});
	}
}
