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
	[InlineData(VIRTUAL_KEY.VK_LCONTROL, "Ctrl")]
	[InlineData(VIRTUAL_KEY.VK_RCONTROL, "Ctrl")]
	[InlineData(VIRTUAL_KEY.VK_LSHIFT, "Shift")]
	[InlineData(VIRTUAL_KEY.VK_RSHIFT, "Shift")]
	[InlineData(VIRTUAL_KEY.VK_LALT, "Alt")]
	[InlineData(VIRTUAL_KEY.VK_RALT, "Alt")]
	[InlineData(VIRTUAL_KEY.VK_LWIN, "Win")]
	[InlineData(VIRTUAL_KEY.VK_RWIN, "Win")]
	[InlineData(VIRTUAL_KEY.VK_A, "A")] // Non-modifier keys should remain the same
	[InlineData(VIRTUAL_KEY.VK_F1, "F1")] // Non-modifier keys should remain the same
	public void GetKeyString_UnifiedModifiers(VIRTUAL_KEY key, string expected)
	{
		string result = key.GetKeyString(unifyKeyModifiers: true);
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
	[InlineData("Shift", VIRTUAL_KEY.VK_LSHIFT, true)]
	[InlineData("Ctrl", VIRTUAL_KEY.VK_LCONTROL, true)]
	[InlineData("Alt", VIRTUAL_KEY.VK_LALT, true)]
	[InlineData("Win", VIRTUAL_KEY.VK_LWIN, true)]
	public void TryParseKey_UnifiedModifiers(string keyString, VIRTUAL_KEY expected, bool expectedSuccess)
	{
		bool success = keyString.TryParseKey(out VIRTUAL_KEY key);
		Assert.Equal(expectedSuccess, success);
		Assert.Equal(expected, key);
	}

	[Theory]
	[InlineData(VIRTUAL_KEY.VK_LCONTROL, KeyModifiers.LControl, true)]
	[InlineData(VIRTUAL_KEY.VK_RCONTROL, KeyModifiers.RControl, true)]
	[InlineData(VIRTUAL_KEY.VK_LSHIFT, KeyModifiers.LShift, true)]
	[InlineData(VIRTUAL_KEY.VK_RSHIFT, KeyModifiers.RShift, true)]
	[InlineData(VIRTUAL_KEY.VK_LMENU, KeyModifiers.LAlt, true)]
	[InlineData(VIRTUAL_KEY.VK_RMENU, KeyModifiers.RAlt, true)]
	[InlineData(VIRTUAL_KEY.VK_LWIN, KeyModifiers.LWin, true)]
	[InlineData(VIRTUAL_KEY.VK_RWIN, KeyModifiers.RWin, true)]
	[InlineData(VIRTUAL_KEY.VK_A, KeyModifiers.None, false)] // Non-modifier key
	[InlineData(VIRTUAL_KEY.VK_F1, KeyModifiers.None, false)] // Non-modifier key
	[InlineData(VIRTUAL_KEY.VK_ESCAPE, KeyModifiers.None, false)] // Non-modifier key
	public void TryGetModifier(VIRTUAL_KEY key, KeyModifiers expectedModifier, bool expectedSuccess)
	{
		bool success = key.TryGetModifier(out KeyModifiers modifier);
		Assert.Equal(expectedSuccess, success);
		Assert.Equal(expectedModifier, modifier);
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

	[Fact]
	public void SortModifiers_ShouldSortCorrectly()
	{
		// Given
		var unsortedModifiers = new List<VIRTUAL_KEY>
		{
			VIRTUAL_KEY.VK_RALT,
			VIRTUAL_KEY.VK_LSHIFT,
			VIRTUAL_KEY.VK_LWIN,
			VIRTUAL_KEY.VK_RCONTROL,
		};

		// Expected order based on GetVirtualKeyIndex implementation:
		// VK_LWIN (0), VK_RWIN (1), VK_LCONTROL (2), VK_RCONTROL (3),
		// VK_LSHIFT (4), VK_RSHIFT (5), VK_LALT (6), VK_RALT (7)
		var expectedSortedModifiers = new[]
		{
			VIRTUAL_KEY.VK_LWIN,
			VIRTUAL_KEY.VK_RCONTROL,
			VIRTUAL_KEY.VK_LSHIFT,
			VIRTUAL_KEY.VK_RALT,
		};

		// When
		var sortedModifiers = VirtualKeyExtensions.SortModifiers(unsortedModifiers);

		// Then
		Assert.Equal(expectedSortedModifiers, sortedModifiers);
	}

	[Fact]
	public void SortModifiers_ShouldRemoveDuplicates()
	{
		// Given
		var modifiersWithDuplicates = new List<VIRTUAL_KEY>
		{
			VIRTUAL_KEY.VK_LALT,
			VIRTUAL_KEY.VK_LSHIFT,
			VIRTUAL_KEY.VK_LALT,
			VIRTUAL_KEY.VK_LWIN,
			VIRTUAL_KEY.VK_LSHIFT,
		};

		// Expected order with duplicates removed
		var expectedSortedModifiers = new[] { VIRTUAL_KEY.VK_LWIN, VIRTUAL_KEY.VK_LSHIFT, VIRTUAL_KEY.VK_LALT };

		// When
		var sortedModifiers = VirtualKeyExtensions.SortModifiers(modifiersWithDuplicates);

		// Then
		Assert.Equal(expectedSortedModifiers, sortedModifiers);
		Assert.Equal(3, sortedModifiers.Length); // Should only have 3 elements after removing duplicates
	}

	[Fact]
	public void SortModifiers_EmptyCollection_ShouldReturnEmptyArray()
	{
		// Given
		var emptyModifiers = new List<VIRTUAL_KEY>();

		// When
		var sortedModifiers = VirtualKeyExtensions.SortModifiers(emptyModifiers);

		// Then
		Assert.Empty(sortedModifiers);
	}

	[Fact]
	public void SortModifiers_WithNonModifierKeys_ShouldSortBasedOnVirtualKeyIndex()
	{
		// Given
		var mixedModifiers = new List<VIRTUAL_KEY>
		{
			VIRTUAL_KEY.VK_F1,
			VIRTUAL_KEY.VK_LWIN,
			VIRTUAL_KEY.VK_A,
			VIRTUAL_KEY.VK_LSHIFT,
		};

		// When
		var sortedModifiers = VirtualKeyExtensions.SortModifiers(mixedModifiers);

		// Then
		// Non-modifier keys should appear after the modifier keys based on their int value
		Assert.Equal(VIRTUAL_KEY.VK_LWIN, sortedModifiers[0]);
		Assert.Equal(VIRTUAL_KEY.VK_LSHIFT, sortedModifiers[1]);
		// These two should be in order of their integer values
		Assert.True(((int)sortedModifiers[2]) < ((int)sortedModifiers[3]));
		Assert.Equal(4, sortedModifiers.Length);
	}
}
