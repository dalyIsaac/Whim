using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Text;

namespace Whim.CommandPalette;

public record struct HighlightedTextSegment(string Text, bool IsHighlighted)
{
	public Run ToRun()
	{
		string matchText = Text;
		FontWeight fontWeight = IsHighlighted ? FontWeights.Bold : FontWeights.Normal;
		return new() { Text = matchText, FontWeight = fontWeight };
	}
}

public record HighlightedText
{
	public IList<HighlightedTextSegment> Segments { get; } = new List<HighlightedTextSegment>();
}

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

public record PaletteItem(Match Match, HighlightedText Title);
