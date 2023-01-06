using Moq;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class FreeTextVariantViewModelTests
{
	private static (
		Mock<FreeTextVariantCallback>,
		FreeTextVariantConfig,
		Mock<ICommandPaletteWindowViewModel>
	) CreateStubs()
	{
		Mock<FreeTextVariantCallback> callback = new();
		FreeTextVariantConfig config =
			new()
			{
				InitialText = "Hello, world!",
				Callback = callback.Object,
				Prompt = "Hello, world!"
			};

		Mock<ICommandPaletteWindowViewModel> windowViewModel = new();
		windowViewModel.Setup(wvm => wvm.Text).Returns("Hello, world!");

		return (callback, config, windowViewModel);
	}

	[Fact]
	public void OnKeyDown_Enter()
	{
		// Given
		(
			Mock<FreeTextVariantCallback> callback,
			FreeTextVariantConfig config,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();
		FreeTextVariantViewModel vm = new(windowViewModel.Object);

		vm.Activate(config);

		// When
		vm.OnKeyDown(VirtualKey.Enter);

		// Then
		callback.Verify(c => c("Hello, world!"), Times.Once);
		windowViewModel.Verify(w => w.RequestHide(), Times.Once);
	}

	[Fact]
	public void OnKeyDown_Enter_NoActivationConfig()
	{
		// Given
		(
			Mock<FreeTextVariantCallback> callback,
			FreeTextVariantConfig config,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();
		FreeTextVariantViewModel vm = new(windowViewModel.Object);

		// When
		vm.OnKeyDown(VirtualKey.Enter);

		// Then
		callback.Verify(c => c("Hello, world!"), Times.Never);
	}

	[Fact]
	public void OnKeyDown_NotEnter()
	{
		// Given
		(
			Mock<FreeTextVariantCallback> callback,
			FreeTextVariantConfig config,
			Mock<ICommandPaletteWindowViewModel> windowViewModel
		) = CreateStubs();
		FreeTextVariantViewModel vm = new(windowViewModel.Object);

		vm.Activate(config);

		// When
		vm.OnKeyDown(VirtualKey.Escape);

		// Then
		callback.Verify(c => c("Hello, world!"), Times.Never);
	}
}
