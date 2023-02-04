using Xunit;

namespace Whim.CommandPalette.Tests;

public class MostRecentlyUsedMatcherTests
{
	private static void ActionSink() { }

	private static (MenuVariantItem[], MatcherItem<CommandItem>[]) CreateMocks(string[] items)
	{
		MenuVariantItem[] menuItems = new MenuVariantItem[items.Length];
		MatcherItem<CommandItem>[] matcherItems = new MatcherItem<CommandItem>[items.Length];

		for (int i = 0; i < items.Length; i++)
		{
			CommandItem commandItem = new() { Command = new Command(items[i], items[i], ActionSink), };
			MenuVariantItem menuItem = new(commandItem);

			menuItems[i] = menuItem;
			matcherItems[i] = new MatcherItem<CommandItem>()
			{
				Item = menuItem,
				Score = 0,
				TextSegments = Array.Empty<FilterTextMatch>()
			};
		}

		return (menuItems, matcherItems);
	}

	[Fact]
	public void GetFilteredItems()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantItem[] items, MatcherItem<CommandItem>[] matches) = CreateMocks(new string[] { "A", "B" });

		// When
		int matchCount = matcher.GetFilteredItems("A", items, matches);

		// Then
		Assert.Equal(1, matchCount);
		Assert.Equal("A", matches[0].Item.Data.Command.Title);
		Assert.Equal((uint)0, matches[0].Score);
	}

	[Fact]
	public void GetMostRecentlyUsedItems()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantItem[] items, MatcherItem<CommandItem>[] matches) = CreateMocks(new string[] { "A", "B" });

		// When
		int matchCount = matcher.GetMostRecentlyUsedItems(items, matches);

		// Then
		Assert.Equal(2, matchCount);
		Assert.Equal("A", matches[0].Item.Data.Command.Title);
		Assert.Equal((uint)0, matches[0].Score);
		Assert.Equal("B", matches[1].Item.Data.Command.Title);
		Assert.Equal((uint)0, matches[1].Score);
	}

	[Fact]
	public void GetMostRecentlyUsedItems_WithLastExecutionTime()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantItem[] items, MatcherItem<CommandItem>[] matches) = CreateMocks(new string[] { "A", "B" });
		matcher.OnMatchExecuted(items[1]);

		// When
		int matchCount = matcher.GetMostRecentlyUsedItems(items, matches);

		// Then
		Assert.Equal(2, matchCount);
		Assert.Equal("A", matches[0].Item.Data.Command.Title);
		Assert.Equal("B", matches[1].Item.Data.Command.Title);
		Assert.True(matches[1].Score > matches[0].Score);
	}

	[Fact]
	public void Match_NoMatches_PopulatedQuery()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantItem[] items, MatcherItem<CommandItem>[] _) = CreateMocks(new string[] { "A", "B" });

		// When
		IEnumerable<IVariantRowModel<CommandItem>> rowItems = matcher.Match("C", items);

		// Then
		Assert.Empty(rowItems);
	}

	[Fact]
	public void Match_NoMatches_EmptyQuery()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantItem[] items, MatcherItem<CommandItem>[] _) = CreateMocks(new string[] { "A", "B" });

		// When
		IVariantRowModel<CommandItem>[] rowItems = matcher.Match("", items).ToArray();

		// Then
		Assert.Equal(2, rowItems.Length);
		Assert.Equal("A", rowItems.ElementAt(0).Data.Command.Title);
		Assert.Equal("B", rowItems.ElementAt(1).Data.Command.Title);
	}
}
