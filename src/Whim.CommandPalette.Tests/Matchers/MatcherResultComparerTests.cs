using NSubstitute;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MatcherResultComparerTests
{
	private static MatcherResult<MenuVariantRowModelData> CreateMatcherItem(string title, uint score = 0)
	{
		ICommand command = Substitute.For<ICommand>();
		command.Title.Returns(title);
		return new MatcherResult<MenuVariantRowModelData>(
			new MenuVariantRowModel(command, null),
			Array.Empty<FilterTextMatch>(),
			score
		);
	}

	[Fact]
	public void ThrowWhenXIsNull()
	{
		// Given
		MatcherResult<MenuVariantRowModelData>? x = null;
		MatcherResult<MenuVariantRowModelData>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<MenuVariantRowModelData> comparer = new();

		// Then
		Assert.Throws<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[Fact]
	public void ThrowWhenYIsNull()
	{
		// Given
		MatcherResult<MenuVariantRowModelData>? x = CreateMatcherItem("x");
		MatcherResult<MenuVariantRowModelData>? y = null;

		// When
		MatcherItemComparer<MenuVariantRowModelData> comparer = new();

		// Then
		Assert.Throws<ArgumentNullException>(() => comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnZeroWhenXAndYAreEqual()
	{
		// Given
		MatcherResult<MenuVariantRowModelData>? x = CreateMatcherItem("a");
		MatcherResult<MenuVariantRowModelData>? y = CreateMatcherItem("a");

		// When
		MatcherItemComparer<MenuVariantRowModelData> comparer = new();

		// Then
		Assert.Equal(0, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnMinusOneWhenXHasHigherScore()
	{
		// Given
		MatcherResult<MenuVariantRowModelData>? x = CreateMatcherItem("x", 1);
		MatcherResult<MenuVariantRowModelData>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<MenuVariantRowModelData> comparer = new();

		// Then
		Assert.Equal(-1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnOneWhenYHasHigherScore()
	{
		// Given
		MatcherResult<MenuVariantRowModelData>? x = CreateMatcherItem("x");
		MatcherResult<MenuVariantRowModelData>? y = CreateMatcherItem("y", 1);

		// When
		MatcherItemComparer<MenuVariantRowModelData> comparer = new();

		// Then
		Assert.Equal(1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnMinusOneWhenEqualScoreAndXAlphabeticallyFirst()
	{
		// Given
		MatcherResult<MenuVariantRowModelData>? x = CreateMatcherItem("x");
		MatcherResult<MenuVariantRowModelData>? y = CreateMatcherItem("y");

		// When
		MatcherItemComparer<MenuVariantRowModelData> comparer = new();

		// Then
		Assert.Equal(-1, comparer.Compare(x, y));
	}

	[Fact]
	public void ReturnOneWhenEqualScoreAndYAlphabeticallyFirst()
	{
		// Given
		MatcherResult<MenuVariantRowModelData>? x = CreateMatcherItem("b");
		MatcherResult<MenuVariantRowModelData>? y = CreateMatcherItem("a");

		// When
		MatcherItemComparer<MenuVariantRowModelData> comparer = new();

		// Then
		Assert.Equal(1, comparer.Compare(x, y));
	}
}
