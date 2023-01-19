using Microsoft.UI.Xaml;
using Moq;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public record UnknownConfig : BaseVariantConfig { }

public class CommandPaletteWindowViewModelTests
{
	private static (
		Mock<IConfigContext>,
		Mock<ICommandManager>,
		CommandPalettePlugin,
		Mock<IVariantControl>,
		Mock<IVariantControl>,
		Mock<IVariantViewModel>
	) CreateStubs()
	{
		Mock<ICommandManager> commandManager = new();
		commandManager.Setup(x => x.GetEnumerator()).Returns(new List<CommandItem>().GetEnumerator());

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
		configContext.SetupGet(x => x.CommandManager).Returns(commandManager.Object);
		configContext.SetupGet(x => x.MonitorManager).Returns(monitorManager.Object);

		CommandPalettePlugin plugin = new(configContext.Object, new CommandPaletteConfig());

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Mock<IVariantViewModel> variantViewModel = new();

		Mock<IVariantControl> menuVariant = new();
		menuVariant.Setup(m => m.Control).Returns((UIElement)null);
		menuVariant.Setup(m => m.ViewModel).Returns(variantViewModel.Object);

		Mock<IVariantControl> freeTextVariant = new();
		freeTextVariant.Setup(m => m.Control).Returns((UIElement)null);
		freeTextVariant.Setup(m => m.ViewModel).Returns(variantViewModel.Object);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

		return (configContext, commandManager, plugin, menuVariant, freeTextVariant, variantViewModel);
	}

	[Fact]
	public void RequestHide()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			CommandPalettePlugin plugin,
			Mock<IVariantControl> menuVariant,
			Mock<IVariantControl> freeTextVariant,
			Mock<IVariantViewModel> viewModel
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		// When
		// Then
		Assert.Raises<EventArgs>(h => vm.HideRequested += h, h => vm.HideRequested -= h, vm.RequestHide);
	}

	[Fact]
	public void OnKeyDown_Escape()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			CommandPalettePlugin plugin,
			Mock<IVariantControl> menuVariant,
			Mock<IVariantControl> freeTextVariant,
			Mock<IVariantViewModel> viewModel
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		// When
		// Then
		Assert.Raises<EventArgs>(
			h => vm.HideRequested += h,
			h => vm.HideRequested -= h,
			() => vm.OnKeyDown(VirtualKey.Escape)
		);
	}

	[Fact]
	public void OnKeyDown_OtherKeys()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			CommandPalettePlugin plugin,
			Mock<IVariantControl> menuVariant,
			Mock<IVariantControl> freeTextVariant,
			Mock<IVariantViewModel> viewModel
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		vm.Activate(new MenuVariantConfig() { Commands = Array.Empty<CommandItem>() }, null);

		// When
		vm.OnKeyDown(VirtualKey.Space);

		// Then
		viewModel.Verify(x => x.OnKeyDown(VirtualKey.Space), Times.Once);
	}

	[Fact]
	public void Activate_UseDefaults()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			CommandPalettePlugin plugin,
			Mock<IVariantControl> menuVariant,
			Mock<IVariantControl> freeTextVariant,
			Mock<IVariantViewModel> viewModel
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		// When
		Assert.Raises<EventArgs>(
			h => vm.SetWindowPosRequested += h,
			h => vm.SetWindowPosRequested -= h,
			() => vm.Activate(new MenuVariantConfig() { Commands = Array.Empty<CommandItem>() }, null)
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
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			CommandPalettePlugin plugin,
			Mock<IVariantControl> menuVariant,
			Mock<IVariantControl> freeTextVariant,
			Mock<IVariantViewModel> viewModel
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

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

		MenuVariantConfig config =
			new()
			{
				Commands = Array.Empty<CommandItem>(),
				InitialText = "Initial text",
				Hint = "Hint"
			};

		IEnumerable<CommandItem> commandItems = new List<CommandItem>()
		{
			new CommandItem() { Command = new Command("id", "title", () => { }) }
		};

		// When
		Assert.Raises<EventArgs>(
			h => vm.SetWindowPosRequested += h,
			h => vm.SetWindowPosRequested -= h,
			() => vm.Activate(config, monitor.Object)
		);

		// Then
		Assert.Equal("Initial text", vm.Text);
		Assert.Equal("Hint", vm.PlaceholderText);
		Assert.Equal((int)(100 * 0.4), vm.MaxHeight);
	}

	public static IEnumerable<object[]> ActivateData()
	{
		yield return new object[] { new UnknownConfig(), false, false };
		yield return new object[]
		{
			new MenuVariantConfig() { Commands = Array.Empty<CommandItem>() },
			true,
			false
		};
		yield return new object[]
		{
			new FreeTextVariantConfig() { Callback = (text) => { }, Prompt = "Prompt" },
			true,
			false
		};
		yield return new object[]
		{
			new SelectVariantConfig() { Options = Array.Empty<SelectOption>(), Callback = (items) => { } },
			true,
			true
		};
		yield return new object[]
		{
			new SelectVariantConfig() { Options = Array.Empty<SelectOption>(), Callback = (items) => { } },
			true,
			true
		};
	}

	[Theory]
	[MemberData(nameof(ActivateData))]
	public void Activate_Variant(BaseVariantConfig config, bool expected, bool showSaveButton)
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			_,
			CommandPalettePlugin plugin,
			Mock<IVariantControl> menuVariant,
			Mock<IVariantControl> freeTextVariant,
			_
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		// When
		vm.Activate(config, null);

		// Then
		Assert.Equal(expected, vm.IsVariantActive(config));
		Assert.Equal(showSaveButton ? Visibility.Visible : Visibility.Collapsed, vm.SaveButtonVisibility);
	}

	[Fact]
	public void Update()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			CommandPalettePlugin plugin,
			Mock<IVariantControl> menuVariant,
			Mock<IVariantControl> freeTextVariant,
			Mock<IVariantViewModel> viewModel
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		MenuVariantConfig config = new() { Commands = Array.Empty<CommandItem>() };

		vm.Activate(config, null);

		// When
		Assert.Raises<EventArgs>(h => vm.SetWindowPosRequested += h, h => vm.SetWindowPosRequested -= h, vm.Update);

		// Then
		viewModel.Verify(x => x.Update(), Times.Once);
	}

	[Fact]
	public void IsVisible_True()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			_,
			CommandPalettePlugin plugin,
			Mock<IVariantControl> menuVariant,
			Mock<IVariantControl> freeTextVariant,
			_
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		MenuVariantConfig config = new() { Commands = Array.Empty<CommandItem>() };

		vm.Activate(config, null);

		// When
		bool result = vm.IsVisible;

		// Then
		Assert.True(result);
	}

	[Fact]
	public void IsVisible_False()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			_,
			CommandPalettePlugin plugin,
			Mock<IVariantControl> menuVariant,
			Mock<IVariantControl> freeTextVariant,
			_
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		// When
		bool result = vm.IsVisible;

		// Then
		Assert.False(result);
	}
}
