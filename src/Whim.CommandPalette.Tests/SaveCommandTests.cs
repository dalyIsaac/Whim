using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class SaveCommandTests
{
	[Fact]
	public void CanExecute_WhenActiveVariantIsNull_ReturnsFalse()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> viewModelMock = new();
		viewModelMock.Setup(x => x.ActiveVariant).Returns((IVariantControl?)null);

		// When
		SaveCommand command = new(viewModelMock.Object);

		// Then
		Assert.False(command.CanExecute(null));
	}

	[Fact]
	public void CanExecute_WhenActiveVariantIsNotNull_ReturnsTrue()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> viewModelMock = new();
		viewModelMock.Setup(x => x.ActiveVariant).Returns(new Mock<IVariantControl>().Object);

		// When
		SaveCommand command = new(viewModelMock.Object);

		// Then
		Assert.True(command.CanExecute(null));
	}

	[Fact]
	public void Execute_WhenActiveVariantIsNull_ReturnsEarly()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> viewModelMock = new();
		viewModelMock.Setup(x => x.ActiveVariant).Returns((IVariantControl?)null);

		// When
		SaveCommand command = new(viewModelMock.Object);
		command.Execute(null);

		// Then
		viewModelMock.Verify(x => x.RequestHide(), Times.Never);
	}

	[Fact]
	public void Execute_DoesNotHide()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> viewModelMock = new();
		Mock<IVariantControl> variantMock = new();
		Mock<IVariantViewModel> variantViewModelMock = new();

		variantMock.Setup(x => x.ViewModel).Returns(variantViewModelMock.Object);
		viewModelMock.Setup(x => x.ActiveVariant).Returns(variantMock.Object);
		viewModelMock.Setup(x => x.ActivationConfig).Returns(new Mock<BaseVariantConfig>().Object);

		// When
		SaveCommand command = new(viewModelMock.Object);
		command.Execute(null);

		// Then
		viewModelMock.Verify(x => x.RequestHide(), Times.Never);
	}

	[Fact]
	public void Execute_Hides()
	{
		// Given
		Mock<ICommandPaletteWindowViewModel> viewModelMock = new();
		Mock<IVariantControl> variantMock = new();
		Mock<IVariantViewModel> variantViewModelMock = new();

		variantMock.Setup(x => x.ViewModel).Returns(variantViewModelMock.Object);
		viewModelMock.Setup(x => x.ActiveVariant).Returns(variantMock.Object);
		viewModelMock.Setup(x => x.ActivationConfig).Returns(new Mock<BaseVariantConfig>().Object);
		viewModelMock.Setup(x => x.IsConfigActive(It.IsAny<BaseVariantConfig>())).Returns(true);

		// When
		SaveCommand command = new(viewModelMock.Object);
		command.Execute(null);

		// Then
		viewModelMock.Verify(x => x.RequestHide(), Times.Once);
	}
}
