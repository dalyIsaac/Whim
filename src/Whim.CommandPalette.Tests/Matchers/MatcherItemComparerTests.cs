using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MatcherItemComparerTests
{
	private static MatcherResult<CommandItem> CreateMatcherItem(string title, uint score = 0)
	{
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Title).Returns(title);
		return new MatcherResult<CommandItem>(
			new MenuVariantRowModel(new CommandItem() { Command = command.Object }),
			Array.Empty<FilterTextMatch>(),
			score
		);
	}

	[Fact]
	public void ThrowWhenXIsNull()
	{
		// Given
		MatcherResult<CommandItem>? x = null;
		MatcherResult<CommandItem>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Throws<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[Fact]
	public void ThrowWhenYIsNull()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("x");
		MatcherResult<CommandItem>? y = null;

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Throws<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnZeroWhenXAndYAreEqual()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("a");
		MatcherResult<CommandItem>? y = CreateMatcherItem("a");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(0, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnMinusOneWhenXHasHigherScore()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("x", 1);
		MatcherResult<CommandItem>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(-1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnOneWhenYHasHigherScore()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("x");
		MatcherResult<CommandItem>? y = CreateMatcherItem("y", 1);

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnMinusOneWhenEqualScoreAndXAlphabeticallyFirst()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("x");
		MatcherResult<CommandItem>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(-1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnOneWhenEqualScoreAndYAlphabeticallyFirst()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("b");
		MatcherResult<CommandItem>? y = CreateMatcherItem("a");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(1, comparer.Compare(x, y));
	}
}
