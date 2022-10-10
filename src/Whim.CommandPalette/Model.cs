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
public record struct TextSegment(string Text, bool IsHighlighted)
{
	/// <summary>
	/// Converts the <see cref="TextSegment"/> to a <see cref="Run"/>.
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
public record PaletteRowText
{
	/// <summary>
	/// The segments of text.
	/// </summary>
	public IList<TextSegment> Segments { get; } = new List<TextSegment>();
}

/// <summary>
/// An item stored in the command palette, consisting of a match and associated highlighted title
/// text.
/// </summary>
/// <param name="CommandItem"></param>
/// <param name="Title"></param>
public record PaletteRowItem(CommandItem CommandItem, PaletteRowText Title);
