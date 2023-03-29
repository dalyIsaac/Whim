using Moq;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class FreeTextVariantViewModelTests
{
	private class MocksBuilder
	{
		public Mock<FreeTextVariantCallback> Callback { get; } = new();
		public FreeTextVariantConfig Config { get; }
		public Mock<ICommandPaletteWindowViewModel> WindowViewModel { get; } = new();

		public MocksBuilder()
		{
			Config = new FreeTextVariantConfig()
			{
				InitialText = "Hello, world!",
				Callback = Callback.Object,
				Prompt = "Hello, world!"
			};

			WindowViewModel.Setup(wvm => wvm.Text).Returns("Hello, world!");
		}
	}

	[Fact]
	public void NotFreeTextVariantConfig()
	{
		// Given
		MocksBuilder mocks = new();
		FreeTextVariantViewModel vm = new(mocks.WindowViewModel.Object);

		// When
		vm.Activate(new UnknownConfig());

		// Then
		Assert.Equal(string.Empty, vm.Prompt);
		Assert.Null(vm.ConfirmButtonText);
	}

	[Fact]
	public void PromptDefaultsToEmptyString()
	{
		// Given
		MocksBuilder mocks = new();
		FreeTextVariantViewModel vm = new(mocks.WindowViewModel.Object);

		// When
		string prompt = vm.Prompt;

		// Then
		Assert.Equal("", prompt);
	}

	[Fact]
	public void OnKeyDown_Enter()
	{
		// Given
		MocksBuilder mocks = new();
		FreeTextVariantViewModel vm = new(mocks.WindowViewModel.Object);

		vm.Activate(mocks.Config);

		// When
		vm.OnKeyDown(VirtualKey.Enter);

		// Then
		mocks.Callback.Verify(c => c("Hello, world!"), Times.Once);
		mocks.WindowViewModel.Verify(w => w.RequestHide(), Times.Once);
	}

	[Fact]
	public void OnKeyDown_Enter_NoActivationConfig()
	{
		// Given
		MocksBuilder mocks = new();
		FreeTextVariantViewModel vm = new(mocks.WindowViewModel.Object);

		// When
		vm.OnKeyDown(VirtualKey.Enter);

		// Then
		mocks.Callback.Verify(c => c("Hello, world!"), Times.Never);
	}

	[Fact]
	public void OnKeyDown_NotEnter()
	{
		// Given
		MocksBuilder mocks = new();
		FreeTextVariantViewModel vm = new(mocks.WindowViewModel.Object);

		vm.Activate(mocks.Config);

		// When
		vm.OnKeyDown(VirtualKey.Escape);

		// Then
		mocks.Callback.Verify(c => c("Hello, world!"), Times.Never);
	}

	[Fact]
	public void Confirm()
	{
		// Given
		MocksBuilder mocks = new();
		FreeTextVariantViewModel vm = new(mocks.WindowViewModel.Object);

		vm.Activate(mocks.Config);

		// When
		vm.Confirm();

		// Then
		mocks.Callback.Verify(c => c("Hello, world!"), Times.Once);
		mocks.WindowViewModel.Verify(w => w.RequestHide(), Times.Once);
	}
}
