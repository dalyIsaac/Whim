using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MostOftenUsedMatcherTests
{
	private readonly List<Match> _items = new();
	private readonly Dictionary<string, uint> _commandExecutionCounts = new();

	private readonly MostOftenUsedMatcher _matcher = new();

	private void CreateMatch(string commandName, uint count)
	{
		Mock<ICommand> commandMock = new();
		commandMock.Setup(c => c.Identifier).Returns(commandName);
		commandMock.Setup(c => c.Title).Returns(commandName);

		Match match = new(commandMock.Object, new Mock<IKeybind>().Object);
		_items.Add(match);

		for (int i = 0; i < count; i++)
		{
			_matcher.OnMatchExecuted(match);
		}
	}

	private static IList<HighlightedTextSegment> CreateHighlightedText(params HighlightedTextSegment[] segments)
	{
		List<HighlightedTextSegment> results = new();

		foreach (HighlightedTextSegment segment in segments)
		{
			results.Add(segment);
		}

		return results;
	}

	public MostOftenUsedMatcherTests()
	{
		CreateMatch("foo", 1);
		CreateMatch("bar", 2);
		CreateMatch("baz", 3);
		CreateMatch("qux", 4);
		CreateMatch("quux", 5);
		CreateMatch("corge", 6);
		CreateMatch("grault", 7);
		CreateMatch("garply", 8);
		CreateMatch("waldo", 9);
	}

	[Fact]
	public void Match_Returns_Most_Often_Used_Items()
	{
		IEnumerable<PaletteItem> matches = _matcher.Match("", _items);

		List<(string MatchCommand, IList<HighlightedTextSegment> Title)> expectedItems = new()
		{
			new("waldo", CreateHighlightedText(new HighlightedTextSegment("waldo", false))),
			new("garply", CreateHighlightedText(new HighlightedTextSegment("garply", false))),
			new("grault", CreateHighlightedText(new HighlightedTextSegment("grault", false))),
			new("corge", CreateHighlightedText(new HighlightedTextSegment("corge", false))),
			new("quux", CreateHighlightedText(new HighlightedTextSegment("quux", false))),
			new("qux", CreateHighlightedText(new HighlightedTextSegment("qux", false))),
			new("baz", CreateHighlightedText(new HighlightedTextSegment("baz", false))),
			new("bar", CreateHighlightedText(new HighlightedTextSegment("bar", false))),
			new("foo", CreateHighlightedText(new HighlightedTextSegment("foo", false))),
		};

		matches.Select(m => (m.Match.Command.Identifier, m.Title.Segments)).Should().BeEquivalentTo(expectedItems);
	}

	[Fact]
	public void Match_Returns_Most_Often_Used_Items_With_Highlighted_Text()
	{
		IEnumerable<PaletteItem> matches = _matcher.Match("ux", _items);

		List<(string MatchCommand, IList<HighlightedTextSegment> Title)> expectedItems = new()
		{
			("quux", CreateHighlightedText(new HighlightedTextSegment("qu", false), new HighlightedTextSegment("ux", true))),
			("qux", CreateHighlightedText(new HighlightedTextSegment("q", false), new HighlightedTextSegment("ux", true))),
		};

		matches.Select(m => (m.Match.Command.Identifier, m.Title.Segments)).Should().BeEquivalentTo(expectedItems);
	}
}
