using System.Collections.Generic;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Tests;

public class KeyModifiersTests
{
	[Theory]
	[InlineData(KeyModifiers.None, false, new string[] { })]
	[InlineData(KeyModifiers.LControl, false, new string[] { "LCtrl" })]
	[InlineData(KeyModifiers.RControl, false, new string[] { "RCtrl" })]
	[InlineData(KeyModifiers.LShift, false, new string[] { "LShift" })]
	[InlineData(KeyModifiers.RShift, false, new string[] { "RShift" })]
	[InlineData(KeyModifiers.LAlt, false, new string[] { "LAlt" })]
	[InlineData(KeyModifiers.RAlt, false, new string[] { "RAlt" })]
	[InlineData(KeyModifiers.LWin, false, new string[] { "LWin" })]
	[InlineData(KeyModifiers.RWin, false, new string[] { "RWin" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LShift, false, new string[] { "LCtrl", "LShift" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LAlt, false, new string[] { "LCtrl", "LAlt" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LWin, false, new string[] { "LWin", "LCtrl" })]
	[InlineData(
		KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		false,
		new string[] { "LCtrl", "LShift", "LAlt" }
	)]
	[InlineData(
		KeyModifiers.LWin
			| KeyModifiers.RWin
			| KeyModifiers.LControl
			| KeyModifiers.RControl
			| KeyModifiers.LAlt
			| KeyModifiers.RAlt
			| KeyModifiers.LShift
			| KeyModifiers.RShift,
		false,
		new string[] { "LWin", "RWin", "LCtrl", "RCtrl", "LShift", "RShift", "LAlt", "RAlt" }
	)]
	[InlineData(KeyModifiers.None, true, new string[] { })]
	[InlineData(KeyModifiers.LControl, true, new string[] { "Ctrl" })]
	[InlineData(KeyModifiers.RControl, true, new string[] { "Ctrl" })]
	[InlineData(KeyModifiers.LShift, true, new string[] { "Shift" })]
	[InlineData(KeyModifiers.RShift, true, new string[] { "Shift" })]
	[InlineData(KeyModifiers.LAlt, true, new string[] { "Alt" })]
	[InlineData(KeyModifiers.RAlt, true, new string[] { "Alt" })]
	[InlineData(KeyModifiers.LWin, true, new string[] { "Win" })]
	[InlineData(KeyModifiers.RWin, true, new string[] { "Win" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LShift, true, new string[] { "Ctrl", "Shift" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LAlt, true, new string[] { "Ctrl", "Alt" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LWin, true, new string[] { "Win", "Ctrl" })]
	[InlineData(
		KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		true,
		new string[] { "Ctrl", "Shift", "Alt" }
	)]
	[InlineData(
		KeyModifiers.LWin
			| KeyModifiers.RWin
			| KeyModifiers.LControl
			| KeyModifiers.RControl
			| KeyModifiers.LAlt
			| KeyModifiers.RAlt
			| KeyModifiers.LShift
			| KeyModifiers.RShift,
		true,
		new string[] { "Win", "Ctrl", "Shift", "Alt" }
	)]
	public void GetParts_ReturnsCorrectParts(KeyModifiers modifiers, bool unifyKeyModifiers, string[] expected)
	{
		IEnumerable<string> parts = modifiers.GetParts(unifyKeyModifiers);

		Assert.Equal(expected, parts);
	}

	[Theory]
	[InlineData(KeyModifiers.None, KeyModifiers.None)]
	[InlineData(KeyModifiers.LControl, KeyModifiers.LControl)]
	[InlineData(KeyModifiers.RControl, KeyModifiers.LControl)]
	[InlineData(KeyModifiers.LShift, KeyModifiers.LShift)]
	[InlineData(KeyModifiers.RShift, KeyModifiers.LShift)]
	[InlineData(KeyModifiers.LAlt, KeyModifiers.LAlt)]
	[InlineData(KeyModifiers.RAlt, KeyModifiers.LAlt)]
	[InlineData(KeyModifiers.LWin, KeyModifiers.LWin)]
	[InlineData(KeyModifiers.RWin, KeyModifiers.LWin)]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LShift, KeyModifiers.LControl | KeyModifiers.LShift)]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LAlt, KeyModifiers.LControl | KeyModifiers.LAlt)]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LWin, KeyModifiers.LControl | KeyModifiers.LWin)]
	[InlineData(
		KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt
	)]
	[InlineData(
		KeyModifiers.LWin
			| KeyModifiers.RWin
			| KeyModifiers.LControl
			| KeyModifiers.RControl
			| KeyModifiers.LAlt
			| KeyModifiers.RAlt
			| KeyModifiers.LShift
			| KeyModifiers.RShift,
		KeyModifiers.LWin | KeyModifiers.LControl | KeyModifiers.LAlt | KeyModifiers.LShift
	)]
	public void UnifyModifiers(KeyModifiers modifiers, KeyModifiers expectedModifiers)
	{
		// Given
		IKeybind keybind = new Keybind(modifiers, VIRTUAL_KEY.VK_A);

		// When
		IKeybind unifiedKeybind = keybind.UnifyModifiers();

		// Then
		Assert.Equal(expectedModifiers, unifiedKeybind.Modifiers);
		Assert.Equal(VIRTUAL_KEY.VK_A, unifiedKeybind.Key);
	}
}
