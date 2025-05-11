using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.Tests;

public class KeybindMapTests
{
	[Fact]
	public void AddKeybindWithSingleKey()
	{
		KeybindMap keybindMap = new();
		IKeybind keybind = new Keybind(VIRTUAL_KEY.VK_A);
		keybindMap.Add(keybind);

		Assert.Single(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_A));
		Assert.Empty(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_B));
	}

	[Fact]
	public void AddKeybindWithMultipleKeys()
	{
		KeybindMap keybindMap = new();
		IKeybind keybind = new Keybind(VIRTUAL_KEY.VK_A, VIRTUAL_KEY.VK_B);
		keybindMap.Add(keybind);

		Assert.Single(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_A));
		Assert.Single(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_B));
		Assert.Empty(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_C));
	}

	[Fact]
	public void RemoveKeybind()
	{
		KeybindMap keybindMap = new();
		IKeybind keybind1 = new Keybind(VIRTUAL_KEY.VK_A);
		IKeybind keybind2 = new Keybind(VIRTUAL_KEY.VK_A, VIRTUAL_KEY.VK_B);

		keybindMap.Add(keybind1);
		keybindMap.Add(keybind2);
		keybindMap.Remove(keybind1);

		Assert.Empty(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_A));
		Assert.Single(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_B));
	}
}
