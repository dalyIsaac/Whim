using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

/// <summary>
///Stores a mapping of keys to keybinds.
/// </summary>
internal class KeybindMap
{
	private readonly Dictionary<VIRTUAL_KEY, List<IKeybind>> _keyKeybindsMap = [];

	public void Add(IKeybind keybind)
	{
		foreach (VIRTUAL_KEY key in keybind.Keys)
		{
			AddKeyKeybind(key, keybind);
		}
	}

	private void AddKeyKeybind(VIRTUAL_KEY key, IKeybind keybind)
	{
		if (!_keyKeybindsMap.TryGetValue(key, out List<IKeybind>? existingKeybinds))
		{
			existingKeybinds = [];
			_keyKeybindsMap.Add(key, existingKeybinds);
		}

		if (existingKeybinds.Contains(keybind))
		{
			return;
		}

		existingKeybinds.Add(keybind);
	}

	public void Remove(IKeybind keybind)
	{
		foreach (VIRTUAL_KEY key in keybind.Keys)
		{
			if (_keyKeybindsMap.TryGetValue(key, out List<IKeybind>? existingKeybinds))
			{
				existingKeybinds.Remove(keybind);
			}
		}
	}

	public IEnumerable<IKeybind> GetKeybindsForKey(VIRTUAL_KEY key)
	{
		if (_keyKeybindsMap.TryGetValue(key, out List<IKeybind>? keybinds))
		{
			return keybinds;
		}

		return [];
	}
}
