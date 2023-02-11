using Xunit;

namespace Whim.CommandPalette.Tests;

public class PaletteTextTests
{
	[Fact]
	public void ImplicitOperator()
	{
		// Given
		string text = "Implicit";

		// When
		PaletteText paletteText = new(text);

		// Then
		Assert.Single(paletteText.Segments);
		Assert.Equal("Implicit", paletteText.Segments[0].Text);
		Assert.False(paletteText.Segments[0].IsHighlighted);
	}

	[Fact]
	public void FromString()
	{
		// Given
		string text = "From String";

		// When
		PaletteText paletteText = new(text);

		// Then
		Assert.Single(paletteText.Segments);
		Assert.Equal("From String", paletteText.Segments[0].Text);
		Assert.False(paletteText.Segments[0].IsHighlighted);
	}
}
