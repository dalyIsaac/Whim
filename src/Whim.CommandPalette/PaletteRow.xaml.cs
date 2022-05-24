using Microsoft.UI.Text;
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
	}

	public void Initialize()
	{
		Logger.Verbose("a");
		SetTitle();
		Logger.Verbose("b");
		SetKeybinds();
		Logger.Verbose("c");
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
		InlineCollection inlines = CommandKeybind.Inlines;

		for (int i = 0; i < Model.Match.AllKeys.Count; i++)
		{
			string key = Model.Match.AllKeys[i];
			inlines.Add(new Run() { Text = key, FontWeight = FontWeights.Bold });

			if (i != Model.Match.AllKeys.Count - 1)
			{
				inlines.Add(new Run() { Text = " + " });
			}
		}
	}
}
