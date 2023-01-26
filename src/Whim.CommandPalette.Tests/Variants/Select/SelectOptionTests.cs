using Xunit;

namespace Whim.CommandPalette.Tests;

public class SelectOptionTests
{
	[Fact]
	public void IsSelected_INotifyPropertyChanged()
	{
		// Given
		SelectOption option =
			new()
			{
				Id = "id",
				Title = "title",
				IsEnabled = false,
				IsSelected = false
			};

		// When
		// Then
		Assert.PropertyChanged(option, nameof(SelectOption.IsSelected), () => option.IsSelected = true);
	}

	[Fact]
	public void IsEnabled_INotifyPropertyChanged()
	{
		// Given
		SelectOption option =
			new()
			{
				Id = "id",
				Title = "title",
				IsEnabled = false,
				IsSelected = false
			};

		// When
		// Then
		Assert.PropertyChanged(option, nameof(SelectOption.IsEnabled), () => option.IsEnabled = true);
	}
}
