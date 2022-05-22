using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Windows.UI.Text;

namespace Whim.CommandPalette;

public sealed partial class PaletteItem : UserControl
{
	public CommandPaletteMatch Match { get; }

	public PaletteItem(CommandPaletteMatch match)
	{
		Match = match;
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "PaletteItems/PaletteItem");

		InlineCollection inlinesCollection = CommandTitle.Inlines;
		inlinesCollection.Clear();

		// Set up the title.
		foreach (HighlightedTextSegment segment in match.Title.Segments)
		{
			string matchText = segment.Text;
			FontWeight fontWeight = segment.IsHighlighted ? FontWeights.Bold : FontWeights.Normal;

			Run run = new() { Text = matchText, FontWeight = fontWeight };
			inlinesCollection.Add(run);
		}

		// Set up the keybinds.
		for (int i = 0; i < match.AllKeys.Count; i++)
		{
			string key = match.AllKeys[i];
			CommandKeybind.Items.Add(new PaletteKeybindItem(key));

			if (i != match.AllKeys.Count - 1)
			{
				CommandKeybind.Items.Add(new TextBlock() { Text = "+" });
			}
		}
	}
}
