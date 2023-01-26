using Microsoft.UI.Xaml.Documents;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// Extension methods for <see cref="IVariantRow{T}"/>.
/// </summary>
public static class IVariantRowExtensions
{
	/// <summary>
	/// Sets the title of the row.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="row"></param>
	/// <param name="inlines">The inlines to set the title to.</param>
	public static void SetTitle<T>(this IVariantRow<T> row, InlineCollection inlines)
	{
		Logger.Debug("Setting title");
		IList<PaletteTextSegment> segments = row.Item.FormattedTitle.Segments;

		int idx;
		for (idx = 0; idx < segments.Count; idx++)
		{
			PaletteTextSegment seg = segments[idx];
			Run run = seg.ToRun();

			if (idx < inlines.Count)
			{
				// Only update the run if it's different.
				if (run != inlines[idx])
				{
					inlines[idx] = run;
				}
			}
			else
			{
				// Add the run, because there's no space.
				inlines.Add(run);
			}
		}

		// Remove excess runs.
		int inlinesCount = inlines.Count;
		for (; idx < inlinesCount; idx++)
		{
			inlines.RemoveAt(inlines.Count - 1);
		}
	}
}
