using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.Tests;

public class VirtualKeyExtensionsTests
{
	[Theory]
	[InlineData(VIRTUAL_KEY.VK_A, "A")]
	[InlineData(VIRTUAL_KEY.VK_Z, "Z")]
	[InlineData(VIRTUAL_KEY.VK_1, "1")]
	[InlineData(VIRTUAL_KEY.VK_9, "9")]
	[InlineData(VIRTUAL_KEY.VK_F1, "F1")]
	[InlineData(VIRTUAL_KEY.VK_F12, "F12")]
	public void GetKeyString(VIRTUAL_KEY key, string expected)
	{
		string result = key.GetKeyString();
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("A", VIRTUAL_KEY.VK_A, true)]
	[InlineData("Z", VIRTUAL_KEY.VK_Z, true)]
	[InlineData("1", VIRTUAL_KEY.VK_1, true)]
	[InlineData("9", VIRTUAL_KEY.VK_9, true)]
	[InlineData("F1", VIRTUAL_KEY.VK_F1, true)]
	[InlineData("F12", VIRTUAL_KEY.VK_F12, true)]
	[InlineData("LShift", VIRTUAL_KEY.VK_LSHIFT, true)]
	[InlineData("RShift", VIRTUAL_KEY.VK_RSHIFT, true)]
	[InlineData("OEM_1", VIRTUAL_KEY.VK_OEM_1, true)]
	[InlineData("Bob", VIRTUAL_KEY.None, false)]
	[InlineData("", VIRTUAL_KEY.None, false)]
	[InlineData(" ", VIRTUAL_KEY.None, false)]
	public void TryParseKey(string keyString, VIRTUAL_KEY expected, bool expectedSuccess)
	{
		bool success = keyString.TryParseKey(out VIRTUAL_KEY key);
		Assert.Equal(expectedSuccess, success);
		Assert.Equal(expected, key);
	}

	[Theory]
	[InlineData("CTRL", VIRTUAL_KEY.VK_LCONTROL, true)]
	[InlineData("Ctrl", VIRTUAL_KEY.VK_LCONTROL, true)]
	[InlineData("CONTROL", VIRTUAL_KEY.VK_LCONTROL, true)]
	[InlineData("Control", VIRTUAL_KEY.VK_LCONTROL, true)]
	[InlineData("LCTRL", VIRTUAL_KEY.VK_LCONTROL, true)]
	[InlineData("Lctrl", VIRTUAL_KEY.VK_LCONTROL, true)]
	[InlineData("LCONTROL", VIRTUAL_KEY.VK_LCONTROL, true)]
	[InlineData("Lcontrol", VIRTUAL_KEY.VK_LCONTROL, true)]
	[InlineData("RCTRL", VIRTUAL_KEY.VK_RCONTROL, true)]
	[InlineData("Rctrl", VIRTUAL_KEY.VK_RCONTROL, true)]
	[InlineData("RCONTROL", VIRTUAL_KEY.VK_RCONTROL, true)]
	[InlineData("Rcontrol", VIRTUAL_KEY.VK_RCONTROL, true)]
	[InlineData("SHIFT", VIRTUAL_KEY.VK_LSHIFT, true)]
	[InlineData("Shift", VIRTUAL_KEY.VK_LSHIFT, true)]
	[InlineData("LSHIFT", VIRTUAL_KEY.VK_LSHIFT, true)]
	[InlineData("Lshift", VIRTUAL_KEY.VK_LSHIFT, true)]
	[InlineData("RSHIFT", VIRTUAL_KEY.VK_RSHIFT, true)]
	[InlineData("Rshift", VIRTUAL_KEY.VK_RSHIFT, true)]
	[InlineData("ALT", VIRTUAL_KEY.VK_LALT, true)]
	[InlineData("Alt", VIRTUAL_KEY.VK_LALT, true)]
	[InlineData("LALT", VIRTUAL_KEY.VK_LALT, true)]
	[InlineData("Lalt", VIRTUAL_KEY.VK_LALT, true)]
	[InlineData("RALT", VIRTUAL_KEY.VK_RALT, true)]
	[InlineData("Ralt", VIRTUAL_KEY.VK_RALT, true)]
	[InlineData("WIN", VIRTUAL_KEY.VK_LWIN, true)]
	[InlineData("Win", VIRTUAL_KEY.VK_LWIN, true)]
	[InlineData("LWIN", VIRTUAL_KEY.VK_LWIN, true)]
	[InlineData("Lwin", VIRTUAL_KEY.VK_LWIN, true)]
	[InlineData("RWIN", VIRTUAL_KEY.VK_RWIN, true)]
	[InlineData("Rwin", VIRTUAL_KEY.VK_RWIN, true)]
	[InlineData(" ", VIRTUAL_KEY.None, false)]
	[InlineData("", VIRTUAL_KEY.None, false)]
	[InlineData("Bob", VIRTUAL_KEY.None, false)]
	[InlineData("A", VIRTUAL_KEY.None, false)]
	public void TryParseKeyModifier(string keyString, VIRTUAL_KEY expected, bool expectedSuccess)
	{
		// When
		bool success = keyString.TryParseKeyModifier(out VIRTUAL_KEY key);

		// Then
		Assert.Equal(expectedSuccess, success);
		Assert.Equal(expected, key);
	}
}
