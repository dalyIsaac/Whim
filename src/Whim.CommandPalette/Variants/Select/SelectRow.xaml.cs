using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class SelectRow : UserControl, ISelectRow
{
	public static double SelectRowHeight => 24;

	public IVariantItem<SelectOption> Item { get; private set; }

	public SelectRow(IVariantItem<SelectOption> item)
	{
		Item = item;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "PaletteRow");
	}

	public void Initialize()
	{
		SetTitle();
	}

	public void Update(IVariantItem<SelectOption> item)
	{
		Logger.Debug("Updating with a new item");
		Item = item;
		SetTitle();
	}

	/// <summary>
	/// Update the title based on the model's title segments.
	/// Efforts have been made to reduce the amount of time spent in this method.
	/// </summary>
	private void SetTitle()
	{
		Logger.Debug("Setting title");
		InlineCollection inlines = OptionTitle.Inlines;
		IList<PaletteTextSegment> segments = Item.FormattedTitle.Segments;

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
