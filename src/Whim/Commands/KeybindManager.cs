using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal class KeybindManager : IKeybindManager
{
	private readonly ICommandItems _commandItems;
	private readonly HOOKPROC _keyboardHook;
	private UnhookWindowsHookExSafeHandle? _unhookKeyboardHook;
	private bool _disposedValue;

	public KeybindManager(ICommandItems commandItems)
	{
		_commandItems = commandItems;
		_keyboardHook = KeyboardHook;
	}

	public void Initialize()
	{
		Logger.Debug("Initializing keybind manager...");
		_unhookKeyboardHook = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _keyboardHook, null, 0);
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
		Logger.Verbose($"{nCode} {wParam.Value} {lParam.Value}");
		if (nCode != 0 || ((nuint)wParam != PInvoke.WM_KEYDOWN && (nuint)wParam != PInvoke.WM_SYSKEYDOWN))
		{
			return PInvoke.CallNextHookEx(null, nCode, wParam, lParam);
		}

		KBDLLHOOKSTRUCT kbdll = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
		VIRTUAL_KEY key = (VIRTUAL_KEY)kbdll.vkCode;

		// If one of the following keys are pressed, and they're the only key pressed,
		// then we want to ignore the keypress.
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

		// There is no other way to distinguish between left and right modifier keys.
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
		Logger.Verbose(keybind.ToString());
		if (keybind.Modifiers == KeyModifiers.None)
		{
			Logger.Verbose("No modifiers");
			return false;
		}

		ICommand? command = _commandItems.TryGetCommand(keybind);
		if (command == null)
		{
			Logger.Verbose($"No handler for {keybind}");
			return false;
		}

		command.TryExecute();

		return true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
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
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
