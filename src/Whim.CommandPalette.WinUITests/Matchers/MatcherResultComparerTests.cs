using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Whim.CommandPalette.WinUITests;

public class MatcherResultComparerTests
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

	[TestMethod]
	public void ThrowWhenXIsNull()
	{
		// Given
		MatcherResult<CommandItem>? x = null;
		MatcherResult<CommandItem>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.ThrowsException<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[TestMethod]
	public void ThrowWhenYIsNull()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("x");
		MatcherResult<CommandItem>? y = null;

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.ThrowsException<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[TestMethod]
	public void ReturnZeroWhenXAndYAreEqual()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("a");
		MatcherResult<CommandItem>? y = CreateMatcherItem("a");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.AreEqual(0, comparer.Compare(x, y));
	}

	[TestMethod]
	public void ReturnMinusOneWhenXHasHigherScore()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("x", 1);
		MatcherResult<CommandItem>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.AreEqual(-1, comparer.Compare(x, y));
	}

	[TestMethod]
	public void ReturnOneWhenYHasHigherScore()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("x");
		MatcherResult<CommandItem>? y = CreateMatcherItem("y", 1);

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.AreEqual(1, comparer.Compare(x, y));
	}

	[TestMethod]
	public void ReturnMinusOneWhenEqualScoreAndXAlphabeticallyFirst()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("x");
		MatcherResult<CommandItem>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.AreEqual(-1, comparer.Compare(x, y));
	}

	[TestMethod]
	public void ReturnOneWhenEqualScoreAndYAlphabeticallyFirst()
	{
		// Given
		MatcherResult<CommandItem>? x = CreateMatcherItem("b");
		MatcherResult<CommandItem>? y = CreateMatcherItem("a");

		// When
		MatcherItemComparer<CommandItem> comparer = new();

		// Then
		Assert.AreEqual(1, comparer.Compare(x, y));
	}
}
