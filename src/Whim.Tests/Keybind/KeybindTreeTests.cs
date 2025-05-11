using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.Tests;

public class KeybindTreeTests
{
	[Fact]
	public void AddKeybindWithSingleKey()
	{
		// Given
		KeybindTree keybindMap = new();
		IKeybind keybind = new Keybind(VIRTUAL_KEY.VK_A);

		// When
		keybindMap.Add(keybind);

		// Then
		Assert.Single(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_A));
		Assert.Empty(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_B));
	}

	[Fact]
	public void AddKeybindWithMultipleKeys()
	{
		// Given
		KeybindTree keybindMap = new();
		IKeybind keybind = new Keybind(VIRTUAL_KEY.VK_A, VIRTUAL_KEY.VK_B);

		// When
		keybindMap.Add(keybind);

		// Then
		Assert.Single(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_A));
		Assert.Single(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_B));
		Assert.Empty(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_C));
	}

	[Fact]
	public void RemoveKeybind()
	{
		// Given
		KeybindTree keybindMap = new();
		IKeybind keybind1 = new Keybind(VIRTUAL_KEY.VK_A);
		IKeybind keybind2 = new Keybind(VIRTUAL_KEY.VK_A, VIRTUAL_KEY.VK_B);

		// When
		keybindMap.Add(keybind1);
		keybindMap.Add(keybind2);
		keybindMap.Remove(keybind1);

		// Then
		Assert.Empty(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_A));
		Assert.Single(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_B));
	}

	[Fact]
	public void ClearKeybinds()
	{
		// Given
		KeybindTree keybindMap = new();
		IKeybind keybind1 = new Keybind(VIRTUAL_KEY.VK_A);
		IKeybind keybind2 = new Keybind(VIRTUAL_KEY.VK_B);

		// When
		keybindMap.Add(keybind1);
		keybindMap.Add(keybind2);
		keybindMap.Clear();

		// Then
		Assert.Empty(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_A));
		Assert.Empty(keybindMap.GetKeybindsForKey(VIRTUAL_KEY.VK_B));
	}
}
