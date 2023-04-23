using Microsoft.UI.Xaml;
using Moq;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public record UnknownConfig : BaseVariantConfig { }

public class CommandPaletteWindowViewModelTests
{
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<ICommandManager> CommandManager { get; } = new();
		public CommandPalettePlugin Plugin { get; }
		public Mock<IVariantControl> MenuVariant { get; } = new();
		public Mock<IVariantControl> FreeTextVariant { get; } = new();
		public Mock<IVariantControl> SelectVariant { get; } = new();
		public Mock<IVariantViewModel> VariantViewModel { get; } = new();

		public MocksBuilder()
		{
			CommandManager.Setup(x => x.GetEnumerator()).Returns(new List<CommandItem>().GetEnumerator());

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

			Context.SetupGet(x => x.CommandManager).Returns(CommandManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(monitorManager.Object);

			Plugin = new(Context.Object, new CommandPaletteConfig(Context.Object));

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			MenuVariant.Setup(m => m.Control).Returns((UIElement)null);
			MenuVariant.Setup(m => m.ViewModel).Returns(VariantViewModel.Object);

			FreeTextVariant.Setup(m => m.Control).Returns((UIElement)null);
			FreeTextVariant.Setup(m => m.ViewModel).Returns(VariantViewModel.Object);

			VariantViewModel.Setup(m => m.ConfirmButtonText).Returns("Save");

			SelectVariant.Setup(m => m.Control).Returns((UIElement)null);
			SelectVariant.Setup(m => m.ViewModel).Returns(VariantViewModel.Object);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
		}
	}

	[Fact]
	public void RequestHide()
	{
		// Given
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

		// When
		// Then
		Assert.Raises<EventArgs>(h => vm.HideRequested += h, h => vm.HideRequested -= h, vm.RequestHide);
	}

	[Fact]
	public void OnKeyDown_Escape()
	{
		// Given
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

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
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

		vm.Activate(new MenuVariantConfig() { Commands = Array.Empty<CommandItem>() }, null);

		// When
		vm.OnKeyDown(VirtualKey.Space);

		// Then
		mocks.VariantViewModel.Verify(x => x.OnKeyDown(VirtualKey.Space), Times.Once);
	}

	[Fact]
	public void Activate_UseDefaults()
	{
		// Given
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

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
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

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
		yield return new object[] { new UnknownConfig(), false, "Confirm" };
		yield return new object[]
		{
			new MenuVariantConfig() { Commands = Array.Empty<CommandItem>() },
			true,
			"Confirm"
		};
		yield return new object[]
		{
			new FreeTextVariantConfig() { Callback = (text) => { }, Prompt = "Prompt" },
			true,
			"Confirm"
		};
		yield return new object[]
		{
			new SelectVariantConfig()
			{
				Options = Array.Empty<SelectOption>(),
				Callback = (items) => { },
				ConfirmButtonText = "Save"
			},
			true,
			"Save"
		};
	}

	[Theory]
	[MemberData(nameof(ActivateData))]
	public void Activate_Variant(BaseVariantConfig config, bool expected, string confirmButtonText)
	{
		// Given
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

		// When
		vm.Activate(config, null);

		// Then
		Assert.Equal(expected, vm.IsConfigActive(config));
		Assert.Equal(confirmButtonText, vm.ConfirmButtonText);
	}

	[Fact]
	public void Update()
	{
		// Given
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

		MenuVariantConfig config = new() { Commands = Array.Empty<CommandItem>() };

		vm.Activate(config, null);

		// When
		Assert.Raises<EventArgs>(h => vm.SetWindowPosRequested += h, h => vm.SetWindowPosRequested -= h, vm.Update);

		// Then
		mocks.VariantViewModel.Verify(x => x.Update(), Times.Once);
	}

	[Fact]
	public void IsVisible_True()
	{
		// Given
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

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
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

		// When
		bool result = vm.IsVisible;

		// Then
		Assert.False(result);
	}

	[Fact]
	public void RequestFocusTextBox()
	{
		// Given
		MocksBuilder mocks = new();

		CommandPaletteWindowViewModel vm =
			new(
				mocks.Context.Object,
				mocks.Plugin,
				mocks.MenuVariant.Object,
				mocks.FreeTextVariant.Object,
				mocks.SelectVariant.Object
			);

		// When
		// Then
		Assert.Raises<EventArgs>(
			h => vm.FocusTextBoxRequested += h,
			h => vm.FocusTextBoxRequested -= h,
			vm.RequestFocusTextBox
		);
	}
}
