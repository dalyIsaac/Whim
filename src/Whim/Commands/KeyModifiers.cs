using System;
using System.Collections.Generic;

namespace Whim;

[Flags]
public enum KeyModifiers
{
	None = 0,

	LControl = 1,
	RControl = 2,

	LShift = 4,
	RShift = 8,

	LAlt = 16,
	RAlt = 32,

	LWin = 64,
	RWin = 128,
}

public static class KeyModifiersExtensions
{
	/// <summary>
	/// Get an <see cref="IEnumerable{string}"/> of <see cref="KeyModifiers"/> names, ordered
	/// by how keybindings are normally shown.
	/// </summary>
	/// <param name="modifiers"></param>
	/// <returns></returns>
	public static IEnumerable<string> GetParts(this KeyModifiers modifiers)
	{
		List<string> parts = new();

		if (modifiers.HasFlag(KeyModifiers.LWin))
		{
			parts.Add("LWin");
		}

		if (modifiers.HasFlag(KeyModifiers.RWin))
		{
			parts.Add("RWin");
		}

		if (modifiers.HasFlag(KeyModifiers.LControl))
		{
			parts.Add("LCtrl");
		}

		if (modifiers.HasFlag(KeyModifiers.RControl))
		{
			parts.Add("RCtrl");
		}

		if (modifiers.HasFlag(KeyModifiers.LShift))
		{
			parts.Add("LShift");
		}

		if (modifiers.HasFlag(KeyModifiers.RShift))
		{
			parts.Add("RShift");
		}

		if (modifiers.HasFlag(KeyModifiers.LAlt))
		{
			parts.Add("LAlt");
		}

		if (modifiers.HasFlag(KeyModifiers.RAlt))
		{
			parts.Add("RAlt");
		}

		return parts;
	}
}
