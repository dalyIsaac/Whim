using System.Collections.Generic;

namespace Whim.CommandPalette;

public record struct HighlightedTextSegment(string Text, bool IsHighlighted);

public record HighlightedText
{
	public ICollection<HighlightedTextSegment> Segments { get; } = new List<HighlightedTextSegment>();
}
