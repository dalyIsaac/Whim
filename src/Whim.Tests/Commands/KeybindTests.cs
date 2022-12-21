using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Tests;

public class KeybindTests
{
	[Theory]
	[InlineData(KeyModifiers.None, VIRTUAL_KEY.VK_A, "A")]
	[InlineData(KeyModifiers.LShift, VIRTUAL_KEY.VK_A, "LShift + A")]
	[InlineData(KeyModifiers.LControl, VIRTUAL_KEY.VK_A, "LCtrl + A")]
	[InlineData(KeyModifiers.LAlt, VIRTUAL_KEY.VK_A, "LAlt + A")]
	[InlineData(KeyModifiers.LShift | KeyModifiers.LControl, VIRTUAL_KEY.VK_A, "LCtrl + LShift + A")]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LAlt, VIRTUAL_KEY.VK_A, "LCtrl + LAlt + A")]
	[InlineData(KeyModifiers.LShift | KeyModifiers.LAlt, VIRTUAL_KEY.VK_A, "LShift + LAlt + A")]
	[InlineData(
		KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		VIRTUAL_KEY.VK_A,
		"LCtrl + LShift + LAlt + A"
	)]
	[InlineData(
		KeyModifiers.LWin | KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		VIRTUAL_KEY.VK_A,
		"LWin + LCtrl + LShift + LAlt + A"
	)]
	public void Keybind_ToString_ReturnsCorrectString(KeyModifiers modifiers, VIRTUAL_KEY key, string expected)
	{
		Keybind keybind = new(modifiers, key);
		Assert.Equal(expected, keybind.ToString());
	}

	[Theory]
	[InlineData(KeyModifiers.None, VIRTUAL_KEY.VK_A, new string[] { "A" })]
	[InlineData(KeyModifiers.LShift, VIRTUAL_KEY.VK_A, new string[] { "LShift", "A" })]
	[InlineData(KeyModifiers.LControl, VIRTUAL_KEY.VK_A, new string[] { "LCtrl", "A" })]
	[InlineData(KeyModifiers.LAlt, VIRTUAL_KEY.VK_A, new string[] { "LAlt", "A" })]
	[InlineData(KeyModifiers.LShift | KeyModifiers.LControl, VIRTUAL_KEY.VK_A, new string[] { "LCtrl", "LShift", "A" })]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LAlt, VIRTUAL_KEY.VK_A, new string[] { "LCtrl", "LAlt", "A" })]
	[InlineData(KeyModifiers.LShift | KeyModifiers.LAlt, VIRTUAL_KEY.VK_A, new string[] { "LShift", "LAlt", "A" })]
	[InlineData(
		KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		VIRTUAL_KEY.VK_A,
		new string[] { "LCtrl", "LShift", "LAlt", "A" }
	)]
	[InlineData(
		KeyModifiers.LWin | KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		VIRTUAL_KEY.VK_A,
		new string[] { "LWin", "LCtrl", "LShift", "LAlt", "A" }
	)]
	public void Keybind_AllKeys_ReturnsCorrect(KeyModifiers modifiers, VIRTUAL_KEY key, string[] expected)
	{
		Keybind keybind = new(modifiers, key);
		Assert.Equal(expected, keybind.AllKeys);
	}
}
