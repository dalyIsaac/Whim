using Moq;
using FluentAssertions;
using Xunit;
using Microsoft.UI.Xaml;

namespace Whim.CommandPalette.Tests;

public class CommandPaletteWindowViewModelTests
{
	private static (Mock<IConfigContext>, Mock<ICommandManager>, CommandPalettePlugin) CreateStubs()
	{
		Mock<ICommandManager> commandManager = new();
		commandManager.Setup(cm => cm.GetEnumerator()).Returns(new List<CommandItem>().GetEnumerator());

		Mock<IMonitor> monitor = new();
		monitor
			.Setup(m => m.WorkingArea)
			.Returns(
				new Location<int>()
				{
					X = 0,
					Y = 0,
					Height = 1080,
					Width = 1920
				}
			);

		Mock<IMonitorManager> monitorManager = new();
		monitorManager.Setup(m => m.FocusedMonitor).Returns(monitor.Object);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.CommandManager).Returns(commandManager.Object);
		configContext.Setup(c => c.MonitorManager).Returns(monitorManager.Object);

		CommandPalettePlugin plugin = new(configContext.Object, new());

		return (configContext, commandManager, plugin);
	}

	private static IPaletteRow PaletteRowFactory(PaletteRowItem item)
	{
		return new PaletteRowStub() { Model = item };
	}

	[Fact]
	public void Constructor()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();
		commandManager
			.Setup(cm => cm.GetEnumerator())
			.Returns(
				new List<CommandItem>()
				{
					new CommandItem() { Command = new Command("id", "title", () => { }) }
				}.GetEnumerator()
			);

		// When
		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		// Then
		Assert.Single(vm.PaletteRows);
	}

	[Fact]
	public void Activate_UseDefaults()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		// When
		Assert.Raises<EventArgs>(
			h => vm.SetWindowPosRequested += h,
			h => vm.SetWindowPosRequested -= h,
			() => vm.Activate(new MenuVariantConfig(), new List<CommandItem>(), null)
		);

		// Then
		Assert.Equal("", vm.Text);
		Assert.Equal("Start typing...", vm.PlaceholderText);
		Assert.Equal((int)(1080 * 0.4), vm.MaxHeight);
	}

	[Fact]
	public void Activate_DefineItems()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		Mock<IMonitor> monitor = new();
		monitor
			.Setup(m => m.WorkingArea)
			.Returns(
				new Location<int>()
				{
					X = 0,
					Y = 0,
					Height = 100,
					Width = 100
				}
			);

		BaseVariantConfig config = new MenuVariantConfig() { Hint = "Hint", InitialText = "Initial text" };

		IEnumerable<CommandItem> commandItems = new List<CommandItem>()
		{
			new CommandItem() { Command = new Command("id", "title", () => { }) }
		};

		// When
		Assert.Raises<EventArgs>(
			h => vm.SetWindowPosRequested += h,
			h => vm.SetWindowPosRequested -= h,
			() => vm.Activate(config, commandItems, monitor.Object)
		);

		// Then
		Assert.Equal("Initial text", vm.Text);
		Assert.Equal("Hint", vm.PlaceholderText);
		Assert.Equal((int)(100 * 0.4), vm.MaxHeight);
	}

	[Fact]
	public void RequestHide()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		// When
		// Then
		Assert.Raises<EventArgs>(h => vm.HideRequested += h, h => vm.HideRequested -= h, vm.RequestHide);
	}

	[Fact]
	public void OnKeyDown_Escape()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		// When
		bool? result = null;
		Assert.Raises<EventArgs>(
			h => vm.HideRequested += h,
			h => vm.HideRequested -= h,
			() =>
			{
				result = vm.OnKeyDown(this, Windows.System.VirtualKey.Escape);
			}
		);

		// Then
		Assert.False(result);
	}

	[Fact]
	public void OnKeyDown_Enter()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		string callbackText = "";
		FreeTextVariantConfig config =
			new() { InitialText = "Hello, world!", Callback = (text) => callbackText = text };
		plugin.Config.ActivationConfig = config;

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		vm.Activate(config, new List<CommandItem>(), null);

		// When
		bool? result = null;
		Assert.Raises<EventArgs>(
			h => vm.HideRequested += h,
			h => vm.HideRequested -= h,
			() =>
			{
				result = vm.OnKeyDown(this, Windows.System.VirtualKey.Enter);
			}
		);

		// Then
		Assert.False(result);
		Assert.Equal("Hello, world!", callbackText);
	}

	[Theory]
	[InlineData(Windows.System.VirtualKey.Up, 2)]
	[InlineData(Windows.System.VirtualKey.Down, 1)]
	public void OnKeyDown_VerticalArrow(Windows.System.VirtualKey key, int expectedIndex)
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		IEnumerable<CommandItem> items = new List<CommandItem>()
		{
			new CommandItem() { Command = new Command("id", "title", () => { }) },
			new CommandItem() { Command = new Command("id2", "title2", () => { }) },
			new CommandItem() { Command = new Command("id3", "title3", () => { }) }
		};

		commandManager.Setup(cm => cm.GetEnumerator()).Returns(items.GetEnumerator());

		plugin.Config.ActivationConfig = new MenuVariantConfig();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		// When
		bool result = vm.OnKeyDown(this, key);

		// Then
		Assert.Equal(expectedIndex, vm.SelectedIndex);
		Assert.True(result);
	}

	[Fact]
	public void OnKeyDown_UnhandledKeys()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		IEnumerable<CommandItem> items = new List<CommandItem>()
		{
			new CommandItem() { Command = new Command("id", "title", () => { }) },
			new CommandItem() { Command = new Command("id2", "title2", () => { }) },
			new CommandItem() { Command = new Command("id3", "title3", () => { }) }
		};

		commandManager.Setup(cm => cm.GetEnumerator()).Returns(items.GetEnumerator());

		plugin.Config.ActivationConfig = new MenuVariantConfig();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		// When
		bool result = vm.OnKeyDown(this, Windows.System.VirtualKey.A);

		// Then
		Assert.Equal(0, vm.SelectedIndex);
		Assert.False(result);
	}

	[Fact]
	public void PopulateItems_CannotExecute()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		// When
		vm.PopulateItems(
			new List<CommandItem>()
			{
				new CommandItem() { Command = new Command("id", "title", () => { }, () => false) },
			}
		);

		// Then
		Assert.Empty(vm._allCommands);
	}

	[Fact]
	public void PopulateItems_AddNew()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		// When
		vm.PopulateItems(
			new List<CommandItem>() { new CommandItem() { Command = new Command("id", "title", () => { }) }, }
		);

		// Then
		Assert.Single(vm._allCommands);
	}

	[Fact]
	public void PopulateItems_UpdateExisting()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		vm.PopulateItems(
			new List<CommandItem>() { new CommandItem() { Command = new Command("id", "title", () => { }) }, }
		);

		// When
		vm.PopulateItems(
			new List<CommandItem>() { new CommandItem() { Command = new Command("id", "new title", () => { }) }, }
		);

		// Then
		Assert.Single(vm._allCommands);
		Assert.Equal("new title", vm._allCommands[0].Command.Title);
	}

	[Fact]
	public void PopulateItems_RemoveExtra()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		vm.PopulateItems(
			new List<CommandItem>() { new CommandItem() { Command = new Command("id", "title", () => { }) }, }
		);

		// When
		vm.PopulateItems(new List<CommandItem>());

		// Then
		Assert.Empty(vm._allCommands);
	}

	[Fact]
	public void PopulateItems_SkipEqualCommand()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		Command command = new("id", "title", () => { });
		CommandItem commandItem = new() { Command = command };
		vm.PopulateItems(new List<CommandItem>() { commandItem, });

		// When
		vm.PopulateItems(new List<CommandItem>() { new CommandItem() { Command = command }, });

		// Then
		Assert.Single(vm._allCommands);
		Assert.Equal(commandItem, vm._allCommands[0]);
	}

	[Fact]
	public void Update_Menu_NoMatches()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		vm.Activate(new MenuVariantConfig(), null, null);

		// When
		vm.Update();

		// Then
		Assert.Equal(Visibility.Visible, vm.ListViewItemsWrapperVisibility);
		Assert.Equal(Visibility.Visible, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Collapsed, vm.ListViewItemsVisibility);
	}

	[Fact]
	public void Update_Menu_SomeMatches()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		vm.Activate(CreateMenuActivationConfig(3), null, null);

		// When
		vm.Update();

		// Then
		Assert.Equal(Visibility.Visible, vm.ListViewItemsWrapperVisibility);
		Assert.Equal(Visibility.Collapsed, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Visible, vm.ListViewItemsVisibility);
		Assert.Equal(3, vm.PaletteRows.Count);
	}

	[Fact]
	public void Update_Menu_RemoveUnused()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		vm.Activate(CreateMenuActivationConfig(3), null, null);
		vm.Activate(CreateMenuActivationConfig(2), null, null);

		// When
		vm.Activate(CreateMenuActivationConfig(3), null, null);
		vm.Update();

		// Then
		Assert.Equal(Visibility.Visible, vm.ListViewItemsWrapperVisibility);
		Assert.Equal(Visibility.Collapsed, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Visible, vm.ListViewItemsVisibility);
		Assert.Equal(3, vm.PaletteRows.Count);

		// First and second element should have been updated
		Assert.True(vm.PaletteRows[0] is PaletteRowStub stub && stub.IsUpdated);
		Assert.True(vm.PaletteRows[1] is PaletteRowStub stub2 && stub2.IsUpdated);
	}

	[Fact]
	public void Update_Free()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);

		vm.Activate(new FreeTextVariantConfig() { Callback = (text) => { } }, null, null);

		// When
		vm.Update();

		// Then
		Assert.Equal(Visibility.Collapsed, vm.ListViewItemsWrapperVisibility);
		Assert.Equal(Visibility.Collapsed, vm.NoMatchingCommandsTextBlockVisibility);
		Assert.Equal(Visibility.Collapsed, vm.ListViewItemsVisibility);
	}

	private static PaletteRowText CreatePaletteRowText(string text)
	{
		PaletteRowText rowText = new();
		rowText.Segments.Add(new TextSegment(Text: text, IsHighlighted: false));
		return rowText;
	}

	private static MenuVariantConfig CreateMenuActivationConfig(int itemCount)
	{
		List<PaletteRowItem> items = new();

		for (int i = 0; i < itemCount; i++)
		{
			items.Add(
				new PaletteRowItem(
					CommandItem: new CommandItem() { Command = new Command($"id{i}", $"title{i}", () => { }) },
					Title: CreatePaletteRowText($"title{i}")
				)
			);
		}

		Mock<IMatcher> matcher = new();
		matcher.Setup(m => m.Match(It.IsAny<string>(), It.IsAny<ICollection<CommandItem>>())).Returns(items);

		MenuVariantConfig config = new() { Matcher = matcher.Object, };

		return config;
	}

	[Fact]
	public void LoadMenuMatches_AddRows()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);
		string query = "ti";
		MenuVariantConfig config = CreateMenuActivationConfig(2);

		// When
		vm.LoadMenuMatches(query, config);

		// Then
		Assert.Equal(2, vm.PaletteRows.Count);
		Assert.Equal("title0", vm.PaletteRows[0].Model.CommandItem.Command.Title);
		Assert.Equal("title1", vm.PaletteRows[1].Model.CommandItem.Command.Title);
	}

	[Fact]
	public void LoadMenuMatches_UpdateRows()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm = new(configContext.Object, plugin, PaletteRowFactory);
		string query = "ti";

		vm.LoadMenuMatches(query, CreateMenuActivationConfig(2));

		// When
		vm.LoadMenuMatches(query, CreateMenuActivationConfig(2));

		// Then
		Assert.Equal(2, vm.PaletteRows.Count);
		vm.PaletteRows
			.Should()
			.AllSatisfy(r =>
			{
				Assert.True(r is PaletteRowStub row && row.IsUpdated);
			});
	}

	[Fact]
	public void ExecuteCommand_Free()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		string? callbackText = null;
		FreeTextVariantConfig config =
			new()
			{
				InitialText = "text",
				Callback = (text) =>
				{
					callbackText = text;
				}
			};
		plugin.Config.ActivationConfig = config;

		CommandPaletteWindowViewModel vm =
			new(
				configContext.Object,
				plugin,
				(rowItem) =>
				{
					Mock<IPaletteRow> paletteRow = new();
					paletteRow.Setup(r => r.Model).Returns(rowItem);
					return paletteRow.Object;
				}
			);

		vm.Activate(config, null, null);

		// When
		Assert.Raises<EventArgs>(h => vm.HideRequested += h, h => vm.HideRequested -= h, vm.ExecuteCommand);

		// Then
		Assert.Equal("text", callbackText);
	}

	[Fact]
	public void ExecuteCommand_Menu()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

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

		MenuVariantConfig config = new();
		plugin.Config.ActivationConfig = config;

		CommandPaletteWindowViewModel vm =
			new(
				configContext.Object,
				plugin,
				(rowItem) =>
				{
					Mock<IPaletteRow> paletteRow = new();
					paletteRow.Setup(r => r.Model).Returns(rowItem);
					return paletteRow.Object;
				}
			);

		// When
		Assert.Raises<EventArgs>(h => vm.HideRequested += h, h => vm.HideRequested -= h, vm.ExecuteCommand);

		// Then
		Assert.True(called);
	}

	[Fact]
	public void ExecuteCommand_Menu_Reuse()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel? vm = null;

		int called = 0;
		string callbackText = string.Empty;
		IEnumerable<CommandItem> items = new List<CommandItem>()
		{
			new CommandItem()
			{
				Command = new Command(
					"id",
					"title",
					() =>
					{
						called++;
						vm?.Activate(
							new FreeTextVariantConfig()
							{
								InitialText = "free text",
								Callback = (text) =>
								{
									callbackText = text;
								}
							},
							null,
							null
						);
					}
				)
			},
		};

		commandManager.Setup(cm => cm.GetEnumerator()).Returns(items.GetEnumerator());

		MenuVariantConfig config = new();
		plugin.Config.ActivationConfig = config;

		vm = new(
			configContext.Object,
			plugin,
			(rowItem) =>
			{
				Mock<IPaletteRow> paletteRow = new();
				paletteRow.Setup(r => r.Model).Returns(rowItem);
				return paletteRow.Object;
			}
		);

		bool hideRequested = false;
		vm.HideRequested += (sender, args) =>
		{
			hideRequested = true;
		};

		// For the first execution, show the menu
		// When
		vm.ExecuteCommand();

		// Then
		Assert.Equal(1, called);
		Assert.Empty(callbackText);
		Assert.False(hideRequested);

		// For the second execution, show the free text
		// When
		vm.ExecuteCommand();

		// Then
		Assert.Equal(1, called);
		Assert.Equal("free text", callbackText);
		Assert.True(hideRequested);
	}
}
