using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System.Collections.Generic;

namespace Whim.CommandPalette;

public sealed partial class PaletteRow : UserControl
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
				inlines.Add(run);
			}
		}

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
		}
	}
}
