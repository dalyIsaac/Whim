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

	[Fact]
	public void Keybind_ToString_WithMultipleModifiers_IncludingNonStandard()
	{
		// Using non-standard modifiers like VK_ESCAPE and VK_SPACE
		Keybind keybind = new([VIRTUAL_KEY.VK_ESCAPE, VIRTUAL_KEY.VK_SPACE, VIRTUAL_KEY.VK_CONTROL], VIRTUAL_KEY.VK_A);
		Assert.Equal("Control + Escape + Space + A", keybind.ToString());
	}

	[Theory]
	[MemberData(nameof(GetEnumerableModifiersTestData))]
	public void Keybind_WithEnumerableConstructor_ReturnsCorrectString(
		VIRTUAL_KEY[] modifiers,
		VIRTUAL_KEY key,
		string expected
	)
	{
		Keybind keybind = new(modifiers, key);
		Assert.Equal(expected, keybind.ToString());
	}

	public static TheoryData<VIRTUAL_KEY[], VIRTUAL_KEY, string> GetEnumerableModifiersTestData =>
		new()
		{
			{ Array.Empty<VIRTUAL_KEY>(), VIRTUAL_KEY.VK_A, "A" },
			{ new[] { VIRTUAL_KEY.VK_LSHIFT }, VIRTUAL_KEY.VK_A, "LShift + A" },
			{ new[] { VIRTUAL_KEY.VK_LCONTROL }, VIRTUAL_KEY.VK_A, "LCtrl + A" },
			{ new[] { VIRTUAL_KEY.VK_LMENU }, VIRTUAL_KEY.VK_A, "LAlt + A" },
			{ new[] { VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT }, VIRTUAL_KEY.VK_A, "LCtrl + LShift + A" },
			{ new[] { VIRTUAL_KEY.VK_RSHIFT }, VIRTUAL_KEY.VK_A, "RShift + A" },
			{
				new[] { VIRTUAL_KEY.VK_LWIN, VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT, VIRTUAL_KEY.VK_LMENU },
				VIRTUAL_KEY.VK_A,
				"LWin + LCtrl + LShift + LAlt + A"
			},
		};

	[Theory]
	[InlineData("A", KeyModifiers.None, VIRTUAL_KEY.VK_A)]
	[InlineData("B + A", KeyModifiers.None, VIRTUAL_KEY.VK_A)] // B will be treated as a modifier
	[InlineData("LShift + A", KeyModifiers.LShift, VIRTUAL_KEY.VK_A)]
	[InlineData("LCtrl + A", KeyModifiers.LControl, VIRTUAL_KEY.VK_A)]
	[InlineData("LAlt + A", KeyModifiers.LAlt, VIRTUAL_KEY.VK_A)]
	[InlineData(
		"LWin+LCtrl+LShift+LAlt+A",
		KeyModifiers.LWin | KeyModifiers.LControl | KeyModifiers.LShift | KeyModifiers.LAlt,
		VIRTUAL_KEY.VK_A
	)]
	[Obsolete("Tests for the obsolete Modifiers property.")]
	public void Keybind_FromString(string input, KeyModifiers expectedModifiers, VIRTUAL_KEY expectedKey)
	{
		IKeybind? keybind = Keybind.FromString(input);
		Assert.NotNull(keybind);
		Assert.Equal(expectedModifiers, keybind.Modifiers);
		Assert.Equal(expectedKey, keybind.Key);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("+++++")]
	[InlineData("LCtrl + LShift + LAlt")]
	public void Keybind_FromString_Invalid_ReturnsNull(string input)
	{
		IKeybind? keybind = Keybind.FromString(input);
		Assert.Null(keybind);
	}

	[Fact]
	public void Keybind_Equals_Same_ReturnsTrue()
	{
		// Using the new constructor with IEnumerable<VIRTUAL_KEY>
		Keybind keybind1 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);
		Keybind keybind2 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);

		Assert.True(keybind1.Equals(keybind2));
		Assert.True(keybind1.Equals((object)keybind2));
	}

	[Fact]
	public void Keybind_Equals_Different_ReturnsFalse()
	{
		Keybind keybind1 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);
		Keybind keybind2 = new([VIRTUAL_KEY.VK_LCONTROL], VIRTUAL_KEY.VK_A);
		Keybind keybind3 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_B);

		Assert.False(keybind1.Equals(keybind2));
		Assert.False(keybind1.Equals(keybind3));
		Assert.False(keybind1.Equals(null));
		Assert.False(keybind1.Equals("not a keybind"));
	}

	[Fact]
	public void Keybind_EqualsOperator_Same_ReturnsTrue()
	{
		Keybind keybind1 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);
		Keybind keybind2 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);

		Assert.True(keybind1 == keybind2);
	}

	[Fact]
	public void Keybind_NotEqualsOperator_Different_ReturnsTrue()
	{
		Keybind keybind1 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);
		Keybind keybind2 = new([VIRTUAL_KEY.VK_LCONTROL], VIRTUAL_KEY.VK_A);

		Assert.True(keybind1 != keybind2);
	}

	[Fact]
	public void Keybind_GetHashCode_Same_ReturnsSameValue()
	{
		Keybind keybind1 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);
		Keybind keybind2 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);

		Assert.Equal(keybind1.GetHashCode(), keybind2.GetHashCode());
	}

	[Fact]
	public void Keybind_GetHashCode_Different_ReturnsDifferentValues()
	{
		Keybind keybind1 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);
		Keybind keybind2 = new([VIRTUAL_KEY.VK_LCONTROL], VIRTUAL_KEY.VK_A);
		Keybind keybind3 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_B);

		Assert.NotEqual(keybind1.GetHashCode(), keybind2.GetHashCode());
		Assert.NotEqual(keybind1.GetHashCode(), keybind3.GetHashCode());
	}

	[Fact]
	public void Keybind_DifferentModifierOrder_ProducesSameKeybind()
	{
		// The constructor should sort modifiers internally, so order shouldn't matter
		Keybind keybind1 = new([VIRTUAL_KEY.VK_LSHIFT, VIRTUAL_KEY.VK_LCONTROL], VIRTUAL_KEY.VK_A);
		Keybind keybind2 = new([VIRTUAL_KEY.VK_LCONTROL, VIRTUAL_KEY.VK_LSHIFT], VIRTUAL_KEY.VK_A);

		Assert.Equal(keybind1, keybind2);
		Assert.Equal(keybind1.ToString(), keybind2.ToString());
	}
}
