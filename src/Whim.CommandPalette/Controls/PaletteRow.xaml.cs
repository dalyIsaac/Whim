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
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Controls/PaletteRow");

		SetTitle();
		SetKeybinds();
	}

	public void Update(PaletteItem item)
	{
		Model = item;
		SetTitle();
	}

	private void SetTitle()
	{
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
					inlines[idx] = seg.ToRun();
				}
			}
			else
			{
				inlines.Add(run);
			}
		}

		for (; idx < segments.Count; idx++)
		{
			inlines.RemoveAt(inlines.Count - 1);
		}
	}

	private void SetKeybinds()
	{
		for (int i = 0; i < Model.Match.AllKeys.Count; i++)
		{
			string key = Model.Match.AllKeys[i];
			CommandKeybind.Items.Add(new PaletteKeybindItem(key));

			if (i != Model.Match.AllKeys.Count - 1)
			{
				CommandKeybind.Items.Add(new TextBlock() { Text = "+" });
			}
		}
	}
}
