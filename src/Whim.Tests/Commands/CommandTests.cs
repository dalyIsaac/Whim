using Xunit;

namespace Whim.Tests;

public class CommandTests
{
	[Fact]
	public void CanExecute_WithCondition_ReturnsCondition()
	{
		// Given
		var command = new Command("id", "title", () => { }, () => false);

		// When
		var result = command.CanExecute();

		// Then
		Assert.False(result);
	}

	[Fact]
	public void CanExecute_WithoutCondition_ReturnsTrue()
	{
		// Given
		var command = new Command("id", "title", () => { });

		// When
		var result = command.CanExecute();

		// Then
		Assert.True(result);
	}

	[Fact]
	public void TryExecute_WithCondition_ReturnsCondition()
	{
		// Given
		var command = new Command("id", "title", () => { }, () => false);

		// When
		var result = command.TryExecute();

		// Then
		Assert.False(result);
	}

	[Fact]
	public void TryExecute_WithoutCondition_ReturnsTrue()
	{
		// Given
		var command = new Command("id", "title", () => { });

		// When
		var result = command.TryExecute();

		// Then
		Assert.True(result);
	}
}
