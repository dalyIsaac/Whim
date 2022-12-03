using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// The available key modifiers.
/// </summary>
[Flags]
public enum KeyModifiers
{
	/// <summary>
	/// No modifiers.
	/// </summary>
	None = 0,

	/// <summary>
	/// The <c>LControl</c> modifier key.
	/// </summary>
	LControl = 1,

	/// <summary>
	/// The <c>RControl</c> modifier key.
	/// </summary>
	RControl = 2,

	/// <summary>
	/// The <c>LShift</c> modifier key.
	/// </summary>
	LShift = 4,

	/// <summary>
	/// The <c>RShift</c> modifier key.
	/// </summary>
	RShift = 8,

	/// <summary>
	/// The <c>LAlt</c> modifier key.
	/// </summary>
	LAlt = 16,

	/// <summary>
	/// The <c>RAlt</c> modifier key.
	/// </summary>
	RAlt = 32,

	/// <summary>
	/// The <c>LWin</c> modifier key.
	/// </summary>
	LWin = 64,

	/// <summary>
	/// The <c>RWin</c> modifier key.
	/// </summary>
	RWin = 128,
}

/// <summary>
/// Extension methods for <see cref="KeyModifiers"/>.
/// </summary>
public static class KeyModifiersExtensions
{
	/// <summary>
	/// Get an <see cref="IEnumerable{T}"/> of <see cref="KeyModifiers"/> names, ordered
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
