using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class FreeTextVariantViewModelCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		ICommandPaletteWindowViewModel windowViewModel = fixture.Freeze<ICommandPaletteWindowViewModel>();
		windowViewModel.Text = "Hello, world!";

		FreeTextVariantCallback callback = Substitute.For<FreeTextVariantCallback>();
		fixture.Inject(callback);

		FreeTextVariantConfig config =
			new()
			{
				InitialText = "Hello, world!",
				Callback = callback,
				Prompt = "Hello, world!"
			};
		fixture.Inject(config);
	}
}

public class FreeTextVariantViewModelTests
{
	[Theory, AutoSubstituteData<FreeTextVariantViewModelCustomization>]
	internal void NotFreeTextVariantConfig(ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		FreeTextVariantViewModel vm = new(windowViewModel);

		// When
		vm.Activate(new UnknownConfig());

		// Then
		Assert.Equal(string.Empty, vm.Prompt);
		Assert.Null(vm.ConfirmButtonText);
	}

	[Theory, AutoSubstituteData<FreeTextVariantViewModelCustomization>]
	internal void PromptDefaultsToEmptyString(ICommandPaletteWindowViewModel windowViewModel)
	{
		// Given
		FreeTextVariantViewModel vm = new(windowViewModel);

		// When
		string prompt = vm.Prompt;

		// Then
		Assert.Equal("", prompt);
	}

	[Theory, AutoSubstituteData<FreeTextVariantViewModelCustomization>]
	internal void OnKeyDown_Enter(
		ICommandPaletteWindowViewModel windowViewModel,
		FreeTextVariantConfig config,
		FreeTextVariantCallback callback
	)
	{
		// Given
		FreeTextVariantViewModel vm = new(windowViewModel);

		vm.Activate(config);

		// When
		vm.OnKeyDown(VirtualKey.Enter);

		// Then
		callback.Received(1).Invoke("Hello, world!");
		windowViewModel.Received(1).RequestHide();
	}

	[Theory, AutoSubstituteData<FreeTextVariantViewModelCustomization>]
	internal void OnKeyDown_Enter_NoActivationConfig(
		ICommandPaletteWindowViewModel windowViewModel,
		FreeTextVariantCallback callback
	)
	{
		// Given
		FreeTextVariantViewModel vm = new(windowViewModel);

		// When
		vm.OnKeyDown(VirtualKey.Enter);

		// Then
		callback.DidNotReceive().Invoke("Hello, world!");
	}

	[Theory, AutoSubstituteData<FreeTextVariantViewModelCustomization>]
	internal void OnKeyDown_NotEnter(
		ICommandPaletteWindowViewModel windowViewModel,
		FreeTextVariantConfig config,
		FreeTextVariantCallback callback
	)
	{
		// Given
		FreeTextVariantViewModel vm = new(windowViewModel);

		vm.Activate(config);

		// When
		vm.OnKeyDown(VirtualKey.Escape);

		// Then
		callback.DidNotReceive().Invoke("Hello, world!");
	}

	[Theory, AutoSubstituteData<FreeTextVariantViewModelCustomization>]
	internal void Confirm(
		ICommandPaletteWindowViewModel windowViewModel,
		FreeTextVariantConfig config,
		FreeTextVariantCallback callback
	)
	{
		// Given
		FreeTextVariantViewModel vm = new(windowViewModel);

		vm.Activate(config);

		// When
		vm.Confirm();

		// Then
		callback.Received(1).Invoke("Hello, world!");
		windowViewModel.Received(1).RequestHide();
	}
}
