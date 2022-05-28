using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using System.Collections.Generic;
using Windows.UI.Text;

namespace Whim.CommandPalette;

/// <summary>
/// A single segment of text, which can be highlighted.
/// </summary>
/// <param name="Text"></param>
/// <param name="IsHighlighted"></param>
public record struct HighlightedTextSegment(string Text, bool IsHighlighted)
{
	public Run ToRun()
	{
		string matchText = Text;
		FontWeight fontWeight = IsHighlighted ? FontWeights.Bold : FontWeights.Normal;
		return new() { Text = matchText, FontWeight = fontWeight };
	}
}

/// <summary>
/// The segments which make up the highlighted text.
/// </summary>
public record HighlightedText
{
	public IList<HighlightedTextSegment> Segments { get; } = new List<HighlightedTextSegment>();
}

/// <summary>
/// A command and associated keybind.
/// </summary>
public class Match
{
	public ICommand Command { get; }
	public string? Keys { get; }

	public Match(ICommand command, IKeybind? keybind = null)
	{
		Command = command;
		Keys = keybind?.ToString();
	}

	public override int GetHashCode() => Command.GetHashCode();
}

/// <summary>
/// An item stored in the command palette, consisting of a match and associated highlighted title
/// text.
/// </summary>
/// <param name="Match"></param>
/// <param name="Title"></param>
public record PaletteItem(Match Match, HighlightedText Title);
