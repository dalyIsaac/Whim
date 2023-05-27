using Xunit;

namespace Whim.Bar.Tests;

public class TextWidgetViewModelTests
{
	[Fact]
	public void Value_PropertyChanged_Null()
	{
		// Given
		TextWidgetViewModel viewModel = new(null);

		// When
		// Then
		Assert.PropertyChanged(viewModel, nameof(viewModel.Value), () => viewModel.Value = "test");
	}

	[Fact]
	public void Value_PropertyChanged()
	{
		// Given
		TextWidgetViewModel viewModel = new("test");

		// When
		// Then
		Assert.PropertyChanged(viewModel, nameof(viewModel.Value), () => viewModel.Value = "test2");
	}
}
