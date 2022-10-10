using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MatcherCommandItemComparerTests
{
	private static MatcherCommandItem CreateMatcherCommandItem(string title, uint score = 0)
	{
		Mock<ICommand> command = new();
		command.SetupGet(c => c.Title).Returns(title);

		return new MatcherCommandItem(new CommandItem(command.Object), Array.Empty<PaletteFilterTextMatch>(), score);
	}

	[Fact]
	public void ThrowWhenXIsNull()
	{
		// Given
		MatcherCommandItem? x = null;
		MatcherCommandItem? y = CreateMatcherCommandItem("y");

		// When
		MatcherCommandItemComparer comparer = new();

		// Then
		Assert.Throws<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[Fact]
	public void ThrowWhenYIsNull()
	{
		// Given
		MatcherCommandItem? x = CreateMatcherCommandItem("x");
		MatcherCommandItem? y = null;

		// When
		MatcherCommandItemComparer comparer = new();

		// Then
		Assert.Throws<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnZeroWhenXAndYAreEqual()
	{
		// Given
		MatcherCommandItem? x = CreateMatcherCommandItem("a");
		MatcherCommandItem? y = CreateMatcherCommandItem("a");

		// When
		MatcherCommandItemComparer comparer = new();

		// Then
		Assert.Equal(0, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnMinusOneWhenXHasHigherScore()
	{
		// Given
		MatcherCommandItem? x = CreateMatcherCommandItem("x", 1);
		MatcherCommandItem? y = CreateMatcherCommandItem("y");

		// When
		MatcherCommandItemComparer comparer = new();

		// Then
		Assert.Equal(-1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnOneWhenYHasHigherScore()
	{
		// Given
		MatcherCommandItem? x = CreateMatcherCommandItem("x");
		MatcherCommandItem? y = CreateMatcherCommandItem("y", 1);

		// When
		MatcherCommandItemComparer comparer = new();

		// Then
		Assert.Equal(1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnMinusOneWhenEqualScoreAndXAlphabeticallyFirst()
	{
		// Given
		MatcherCommandItem? x = CreateMatcherCommandItem("x");
		MatcherCommandItem? y = CreateMatcherCommandItem("y");

		// When
		MatcherCommandItemComparer comparer = new();

		// Then
		Assert.Equal(-1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnOneWhenEqualScoreAndYAlphabeticallyFirst()
	{
		// Given
		MatcherCommandItem? x = CreateMatcherCommandItem("b");
		MatcherCommandItem? y = CreateMatcherCommandItem("a");

		// When
		MatcherCommandItemComparer comparer = new();

		// Then
		Assert.Equal(1, comparer.Compare(x, y));
	}
}
