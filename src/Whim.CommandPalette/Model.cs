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

/// <summary>
/// A command and associated keybind.
/// </summary>
public class PaletteItem
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
	public PaletteItem(ICommand command, IKeybind? keybind = null)
	{
		Command = command;
		Keys = keybind?.ToString();
	}

	/// <inheritdoc />
	public override int GetHashCode() => Command.GetHashCode();
}

public record PaletteItemSortPayload
{
	public PaletteItem Item { get; }
	public PaletteFilterMatch[] Matches { get; }
	public uint LastUsedTime { get; }

	public PaletteItemSortPayload(PaletteItem item, PaletteFilterMatch[] matches, uint lastUsedTime)
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


public class PaletteItemSortPayloadSorter : IComparer<PaletteItemSortPayload>
{
	public int Compare(PaletteItemSortPayload x, PaletteItemSortPayload y)
	{
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
/// <param name="PaletteItem"></param>
/// <param name="Title"></param>
public record PaletteRowItem(PaletteItem PaletteItem, HighlightedText Title);
