using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// A <see cref="IVariantRowModel{T}"/> model, the text matches from the query, and the score of the
/// result for the <see cref="IMatcher{T}"/>.
/// </summary>
public record MatcherResult<T>
{
	private readonly FilterTextMatch[] _textSegments;

	/// <summary>
	/// The associated model of the result.
	/// </summary>
	public IVariantRowModel<T> Model { get; }

	/// <summary>
	/// The score of the result.
	/// </summary>
	public long Score { get; }

	/// <summary>
	/// Creates a new <see cref="MatcherResult{T}"/>.
	/// </summary>
	/// <param name="model">The associated model of the result.</param>
	/// <param name="textSegments">The text matches from the query.</param>
	/// <param name="score">The score of the result.</param>
	public MatcherResult(IVariantRowModel<T> model, FilterTextMatch[] textSegments, long score)
	{
		Model = model;
		_textSegments = textSegments;
		Score = score;
	}

	/// <summary>
	/// The title of the item, with the matched text segments highlighted.
	/// </summary>
	/// <remarks>
	/// This will re-evaluate each time it is accessed.
	/// </remarks>
	public PaletteText FormattedTitle
	{
		get
		{
			PaletteText formattedTitle = new();
			ReadOnlySpan<char> rawTitle = Model.Title.AsSpan();

			int start = 0;
			foreach (FilterTextMatch match in _textSegments)
			{
				if (start < match.Start)
				{
					formattedTitle.Segments.Add(new PaletteTextSegment(rawTitle[start..match.Start].ToString(), false));
					start = match.Start;
				}

				formattedTitle.Segments.Add(new PaletteTextSegment(rawTitle[match.Start..match.End].ToString(), true));
				start = match.End;
			}

			if (start < rawTitle.Length)
			{
				formattedTitle.Segments.Add(new PaletteTextSegment(rawTitle[start..].ToString(), false));
			}

			return formattedTitle;
		}
	}
}

/// <summary>
/// Defines a method to compare <see cref="MatcherResult{T}"/>.
/// A higher score will be sorted first. For equal scores, the title will be sorted normally.
/// </summary>
internal class MatcherItemComparer<T> : IComparer<MatcherResult<T>>
{
	/// <inheritdoc />
	public int Compare(MatcherResult<T>? x, MatcherResult<T>? y)
	{
		// We throw here because it should never happen.
		ArgumentNullException.ThrowIfNull(x);
		ArgumentNullException.ThrowIfNull(y);

		// Sort by the last used time.
		if (x.Score > y.Score)
		{
			return -1;
		}
		else if (x.Score < y.Score)
		{
			return 1;
		}

		// Sort by alphabetical order.
		return string.Compare(x.Model.Title, y.Model.Title, StringComparison.Ordinal);
	}
}
