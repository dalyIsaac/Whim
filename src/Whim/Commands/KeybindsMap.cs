using System.Collections;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// This stores the keybinds and command identifiers.
/// It ensures that it's a one-to-one mapping.
/// </summary>
internal class KeybindsMap : IEnumerable<(IKeybind, string)>
{
	// We have two dictionaries here because we want a fast way to make sure that
	// each keybind has only one corresponding command.
	private readonly Dictionary<IKeybind, string> _keybindIdentifierMap = new();
	private readonly Dictionary<string, IKeybind> _identifierKeybindMap = new();

	/// <summary>
	/// Overwrite the keybind with the new command identifier.
	/// Prevent the identifier from having multiple keybinds.
	/// </summary>
	/// <param name="keybind"></param>
	/// <param name="identifier"></param>
	public void Add(IKeybind keybind, string identifier)
	{
		Logger.Debug($"Adding keybind {keybind}");

		// Prevent the identifier from having multiple keybinds.
		if (_identifierKeybindMap.TryGetValue(identifier, out IKeybind? existingKeybind))
		{
			_keybindIdentifierMap.Remove(existingKeybind);
		}

		// Prevent the keybind from having multiple identifiers.
		if (_keybindIdentifierMap.TryGetValue(keybind, out string? existingIdentifier))
		{
			_identifierKeybindMap.Remove(existingIdentifier);
		}

		// Add the keybind to the map.
		_keybindIdentifierMap[keybind] = identifier;
		_identifierKeybindMap[identifier] = keybind;
	}

	/// <summary>
	/// Remove the keybind from the map.
	/// </summary>
	/// <param name="keybind"></param>
	/// <returns></returns>
	public bool Remove(IKeybind keybind)
	{
		Logger.Debug($"Removing keybind {keybind}");

		// Get the identifier from the keybind.
		if (!_keybindIdentifierMap.TryGetValue(keybind, out string? identifier))
		{
			return false;
		}

		// Remove the keybind from the map.
		_keybindIdentifierMap.Remove(keybind);
		_identifierKeybindMap.Remove(identifier);
		return true;
	}

	/// <summary>
	/// Remove the identifier from the map.
	/// </summary>
	/// <param name="identifier"></param>
	/// <returns></returns>
	public bool Remove(string identifier)
	{
		Logger.Debug($"Removing identifier {identifier}");

		// Get the keybind from the identifier.
		if (!_identifierKeybindMap.TryGetValue(identifier, out IKeybind? keybind))
		{
			return false;
		}

		// Remove the identifier from the map.
		_keybindIdentifierMap.Remove(keybind);
		_identifierKeybindMap.Remove(identifier);
		return true;
	}

	/// <summary>
	/// Clear the map.
	/// </summary>
	public void Clear()
	{
		Logger.Debug("Clearing keybinds");
		_keybindIdentifierMap.Clear();
		_identifierKeybindMap.Clear();
	}

	/// <summary>
	/// Get the command identifier from the keybind.
	/// </summary>
	/// <param name="keybind"></param>
	/// <returns></returns>
	public string? TryGetIdentifier(IKeybind keybind)
	{
		Logger.Debug($"Trying to get identifier for keybind {keybind}");
		return _keybindIdentifierMap.TryGetValue(keybind, out string? identifier) ? identifier : null;
	}

	/// <summary>
	/// Get the keybind from the command identifier.
	/// </summary>
	/// <param name="identifier"></param>
	/// <returns></returns>
	public IKeybind? TryGetKeybind(string identifier)
	{
		Logger.Debug($"Trying to get keybind for identifier {identifier}");
		return _identifierKeybindMap.TryGetValue(identifier, out IKeybind? keybind) ? keybind : null;
	}

	public IEnumerator<(IKeybind, string)> GetEnumerator()
	{
		foreach (KeyValuePair<IKeybind, string> keybindIdentifier in _keybindIdentifierMap)
		{
			yield return (keybindIdentifier.Key, keybindIdentifier.Value);
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
