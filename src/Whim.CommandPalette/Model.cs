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
	/// <summary>
	/// Converts the <see cref="HighlightedTextSegment"/> to a <see cref="Run"/>.
	/// </summary>
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
	/// <summary>
	/// The segments of text.
	/// </summary>
	public IList<HighlightedTextSegment> Segments { get; } = new List<HighlightedTextSegment>();
}

/// <summary>
/// A command and associated keybind.
/// </summary>
public class Match
{
	/// <summary>
	/// The command to execute.
	/// </summary>
	public ICommand Command { get; }

	/// <summary>
	/// The keybind to execute the command.
	/// </summary>
	public string? Keys { get; }

	/// <summary>
	/// Creates a new match from the given command and keybind.
	/// </summary>
	/// <param name="command"></param>
	/// <param name="keybind"></param>
	public Match(ICommand command, IKeybind? keybind = null)
	{
		Command = command;
		Keys = keybind?.ToString();
	}

	/// <inheritdoc />
	public override int GetHashCode() => Command.GetHashCode();
}

/// <summary>
/// An item stored in the command palette, consisting of a match and associated highlighted title
/// text.
/// </summary>
/// <param name="Match"></param>
/// <param name="Title"></param>
public record PaletteItem(Match Match, HighlightedText Title);
