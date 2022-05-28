using System.Collections.Generic;
using Xunit;

namespace Whim.Tests;

public class KeyModifiersTests
{
	[Theory]
	[InlineData(KeyModifiers.None, new string[] { })]
	[InlineData(KeyModifiers.LControl, new string[] { "LCtrl" })]
	[InlineData(KeyModifiers.RControl, new string[] { "RCtrl" })]
	[InlineData(KeyModifiers.LShift, new string[] { "LShift" })]
	[InlineData(KeyModifiers.RShift, new string[] { "RShift" })]
	[InlineData(KeyModifiers.LAlt, new string[] { "LAlt" })]
	[InlineData(KeyModifiers.RAlt, new string[] { "RAlt" })]
	[InlineData(KeyModifiers.LWin, new string[] { "LWin" })]
	[InlineData(KeyModifiers.RWin, new string[] { "RWin" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LShift, new string[] { "LCtrl", "LShift" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LAlt, new string[] { "LCtrl", "LAlt" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LWin, new string[] { "LWin", "LCtrl" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt, new string[] { "LCtrl", "LShift", "LAlt" })]
	[InlineData(
		KeyModifiers.LWin |
		KeyModifiers.RWin |
		KeyModifiers.LControl |
		KeyModifiers.RControl |
		KeyModifiers.LAlt |
		KeyModifiers.RAlt |
		KeyModifiers.LShift |
		KeyModifiers.RShift,
		new string[] { "LWin", "RWin", "LCtrl", "RCtrl", "LShift", "RShift", "LAlt", "RAlt" }
	)]
	public void GetParts_ReturnsCorrectParts(KeyModifiers modifiers, string[] expected)
	{
		IEnumerable<string> parts = modifiers.GetParts();

		Assert.Equal(expected, parts);
	}
}
