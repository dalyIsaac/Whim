using System.Collections.Generic;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
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
	public readonly Run ToRun()
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
	public IList<PaletteTextSegment> Segments { get; } = [];

	/// <summary>
	/// Creates an empty instance of the <see cref="PaletteText"/> class.
	/// </summary>
	public PaletteText() { }

	/// <summary>
	/// Creates a new instance of the <see cref="PaletteText"/> class from the specified text.
	/// </summary>
	/// <remarks>
	/// A single segment of text is created, with no highlighting.
	/// </remarks>
	/// <param name="text"></param>
	public PaletteText(string text)
	{
		Segments.Add(new(text, false));
	}
}
