using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim.Core.Binds;

public class KeybindManager : IKeybindManager
{
	private readonly Dictionary<IKeybind, KeybindHandler> _keybinds = new();

	public KeybindHandler? this[IKeybind keybind]
	{
		get
		{
			Logger.Debug("Getting keybind handler for keybind {0}", keybind);
			return TryGet(keybind);
		}
		set
		{
			Logger.Debug("Setting keybind handler for keybind {0}", keybind);
			if (value == null)
			{
				// This really isn't ideal, but just in case.
				Logger.Warning("Tried to set keybind handler to null. Removing keybind {0}", keybind);
				_keybinds.Remove(keybind);
			}
			else
			{
				_keybinds[keybind] = value;
			}
		}
	}

	public int Count => _keybinds.Count;

	public void Add(IKeybind keybind, KeybindHandler handler, bool throwIfExists = false)
	{
		Logger.Debug("Adding keybind {0}", keybind);
		if (_keybinds.ContainsKey(keybind))
		{
			Logger.Warning("Keybind {0} already exists", keybind);
			if (throwIfExists)
			{
				throw new ArgumentException("Keybind already exists");
			}
		}

		_keybinds.Add(keybind, handler);
	}

	public void Clear()
	{
		Logger.Debug("Clearing keybinds");
		_keybinds.Clear();
	}

	public IEnumerator<KeyValuePair<IKeybind, KeybindHandler>> GetEnumerator() => _keybinds.GetEnumerator();

	public bool Remove(IKeybind keybind)
	{
		Logger.Debug("Removing keybind {0}", keybind);
		return _keybinds.Remove(keybind);
	}

	public KeybindHandler? TryGet(IKeybind keybind)
	{
		Logger.Debug("Trying to get keybind handler for keybind {0}", keybind);
		return _keybinds.TryGetValue(keybind, out KeybindHandler? handler) ? handler : null;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
