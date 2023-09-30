using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class ExecuteCommandTests
{
	[Theory, AutoSubstituteData]
	internal void CanExecute_WhenActiveVariantIsNull_ReturnsFalse(ICommandPaletteWindowViewModel viewModelMock)
	{
		// Given
		viewModelMock.ActiveVariant.Returns((IVariantControl?)null);

		// When
		ConfirmCommand command = new(viewModelMock);

		// Then
		Assert.False(command.CanExecute(null));
	}

	[Theory, AutoSubstituteData]
	internal void CanExecute_WhenActiveVariantIsNotNull_ReturnsTrue(ICommandPaletteWindowViewModel viewModelMock)
	{
		// Given
		viewModelMock.ActiveVariant.Returns(Substitute.For<IVariantControl>());

		// When
		ConfirmCommand command = new(viewModelMock);

		// Then
		Assert.True(command.CanExecute(null));
	}

	[Theory, AutoSubstituteData]
	internal void Execute_WhenActiveVariantIsNull_ReturnsEarly(ICommandPaletteWindowViewModel viewModelMock)
	{
		// Given
		viewModelMock.ActiveVariant.Returns((IVariantControl?)null);

		// When
		ConfirmCommand command = new(viewModelMock);
		command.Execute(null);

		// Then
		viewModelMock.DidNotReceive().RequestHide();
	}

	[Theory, AutoSubstituteData]
	internal void Execute_DoesNotHide(
		ICommandPaletteWindowViewModel viewModelMock,
		IVariantControl variantMock,
		IVariantViewModel variantViewModelMock
	)
	{
		// Given
		variantMock.ViewModel.Returns(variantViewModelMock);
		viewModelMock.ActiveVariant.Returns(variantMock);
		viewModelMock.ActivationConfig.Returns(Substitute.For<BaseVariantConfig>());

		// When
		ConfirmCommand command = new(viewModelMock);
		command.Execute(null);

		// Then
		viewModelMock.DidNotReceive().RequestHide();
	}

	[Theory, AutoSubstituteData]
	internal void Execute_Hides(
		ICommandPaletteWindowViewModel viewModelMock,
		IVariantControl variantMock,
		IVariantViewModel variantViewModelMock
	)
	{
		// Given
		variantMock.ViewModel.Returns(variantViewModelMock);
		viewModelMock.ActiveVariant.Returns(variantMock);
		viewModelMock.ActivationConfig.Returns(Substitute.For<BaseVariantConfig>());
		viewModelMock.IsConfigActive(Arg.Any<BaseVariantConfig>()).Returns(true);

		// When
		ConfirmCommand command = new(viewModelMock);
		command.Execute(null);

		// Then
		viewModelMock.Received(1).RequestHide();
	}
}
