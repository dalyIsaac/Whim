using Xunit;

namespace Whim.CommandPalette.Tests;

public class SelectOptionTests
{
	[Fact]
	public void IsSelected_INotifyPropertyChanged()
	{
		// Given
		SelectOption option = new () {
			Id = "id",
			Title = "title",
			IsDisabled = false,
			IsSelected = false
		};

		// When
		// Then
		Assert.PropertyChanged(option, nameof(SelectOption.IsSelected), () => option.IsSelected = true);
	}

	[Fact]
	public void IsDisabled_INotifyPropertyChanged()
	{
		// Given
		SelectOption option = new () {
			Id = "id",
			Title = "title",
			IsDisabled = false,
			IsSelected = false
		};

		// When
		// Then
		Assert.PropertyChanged(option, nameof(SelectOption.IsDisabled), () => option.IsDisabled = true);
	}
}
