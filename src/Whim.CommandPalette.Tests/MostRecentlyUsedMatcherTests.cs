using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MostRecentlyUsedMatcherTests
{
	private readonly List<Match> _items = new();

	private readonly MostRecentlyUsedMatcher _matcher = new();

	private void CreateMatch(string commandName)
	{
		Mock<ICommand> commandMock = new();
		commandMock.Setup(c => c.Identifier).Returns(commandName);
		commandMock.Setup(c => c.Title).Returns(commandName);

		Match match = new(commandMock.Object, new Mock<IKeybind>().Object);
		_items.Add(match);

		_matcher.OnMatchExecuted(match);
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

	public MostRecentlyUsedMatcherTests()
	{
		CreateMatch("foo");
		CreateMatch("bar");
		CreateMatch("baz");
		CreateMatch("qux");
		CreateMatch("quux");
		CreateMatch("corge");
		CreateMatch("grault");
		CreateMatch("garply");
		CreateMatch("waldo");
		CreateMatch("uxui");
	}

	[Fact]
	public void Match_Returns_Most_Often_Used_Items()
	{
		IEnumerable<PaletteItem> matches = _matcher.Match("", _items);

		List<(string MatchCommand, IList<HighlightedTextSegment> Title)> expectedItems = new()
		{
			new("uxui", CreateHighlightedText(new HighlightedTextSegment("uxui", false))),
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
			("uxui", CreateHighlightedText(new HighlightedTextSegment("ux", true), new HighlightedTextSegment("ui", false))),
			("quux", CreateHighlightedText(new HighlightedTextSegment("qu", false), new HighlightedTextSegment("ux", true))),
			("qux", CreateHighlightedText(new HighlightedTextSegment("q", false), new HighlightedTextSegment("ux", true))),
		};

		matches.Select(m => (m.Match.Command.Identifier, m.Title.Segments)).Should().BeEquivalentTo(expectedItems);
	}
}
