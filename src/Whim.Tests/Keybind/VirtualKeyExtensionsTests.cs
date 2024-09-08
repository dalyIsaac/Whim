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
}
