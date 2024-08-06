using Microsoft.UI.Xaml;
using NSubstitute;
using Whim.TestUtils;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public record UnknownConfig : BaseVariantConfig { }

public class CommandPaletteWindowViewModelTests
{
	private class Wrapper
	{
		public IContext Context { get; } = Substitute.For<IContext>();
		public CommandPalettePlugin Plugin { get; }
		public IVariantControl MenuVariant { get; } = Substitute.For<IVariantControl>();
		public IVariantControl FreeTextVariant { get; } = Substitute.For<IVariantControl>();
		public IVariantControl SelectVariant { get; } = Substitute.For<IVariantControl>();
		public IVariantViewModel VariantViewModel { get; } = Substitute.For<IVariantViewModel>();

		public Wrapper()
		{
			IMonitor monitor = Substitute.For<IMonitor>();
			monitor.WorkingArea.Returns(new Rectangle<int>() { Height = 1080, Width = 1920 });

			Context.MonitorManager.ActiveMonitor.Returns(monitor);

			Plugin = new(Context, new CommandPaletteConfig(Context));

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
			MenuVariant.Control.Returns((UIElement)null);
			MenuVariant.ViewModel.Returns(VariantViewModel);

			FreeTextVariant.Control.Returns((UIElement)null);
			FreeTextVariant.ViewModel.Returns(VariantViewModel);

			VariantViewModel.ConfirmButtonText.Returns("Save");

			SelectVariant.Control.Returns((UIElement)null);
			SelectVariant.ViewModel.Returns(VariantViewModel);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
		}
	}

	[Fact]
	public void RequestHide()
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

		// When
		// Then
		Assert.Raises<EventArgs>(h => vm.HideRequested += h, h => vm.HideRequested -= h, vm.RequestHide);
	}

	[Fact]
	public void OnKeyDown_Escape()
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

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
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

		vm.Activate(new MenuVariantConfig() { Commands = [] }, null);

		// When
		vm.OnKeyDown(VirtualKey.Space);

		// Then
		// mocks.VariantViewModel.Verify(x => x.OnKeyDown(VirtualKey.Space), Times.Once);
	}

	[Fact]
	public void Activate_UseDefaults()
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

		// When
		Assert.Raises<EventArgs>(
			h => vm.SetWindowPosRequested += h,
			h => vm.SetWindowPosRequested -= h,
			() => vm.Activate(new MenuVariantConfig() { Commands = [] }, null)
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
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

		IMonitor monitor = Substitute.For<IMonitor>();
		monitor.WorkingArea.Returns(
			new Rectangle<int>()
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
				Commands = [],
				InitialText = "Initial text",
				Hint = "Hint"
			};

		IEnumerable<ICommand> ICommands = [new Command("id", "title", () => { })];

		// When
		Assert.Raises<EventArgs>(
			h => vm.SetWindowPosRequested += h,
			h => vm.SetWindowPosRequested -= h,
			() => vm.Activate(config, monitor)
		);

		// Then
		Assert.Equal("Initial text", vm.Text);
		Assert.Equal("Hint", vm.PlaceholderText);
		Assert.Equal((int)(100 * 0.4), vm.MaxHeight);
	}

	public static TheoryData<BaseVariantConfig, bool, string> ActivateData =>
		new()
		{
			{ new UnknownConfig(), false, "Confirm" },
			{
				new MenuVariantConfig() { Commands = [] },
				true,
				"Confirm"
			},
			{
				new FreeTextVariantConfig() { Callback = (text) => { }, Prompt = "Prompt" },
				true,
				"Confirm"
			},
			{
				new SelectVariantConfig()
				{
					Options = [],
					Callback = (items) => { },
					ConfirmButtonText = "Save"
				},
				true,
				"Save"
			}
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(ActivateData))]
	public void Activate_Variant(BaseVariantConfig config, bool expected, string confirmButtonText)
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

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
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

		MenuVariantConfig config = new() { Commands = [] };

		vm.Activate(config, null);

		// When
		Assert.Raises<EventArgs>(h => vm.SetWindowPosRequested += h, h => vm.SetWindowPosRequested -= h, vm.Update);

		// Then
		wrapper.VariantViewModel.Received(1).Update();
	}

	[Fact]
	public void IsVisible_True()
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

		MenuVariantConfig config = new() { Commands = [] };

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
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

		// When
		bool result = vm.IsVisible;

		// Then
		Assert.False(result);
	}

	[Fact]
	public void RequestFocusTextBox()
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteWindowViewModel vm =
			new(wrapper.Context, wrapper.Plugin, wrapper.MenuVariant, wrapper.FreeTextVariant, wrapper.SelectVariant);

		// When
		// Then
		Assert.Raises<EventArgs>(
			h => vm.FocusTextBoxRequested += h,
			h => vm.FocusTextBoxRequested -= h,
			vm.RequestFocusTextBox
		);
	}
}
