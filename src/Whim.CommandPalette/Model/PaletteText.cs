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
public record struct PaletteTextSegment(string Text, bool IsHighlighted)
{
	/// <summary>
	/// Converts the <see cref="PaletteTextSegment"/> to a <see cref="Run"/>.
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
public record PaletteText
{
	/// <summary>
	/// The segments of text.
	/// </summary>
	public IList<PaletteTextSegment> Segments { get; } = new List<PaletteTextSegment>();

	/// <summary>
	/// Converts a <see cref="string"/> to a <see cref="PaletteText"/>.
	/// </summary>
	/// <param name="text"></param>
	public static implicit operator PaletteText(string text)
	{
		return FromString(text);
	}

	/// <summary>
	/// Converts a <see cref="string"/> to a <see cref="PaletteText"/>.
	/// </summary>
	/// <param name="text"></param>
	public static PaletteText FromString(string text)
	{
		return new() { Segments = { new(text, false) } };
	}
}
