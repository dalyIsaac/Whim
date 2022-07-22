using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class PaletteRow : UserControl
{
	public PaletteItem Model { get; private set; }

	public PaletteRow(PaletteItem item)
	{
		Model = item;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "PaletteRow");
	}

	public void Initialize()
	{
		SetTitle();
		SetKeybinds();
	}

	public void Update(PaletteItem item)
	{
		Logger.Debug("Updating with a new item");
		Model = item;
		SetTitle();
	}

	/// <summary>
	/// Update the title based on the model's title segments.
	/// Efforts have been made to reduce the amount of time spent in this method.
	/// </summary>
	private void SetTitle()
	{
		Logger.Debug("Setting title");
		InlineCollection inlines = CommandTitle.Inlines;
		IList<HighlightedTextSegment> segments = Model.Title.Segments;

		int idx;
		for (idx = 0; idx < segments.Count; idx++)
		{
			HighlightedTextSegment seg = segments[idx];
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

	private void SetKeybinds()
	{
		Logger.Debug("Setting keybinds");

		if (Model.Match.Keys != null)
		{
			CommandKeybind.Text = Model.Match.Keys;
			CommandKeybindBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
		}
		else
		{
			CommandKeybindBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
		}
	}
}
