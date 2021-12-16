using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim.Core;

public class KeybindManager : IKeybindManager
{
	private readonly IConfigContext _configContext;
	private readonly Dictionary<IKeybind, KeybindHandler> _keybinds = new();

	private readonly HOOKPROC _keyboardHook;
	private UnhookWindowsHookExSafeHandle? _unhookKeyboardHook;
	private bool disposedValue;

	public KeybindManager(IConfigContext configContext)
	{
		_configContext = configContext;
		_keyboardHook = KeyboardHook;
	}

	public void Initialize()
	{
		Logger.Debug("Initializing keybind manager...");
		_unhookKeyboardHook = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _keyboardHook, null, 0);
		// TODO: mouse
	}

	/// <summary>
	/// For relevant documentation, see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-hookproc
	/// </summary>
	/// <param name="nCode"></param>
	/// <param name="wParam"></param>
	/// <param name="lParam"></param>
	/// <returns></returns>
	private LRESULT KeyboardHook(int nCode, WPARAM wParam, LPARAM lParam)
	{
		Logger.Verbose("KeyboardHook: {nCode} {wParam} {lParam}", nCode, wParam, lParam);
		if (nCode != 0 || (wParam != (nuint)PInvoke.WM_KEYDOWN && wParam != (nuint)PInvoke.WM_SYSKEYDOWN))
		{
			return PInvoke.CallNextHookEx(null, nCode, wParam, lParam);
		}

		KBDLLHOOKSTRUCT kbdll = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
		VIRTUAL_KEY key = (VIRTUAL_KEY)kbdll.vkCode;

		switch (key)
		{
			case VIRTUAL_KEY.VK_LSHIFT:
			case VIRTUAL_KEY.VK_RSHIFT:
			case VIRTUAL_KEY.VK_LMENU:
			case VIRTUAL_KEY.VK_RMENU:
			case VIRTUAL_KEY.VK_LCONTROL:
			case VIRTUAL_KEY.VK_RCONTROL:
			case VIRTUAL_KEY.VK_LWIN:
			case VIRTUAL_KEY.VK_RWIN:
				return PInvoke.CallNextHookEx(null, nCode, wParam, lParam);
			default:
				break;
		}

		KeyModifiers modifiers = GetModifiersPressed();
		if (DoKeyboardEvent(new Keybind(modifiers, key)))
		{
			return (LRESULT)1;
		}

		return PInvoke.CallNextHookEx(null, nCode, wParam, lParam);
	}

	private static bool IsModifierPressed(VIRTUAL_KEY key) => (PInvoke.GetKeyState((int)key) & 0x8000) == 0x8000;

	private static KeyModifiers GetModifiersPressed()
	{
		KeyModifiers modifiers = 0;

		// there is no other way to distinguish between left and right modifier keys
		if (IsModifierPressed(VIRTUAL_KEY.VK_LSHIFT))
		{
			modifiers |= KeyModifiers.LShift;
		}
		if (IsModifierPressed(VIRTUAL_KEY.VK_RSHIFT))
		{
			modifiers |= KeyModifiers.RShift;
		}
		if (IsModifierPressed(VIRTUAL_KEY.VK_LMENU))
		{
			modifiers |= KeyModifiers.LAlt;
		}
		if (IsModifierPressed(VIRTUAL_KEY.VK_RMENU))
		{
			modifiers |= KeyModifiers.RAlt;
		}
		if (IsModifierPressed(VIRTUAL_KEY.VK_LCONTROL))
		{
			modifiers |= KeyModifiers.LControl;
		}
		if (IsModifierPressed(VIRTUAL_KEY.VK_RCONTROL))
		{
			modifiers |= KeyModifiers.RControl;
		}
		if (IsModifierPressed(VIRTUAL_KEY.VK_LWIN))
		{
			modifiers |= KeyModifiers.LWin;
		}
		if (IsModifierPressed(VIRTUAL_KEY.VK_RWIN))
		{
			modifiers |= KeyModifiers.RWin;
		}

		return modifiers;
	}

	private bool DoKeyboardEvent(Keybind keybind)
	{
		Logger.Debug("DoKeyboardEvent: {keybind}", keybind);
		if (keybind.Modifiers == KeyModifiers.None)
		{
			Logger.Debug("DoKeyboardEvent: no modifiers");
			return false;
		}

		if (!_keybinds.TryGetValue(keybind, out KeybindHandler? handler) || handler == null)
		{
			Logger.Debug("DoKeyboardEvent: no handler for {keybind}", keybind);
			return false;
		}

		handler.Invoke(keybind);
		return true;
	}

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

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				if (_unhookKeyboardHook != null && !_unhookKeyboardHook.IsInvalid)
				{
					_unhookKeyboardHook.Dispose();
				}
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
