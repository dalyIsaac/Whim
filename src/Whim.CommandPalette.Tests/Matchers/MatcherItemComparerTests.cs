using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MatcherItemComparerTests
{
	private static MatcherItem<CommandItem> CreateMatcherItem(string title, uint score = 0)
	{
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Title).Returns(title);

		return new MatcherItem<CommandItem>()
		{
			Item = new MenuVariantRowModel(new CommandItem() { Command = command.Object }),
			TextSegments = Array.Empty<FilterTextMatch>(),
			Score = score
		};
	}

	[Fact]
	public void ThrowWhenXIsNull()
	{
		// Given
		MatcherItem<CommandItem>? x = null;
		MatcherItem<CommandItem>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Throws<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[Fact]
	public void ThrowWhenYIsNull()
	{
		// Given
		MatcherItem<CommandItem>? x = CreateMatcherItem("x");
		MatcherItem<CommandItem>? y = null;

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Throws<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnZeroWhenXAndYAreEqual()
	{
		// Given
		MatcherItem<CommandItem>? x = CreateMatcherItem("a");
		MatcherItem<CommandItem>? y = CreateMatcherItem("a");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(0, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnMinusOneWhenXHasHigherScore()
	{
		// Given
		MatcherItem<CommandItem>? x = CreateMatcherItem("x", 1);
		MatcherItem<CommandItem>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(-1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnOneWhenYHasHigherScore()
	{
		// Given
		MatcherItem<CommandItem>? x = CreateMatcherItem("x");
		MatcherItem<CommandItem>? y = CreateMatcherItem("y", 1);

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnMinusOneWhenEqualScoreAndXAlphabeticallyFirst()
	{
		// Given
		MatcherItem<CommandItem>? x = CreateMatcherItem("x");
		MatcherItem<CommandItem>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(-1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnOneWhenEqualScoreAndYAlphabeticallyFirst()
	{
		// Given
		MatcherItem<CommandItem>? x = CreateMatcherItem("b");
		MatcherItem<CommandItem>? y = CreateMatcherItem("a");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.Equal(1, comparer.Compare(x, y));
	}
}
