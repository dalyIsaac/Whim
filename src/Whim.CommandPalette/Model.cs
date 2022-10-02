using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using System;
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

internal record PalettePayload
{
	public CommandItem Item { get; }
	public PaletteFilterMatch[] Matches { get; }
	public uint LastUsedTime { get; }

	public PalettePayload(CommandItem item, PaletteFilterMatch[] matches, uint lastUsedTime)
	{
		Item = item;
		Matches = matches;
		LastUsedTime = lastUsedTime;
	}

	public PaletteRowItem ToRowItem()
	{
		HighlightedText title = new();
		ReadOnlySpan<char> rawTitle = Item.Command.Title.AsSpan();

		int start = 0;
		foreach (PaletteFilterMatch match in Matches)
		{
			if (start < match.Start)
			{
				title.Segments.Add(new HighlightedTextSegment(rawTitle[start..match.Start].ToString(), false));
				start = match.Start;
			}

			title.Segments.Add(new HighlightedTextSegment(rawTitle[match.Start..match.End].ToString(), true));
			start = match.End;
		}

		if (start < rawTitle.Length)
		{
			title.Segments.Add(new HighlightedTextSegment(rawTitle[start..].ToString(), false));
		}

		return new PaletteRowItem(Item, title);
	}
}

/// <summary>
/// Defines a method to compare <see cref="PalettePayload"/>.
/// </summary>
internal class PalettePayloadComparer : IComparer<PalettePayload>
{
	/// <inheritdoc />
	public int Compare(PalettePayload? x, PalettePayload? y)
	{
		// We throw here because it should never happen.
		if (x is null)
		{
			throw new ArgumentNullException(nameof(x));
		}
		else if (y is null)
		{
			throw new ArgumentNullException(nameof(y));
		}

		// Sort by the last used time.
		int lastUsedTimeComparison = y.LastUsedTime.CompareTo(x.LastUsedTime);
		if (lastUsedTimeComparison != 0)
		{
			return lastUsedTimeComparison;
		}

		// Sort by alphabetical order.
		return string.Compare(x.Item.Command.Title, y.Item.Command.Title, System.StringComparison.Ordinal);
	}
}

/// <summary>
/// An item stored in the command palette, consisting of a match and associated highlighted title
/// text.
/// </summary>
/// <param name="CommandItem"></param>
/// <param name="Title"></param>
public record PaletteRowItem(CommandItem CommandItem, HighlightedText Title);

/// <summary>
/// Callback for when the user has pressed enter key in the command palette, and is in free text mode.
/// </summary>
/// <param name="text"></param>
public delegate void CommandPaletteFreeTextCallback(string text);

/// <summary>
/// Config for activating the command palette.
/// </summary>
public class CommandPaletteActivationConfig
{
	/// <summary>
	/// The matcher to use to filter the results.
	/// </summary>
	public ICommandPaletteMatcher Matcher { get; set; }

	/// <summary>
	/// Text hint to show in the command palette.
	/// </summary>
	public string? Hint { get; set; }

	/// <summary>
	/// The text to pre-fill the command palette with.
	/// </summary>
	public string? InitialText { get; set; }

	/// <summary>
	/// Creates a new <see cref="CommandPaletteActivationConfig"/>.
	/// </summary>
	/// <param name="matcher"></param>
	/// <param name="hint"></param>
	/// <param name="initialText"></param>
	public CommandPaletteActivationConfig(ICommandPaletteMatcher? matcher = null, string? hint = null, string? initialText = null)
	{
		Matcher = matcher ?? new MostRecentlyUsedMatcher();
		Hint = hint;
		InitialText = initialText;
	}
}
