using Windows.Win32.UI.Input.KeyboardAndMouse;

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
	[InlineData(KeyModifiers.RShift, VIRTUAL_KEY.VK_A, "RShift + A")]
	[InlineData(KeyModifiers.RControl, VIRTUAL_KEY.VK_A, "RCtrl + A")]
	public void Keybind_ToString_ReturnsCorrectString(KeyModifiers modifiers, VIRTUAL_KEY key, string expected)
	{
		Keybind keybind = new(modifiers, key);
		Assert.Equal(expected, keybind.ToString());
	}

	[Theory]
	[InlineData(KeyModifiers.None, VIRTUAL_KEY.VK_A, "A")]
	[InlineData(KeyModifiers.LShift, VIRTUAL_KEY.VK_A, "Shift + A")]
	[InlineData(KeyModifiers.LControl, VIRTUAL_KEY.VK_A, "Ctrl + A")]
	[InlineData(KeyModifiers.LAlt, VIRTUAL_KEY.VK_A, "Alt + A")]
	[InlineData(KeyModifiers.LShift | KeyModifiers.LControl, VIRTUAL_KEY.VK_A, "Ctrl + Shift + A")]
	[InlineData(KeyModifiers.LControl | KeyModifiers.LAlt, VIRTUAL_KEY.VK_A, "Ctrl + Alt + A")]
	[InlineData(KeyModifiers.LShift | KeyModifiers.LAlt, VIRTUAL_KEY.VK_A, "Shift + Alt + A")]
	[InlineData(
		KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		VIRTUAL_KEY.VK_A,
		"Ctrl + Shift + Alt + A"
	)]
	[InlineData(
		KeyModifiers.LWin | KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		VIRTUAL_KEY.VK_A,
		"Win + Ctrl + Shift + Alt + A"
	)]
	[InlineData(KeyModifiers.RShift, VIRTUAL_KEY.VK_A, "Shift + A")]
	[InlineData(KeyModifiers.RControl, VIRTUAL_KEY.VK_A, "Ctrl + A")]
	public void Keybind_ToString_UnifyKeyModifiers_ReturnsCorrectString(
		KeyModifiers modifiers,
		VIRTUAL_KEY key,
		string expected
	)
	{
		Keybind keybind = new(modifiers, key);
		Assert.Equal(expected, keybind.ToString(unifyKeyModifiers: true));
	}
}
