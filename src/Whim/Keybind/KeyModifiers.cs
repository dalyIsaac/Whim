using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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
	/// <param name="unifyKeyModifiers">
	/// Whether to treat key modifiers like `LWin` and `RWin` as the same.
	/// See <see cref="IKeybindManager.UnifyKeyModifiers"/>.
	/// </param>
	/// <returns></returns>
	public static IEnumerable<string> GetParts(this KeyModifiers modifiers, bool unifyKeyModifiers) =>
		unifyKeyModifiers ? GetUnifiedModifiers(modifiers) : GetDisjointModifiers(modifiers);

	private static IEnumerable<string> GetUnifiedModifiers(KeyModifiers modifiers)
	{
		// Uses the ImmutableArray<T> builder to avoid type checks at runtime. See
		// https://devblogs.microsoft.com/dotnet/please-welcome-immutablearrayt
		ImmutableArray<string>.Builder parts = ImmutableArray.CreateBuilder<string>();
		if (modifiers.HasFlag(KeyModifiers.LWin) || modifiers.HasFlag(KeyModifiers.RWin))
		{
			parts.Add("Win");
		}
		if (modifiers.HasFlag(KeyModifiers.LControl) || modifiers.HasFlag(KeyModifiers.RControl))
		{
			parts.Add("Ctrl");
		}
		if (modifiers.HasFlag(KeyModifiers.LShift) || modifiers.HasFlag(KeyModifiers.RShift))
		{
			parts.Add("Shift");
		}
		if (modifiers.HasFlag(KeyModifiers.LAlt) || modifiers.HasFlag(KeyModifiers.RAlt))
		{
			parts.Add("Alt");
		}
		return parts.ToImmutable();
	}

	private static IEnumerable<string> GetDisjointModifiers(KeyModifiers modifiers)
	{
		// Uses the ImmutableArray<T> builder to avoid type checks at runtime. See
		// https://devblogs.microsoft.com/dotnet/please-welcome-immutablearrayt
		ImmutableArray<string>.Builder parts = ImmutableArray.CreateBuilder<string>();
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
		return parts.ToImmutable();
	}

	/// <summary>
	/// Returns a new <see cref="IKeybind"/>, with the right modifiers replaced with left
	/// modifiers.
	/// </summary>
	/// <param name="keybind"></param>
	/// <returns></returns>
	public static IKeybind UnifyModifiers(this IKeybind keybind) => new Keybind(keybind.Modifiers.Unify(), keybind.Key);

	/// <summary>
	/// Returns a new <see cref="KeyModifiers"/>, with the right modifiers replaced with left
	/// modifiers.
	/// </summary>
	/// <param name="modifiers"></param>
	/// <returns></returns>
	public static KeyModifiers Unify(this KeyModifiers modifiers)
	{
		KeyModifiers newModifiers = modifiers;
		if (modifiers.HasFlag(KeyModifiers.RWin))
		{
			newModifiers &= ~KeyModifiers.RWin;
			newModifiers |= KeyModifiers.LWin;
		}
		if (modifiers.HasFlag(KeyModifiers.RControl))
		{
			newModifiers &= ~KeyModifiers.RControl;
			newModifiers |= KeyModifiers.LControl;
		}
		if (modifiers.HasFlag(KeyModifiers.RShift))
		{
			newModifiers &= ~KeyModifiers.RShift;
			newModifiers |= KeyModifiers.LShift;
		}
		if (modifiers.HasFlag(KeyModifiers.RAlt))
		{
			newModifiers &= ~KeyModifiers.RAlt;
			newModifiers |= KeyModifiers.LAlt;
		}
		return newModifiers;
	}
}
