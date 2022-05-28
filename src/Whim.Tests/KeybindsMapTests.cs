using System.Collections.Generic;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Tests;

public class KeybindsMapTests
{
	[Fact]
	public void AddSingleCommand()
	{
		KeybindsMap keybindsMap = new();

		string identifier = "command";
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		keybindsMap.Add(keybind, identifier);

		Assert.Equal(identifier, keybindsMap.TryGetIdentifier(keybind));
		Assert.Equal(keybind, keybindsMap.TryGetKeybind(identifier));
	}

	[Fact]
	public void AddOverwriteCommand()
	{
		KeybindsMap keybindsMap = new();

		string identifier = "command";
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);
		keybindsMap.Add(keybind, identifier);

		string identifier2 = "command2";
		Keybind keybind2 = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);

		keybindsMap.Add(keybind2, identifier2);

		// Verify the keybind maps to the second command.
		Assert.Equal(identifier2, keybindsMap.TryGetIdentifier(keybind));
		Assert.Equal(keybind2, keybindsMap.TryGetKeybind(identifier2));

		// Verify the first command is not mapped.
		Assert.Null(keybindsMap.TryGetKeybind(identifier));
	}

	[Fact]
	public void AddTwoCommandsWithSameIdentifier()
	{
		KeybindsMap keybindsMap = new();

		string identifier = "command";
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);
		keybindsMap.Add(keybind, identifier);

		Keybind keybind2 = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_G);
		keybindsMap.Add(keybind2, identifier);

		// Verify that the first keybind is not mapped.
		Assert.Null(keybindsMap.TryGetIdentifier(keybind));

		// Verify that the second keybind is mapped to the identifier.
		Assert.Equal(identifier, keybindsMap.TryGetIdentifier(keybind2));
		Assert.Equal(keybind2, keybindsMap.TryGetKeybind(identifier));
	}

	[Fact]
	public void RemoveKeybind()
	{
		KeybindsMap keybindsMap = new();

		string identifier = "command";
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);
		keybindsMap.Add(keybind, identifier);

		Assert.True(keybindsMap.Remove(keybind));
		Assert.False(keybindsMap.Remove(keybind));

		Assert.Null(keybindsMap.TryGetIdentifier(keybind));
		Assert.Null(keybindsMap.TryGetKeybind(identifier));
	}

	[Fact]
	public void RemoveIdentifier()
	{
		KeybindsMap keybindsMap = new();

		string identifier = "command";
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);
		keybindsMap.Add(keybind, identifier);

		Assert.True(keybindsMap.Remove(identifier));
		Assert.False(keybindsMap.Remove(identifier));

		Assert.Null(keybindsMap.TryGetIdentifier(keybind));
		Assert.Null(keybindsMap.TryGetKeybind(identifier));
	}

	[Fact]
	public void Clear()
	{
		KeybindsMap keybindsMap = new();

		string identifier = "command";
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);
		keybindsMap.Add(keybind, identifier);

		keybindsMap.Clear();

		Assert.Null(keybindsMap.TryGetIdentifier(keybind));
		Assert.Null(keybindsMap.TryGetKeybind(identifier));
	}

	[Fact]
	public void Enumerator()
	{
		KeybindsMap keybindsMap = new();

		string identifier = "command";
		Keybind keybind = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);
		keybindsMap.Add(keybind, identifier);

		string identifier2 = "command2";
		Keybind keybind2 = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_F);
		keybindsMap.Add(keybind2, identifier2);

		string identifier3 = "command3";
		Keybind keybind3 = new(KeyModifiers.RWin, VIRTUAL_KEY.VK_G);
		keybindsMap.Add(keybind3, identifier3);

		// Get the generic enumerator.
		IEnumerator<(IKeybind, string)> genericEnumerator = keybindsMap.GetEnumerator();
		List<(IKeybind, string)> genericList = new();
		while (genericEnumerator.MoveNext())
		{
			genericList.Add(genericEnumerator.Current);
		}
		// Verify the counts are correct.
		Assert.Equal(2, genericList.Count);

		// Verify the contents are correct.
		Assert.Equal(keybind, genericList[0].Item1);
		Assert.Equal(identifier2, genericList[0].Item2);

		Assert.Equal(keybind3, genericList[1].Item1);
		Assert.Equal(identifier3, genericList[1].Item2);
	}
}
