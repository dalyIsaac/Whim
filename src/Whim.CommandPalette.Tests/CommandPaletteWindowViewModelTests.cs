using AutoFixture;
using Microsoft.UI.Xaml;
using NSubstitute;
using Whim.TestUtils;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public record UnknownConfig : BaseVariantConfig { }

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class CommandPaletteWindowViewModelTests
{
	private class Customization : StoreCustomization
	{
		protected override void PostCustomize(IFixture fixture)
		{
			IMonitor monitor = StoreTestUtils.CreateMonitor();
			fixture.Inject(monitor);

			StoreTestUtils.AddMonitorsToManager(_ctx, _store._root.MutableRootSector, monitor);

			fixture.Inject(new CommandPalettePlugin(_ctx, new CommandPaletteConfig(_ctx)));

			IVariantViewModel variantViewModel = Substitute.For<IVariantViewModel>();
			variantViewModel.ConfirmButtonText.Returns("Save");
			fixture.Inject(variantViewModel);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
			IVariantControl menuVariant = Substitute.For<IVariantControl>();
			menuVariant.Control.Returns((UIElement)null);
			menuVariant.ViewModel.Returns(variantViewModel);

			IVariantControl freeTextVariant = Substitute.For<IVariantControl>();
			freeTextVariant.Control.Returns((UIElement)null);
			freeTextVariant.ViewModel.Returns(variantViewModel);

			IVariantControl selectVariant = Substitute.For<IVariantControl>();
			selectVariant.Control.Returns((UIElement)null);
			selectVariant.ViewModel.Returns(variantViewModel);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

			fixture.Inject(
				new CommandPaletteWindowViewModel(
					_ctx,
					fixture.Create<CommandPalettePlugin>(),
					menuVariant,
					freeTextVariant,
					selectVariant
				)
			);
		}
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void RequestHide(CommandPaletteWindowViewModel vm)
	{
		Assert.Raises<EventArgs>(h => vm.HideRequested += h, h => vm.HideRequested -= h, vm.RequestHide);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void OnKeyDown_Escape(CommandPaletteWindowViewModel vm)
	{
		Assert.Raises<EventArgs>(
			h => vm.HideRequested += h,
			h => vm.HideRequested -= h,
			() => vm.OnKeyDown(VirtualKey.Escape)
		);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void OnKeyDown_OtherKeys(CommandPaletteWindowViewModel vm, IVariantViewModel variantViewModel)
	{
		// Given
		vm.Activate(new MenuVariantConfig() { Commands = [] }, null);

		// When
		vm.OnKeyDown(VirtualKey.Space);

		// Then
		variantViewModel.Received(1).OnKeyDown(VirtualKey.Space);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Activate_UseDefaults(CommandPaletteWindowViewModel vm)
	{
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

	[Theory, AutoSubstituteData<Customization>]
	internal void Activate_DefineItems(CommandPaletteWindowViewModel vm)
	{
		// Given
		IMonitor monitor = Substitute.For<IMonitor>();
		monitor.WorkingArea.Returns(
			new Rectangle<int>()
			{
				X = 0,
				Y = 0,
				Height = 100,
				Width = 100,
			}
		);

		MenuVariantConfig config = new()
		{
			Commands = [],
			InitialText = "Initial text",
			Hint = "Hint",
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
					ConfirmButtonText = "Save",
				},
				true,
				"Save"
			},
		};

	[Theory]
	[MemberAutoSubstituteData<Customization>(nameof(ActivateData))]
	internal void Activate_Variant(
		BaseVariantConfig config,
		bool expected,
		string confirmButtonText,
		CommandPaletteWindowViewModel vm
	)
	{
		// When
		vm.Activate(config, null);

		// Then
		Assert.Equal(expected, vm.IsConfigActive(config));
		Assert.Equal(confirmButtonText, vm.ConfirmButtonText);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Update(CommandPaletteWindowViewModel vm, IVariantViewModel variantViewModel)
	{
		// Given
		MenuVariantConfig config = new() { Commands = [] };

		vm.Activate(config, null);

		// When
		Assert.Raises<EventArgs>(h => vm.SetWindowPosRequested += h, h => vm.SetWindowPosRequested -= h, vm.Update);

		// Then
		variantViewModel.Received(1).Update();
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void IsVisible_True(CommandPaletteWindowViewModel vm)
	{
		// Given
		MenuVariantConfig config = new() { Commands = [] };

		vm.Activate(config, null);

		// When
		bool result = vm.IsVisible;

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void IsVisible_False(CommandPaletteWindowViewModel vm)
	{
		// When
		bool result = vm.IsVisible;

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void RequestFocusTextBox(CommandPaletteWindowViewModel vm)
	{
		// When
		// Then
		Assert.Raises<EventArgs>(
			h => vm.FocusTextBoxRequested += h,
			h => vm.FocusTextBoxRequested -= h,
			vm.RequestFocusTextBox
		);
	}
}
