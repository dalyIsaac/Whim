using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class PaletteMenuRow : UserControl, IPaletteRow
{
	public PaletteRowItem Model { get; private set; }

	public PaletteMenuRow(PaletteRowItem item)
	{
		Model = item;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "PaletteRow");
	}

	public void Initialize()
	{
		SetTitle();
		SetKeybinds();
	}

	public void Update(PaletteRowItem item)
	{
		Logger.Debug("Updating with a new item");
		Model = item;
		SetTitle();
		SetKeybinds();
	}

	/// <summary>
	/// Update the title based on the model's title segments.
	/// Efforts have been made to reduce the amount of time spent in this method.
	/// </summary>
	private void SetTitle()
	{
		Logger.Debug("Setting title");
		InlineCollection inlines = CommandTitle.Inlines;
		IList<TextSegment> segments = Model.Title.Segments;

		int idx;
		for (idx = 0; idx < segments.Count; idx++)
		{
			TextSegment seg = segments[idx];
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

		if (Model.CommandItem.Keybind is not null)
		{
			CommandKeybind.Text = Model.CommandItem.Keybind.ToString();
			CommandKeybindBorder.Visibility = Visibility.Visible;
		}
		else
		{
			CommandKeybindBorder.Visibility = Visibility.Collapsed;
		}
	}
}
