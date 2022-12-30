using Moq;
using Xunit;

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
		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, (rowItem) => new Mock<IPaletteRow>().Object);

		// Then
		Assert.Single(vm.PaletteRows);
	}

	[Fact]
	public void Activate_UseDefaults()
	{
		// Given
		(Mock<IConfigContext> configContext, Mock<ICommandManager> commandManager, CommandPalettePlugin plugin) =
			CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, (rowItem) => new Mock<IPaletteRow>().Object);

		// When
		Assert.Raises<EventArgs>(
			h => vm.SetWindowPosRequested += h,
			h => vm.SetWindowPosRequested -= h,
			() => vm.Activate(new BaseCommandPaletteActivationConfig(), new List<CommandItem>(), null)
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

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, (rowItem) => new Mock<IPaletteRow>().Object);

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

		BaseCommandPaletteActivationConfig config = new() { Hint = "Hint", InitialText = "Initial text" };

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

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, (rowItem) => new Mock<IPaletteRow>().Object);

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

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, (rowItem) => new Mock<IPaletteRow>().Object);

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
		CommandPaletteFreeTextActivationConfig config =
			new() { InitialText = "Hello, world!", Callback = (text) => callbackText = text };
		plugin.Config.ActivationConfig = config;

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, (rowItem) => new Mock<IPaletteRow>().Object);

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

		plugin.Config.ActivationConfig = new CommandPaletteMenuActivationConfig();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, (rowItem) => new Mock<IPaletteRow>().Object);

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

		plugin.Config.ActivationConfig = new CommandPaletteMenuActivationConfig();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, (rowItem) => new Mock<IPaletteRow>().Object);

		// When
		bool result = vm.OnKeyDown(this, Windows.System.VirtualKey.A);

		// Then
		Assert.Equal(0, vm.SelectedIndex);
		Assert.False(result);
	}

	[Fact]
	public void UpdateMatches()
	{
		// TODO
	}

	[Fact]
	public void ExecuteCommand()
	{
		// TODO
	}
}
