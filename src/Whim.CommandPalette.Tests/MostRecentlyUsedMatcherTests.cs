using Xunit;

namespace Whim.CommandPalette.Tests;

public class MostRecentlyUsedMatcherTests
{
	private static void ActionSink() { }

	private static (CommandItem[], MatcherCommandItem[]) CreateMocks(string[] items)
	{
		MatcherCommandItem[] matcherItems = new MatcherCommandItem[items.Length];
		CommandItem[] commandItems = new CommandItem[items.Length];

		for (int i = 0; i < items.Length; i++)
		{
			commandItems[i] = new CommandItem() { Command = new Command(items[i], items[i], ActionSink), };
		}

		return (commandItems, matcherItems);
	}

	[Fact]
	public void GetFilteredItems()
	{
		// Given
		MostRecentlyUsedMatcher matcher = new();
		(CommandItem[] items, MatcherCommandItem[] matches) = CreateMocks(new string[] { "A", "B" });

		// When
		int matchCount = matcher.GetFilteredItems("A", items, matches);

		// Then
		Assert.Equal(1, matchCount);
		Assert.Equal("A", matches[0].Item.Command.Title);
		Assert.Equal((uint)0, matches[0].Score);
	}

	[Fact]
	public void GetMostRecentlyUsedItems()
	{
		// Given
		MostRecentlyUsedMatcher matcher = new();
		(CommandItem[] items, MatcherCommandItem[] matches) = CreateMocks(new string[] { "A", "B" });

		// When
		int matchCount = matcher.GetMostRecentlyUsedItems(items, matches);

		// Then
		Assert.Equal(2, matchCount);
		Assert.Equal("A", matches[0].Item.Command.Title);
		Assert.Equal((uint)0, matches[0].Score);
		Assert.Equal("B", matches[1].Item.Command.Title);
		Assert.Equal((uint)0, matches[1].Score);
	}

	[Fact]
	public void GetMostRecentlyUsedItems_WithLastExecutionTime()
	{
		// Given
		MostRecentlyUsedMatcher matcher = new();
		(CommandItem[] items, MatcherCommandItem[] matches) = CreateMocks(new string[] { "A", "B" });
		matcher.OnMatchExecuted(items[1]);

		// When
		int matchCount = matcher.GetMostRecentlyUsedItems(items, matches);

		// Then
		Assert.Equal(2, matchCount);
		Assert.Equal("A", matches[0].Item.Command.Title);
		Assert.Equal("B", matches[1].Item.Command.Title);
		Assert.True(matches[1].Score > matches[0].Score);
	}

	[Fact]
	public void Match_NoMatches_PopulatedQuery()
	{
		// Given
		MostRecentlyUsedMatcher matcher = new();
		(CommandItem[] items, MatcherCommandItem[] _) = CreateMocks(new string[] { "A", "B" });

		// When
		ICollection<PaletteRowItem> rowItems = matcher.Match("C", items);

		// Then
		Assert.Empty(rowItems);
	}

	[Fact]
	public void Match_NoMatches_EmptyQuery()
	{
		// Given
		MostRecentlyUsedMatcher matcher = new();
		(CommandItem[] items, MatcherCommandItem[] _) = CreateMocks(new string[] { "A", "B" });

		// When
		ICollection<PaletteRowItem> rowItems = matcher.Match("", items);

		// Then
		Assert.Equal(2, rowItems.Count);
		Assert.Equal("A", rowItems.ElementAt(0).CommandItem.Command.Title);
		Assert.Equal("B", rowItems.ElementAt(1).CommandItem.Command.Title);
	}
}
