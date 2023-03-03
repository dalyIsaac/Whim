using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim.CommandPalette.Tests;

[TestClass]
public class MostRecentlyUsedMatcherTests
{
	private static void ActionSink() { }

	private static (MenuVariantRowModel[], MatcherResult<CommandItem>[]) CreateMocks(string[] items)
	{
		MenuVariantRowModel[] menuItems = new MenuVariantRowModel[items.Length];
		MatcherResult<CommandItem>[] matcherItems = new MatcherResult<CommandItem>[items.Length];

		for (int i = 0; i < items.Length; i++)
		{
			CommandItem commandItem = new() { Command = new Command(items[i], items[i], ActionSink), };
			MenuVariantRowModel menuItem = new(commandItem);

			menuItems[i] = menuItem;
			matcherItems[i] = new(menuItem, Array.Empty<FilterTextMatch>(), 0);
		}

		return (menuItems, matcherItems);
	}

	[TestMethod]
	public void GetFilteredItems()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantRowModel[] items, MatcherResult<CommandItem>[] matches) = CreateMocks(new string[] { "A", "B" });

		// When
		int matchCount = matcher.GetFilteredItems("A", items, matches);

		// Then
		Assert.AreEqual(1, matchCount);
		Assert.AreEqual("A", matches[0].Model.Data.Command.Title);
		Assert.AreEqual((uint)0, matches[0].Score);
	}

	[TestMethod]
	public void GetMostRecentlyUsedItems()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantRowModel[] items, MatcherResult<CommandItem>[] matches) = CreateMocks(new string[] { "A", "B" });

		// When
		int matchCount = matcher.GetMostRecentlyUsedItems(items, matches);

		// Then
		Assert.AreEqual(2, matchCount);
		Assert.AreEqual("A", matches[0].Model.Data.Command.Title);
		Assert.AreEqual((uint)0, matches[0].Score);
		Assert.AreEqual("B", matches[1].Model.Data.Command.Title);
		Assert.AreEqual((uint)0, matches[1].Score);
	}

	[TestMethod]
	public void GetMostRecentlyUsedItems_WithLastExecutionTime()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantRowModel[] items, MatcherResult<CommandItem>[] matches) = CreateMocks(new string[] { "A", "B" });
		matcher.OnMatchExecuted(items[1]);

		// When
		int matchCount = matcher.GetMostRecentlyUsedItems(items, matches);

		// Then
		Assert.AreEqual(2, matchCount);
		Assert.AreEqual("A", matches[0].Model.Data.Command.Title);
		Assert.AreEqual("B", matches[1].Model.Data.Command.Title);
		Assert.IsTrue(matches[1].Score > matches[0].Score);
	}

	[TestMethod]
	public void Match_NoMatches_PopulatedQuery()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantRowModel[] items, MatcherResult<CommandItem>[] _) = CreateMocks(new string[] { "A", "B" });

		// When
		IEnumerable<MatcherResult<CommandItem>> rowItems = matcher.Match("C", items);

		// Then
		rowItems.Should().BeEmpty();
	}

	[TestMethod]
	public void Match_NoMatches_EmptyQuery()
	{
		// Given
		MostRecentlyUsedMatcher<CommandItem> matcher = new();
		(MenuVariantRowModel[] items, MatcherResult<CommandItem>[] _) = CreateMocks(new string[] { "A", "B" });

		// When
		MatcherResult<CommandItem>[] rowItems = matcher.Match("", items).ToArray();

		// Then
		Assert.AreEqual(2, rowItems.Length);
		Assert.AreEqual("A", rowItems.ElementAt(0).Model.Data.Command.Title);
		Assert.AreEqual("B", rowItems.ElementAt(1).Model.Data.Command.Title);
	}
}
