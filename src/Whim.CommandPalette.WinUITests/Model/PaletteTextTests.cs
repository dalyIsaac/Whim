using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace Whim.CommandPalette.WinUITests;

[TestClass]
public class PaletteTextSegmentTests
{
	[UITestMethod]
	public void PaletteTextSegment_ToRun()
	{
		// Arrange
		PaletteTextSegment segment = new("Test", true);

		// Act
		Run run = segment.ToRun();

		// Assert
		Assert.AreEqual("Test", run.Text);
		Assert.AreEqual(FontWeights.Bold, run.FontWeight);
	}

	[UITestMethod]
	public void PaletteTextSegment_ToRun_NoHighlight()
	{
		// Arrange
		PaletteTextSegment segment = new("Test", false);

		// Act
		Run run = segment.ToRun();

		// Assert
		Assert.AreEqual("Test", run.Text);
		Assert.AreEqual(FontWeights.Normal, run.FontWeight);
	}
}

[TestClass]
public class PaletteTextTests
{
	[TestMethod]
	public void PaletteText_FromString()
	{
		// Arrange
		string text = "From String";

		// Act
		PaletteText paletteText = new(text);

		// Assert
		Assert.AreEqual(1, paletteText.Segments.Count);
		Assert.AreEqual("From String", paletteText.Segments[0].Text);
		Assert.IsFalse(paletteText.Segments[0].IsHighlighted);
	}
}
