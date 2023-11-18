using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Responsible is responsible for hooking into windows and handling keybinds.
/// </summary>
internal class KeybindHook : IKeybindHook
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly HOOKPROC _lowLevelKeyboardProc;
	private UnhookWindowsHookExSafeHandle? _unhookKeyboardHook;
	private bool _disposedValue;

	public KeybindHook(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
		_lowLevelKeyboardProc = LowLevelKeyboardProcWrapper;
	}

	public void PostInitialize()
	{
		Logger.Debug("Initializing keybind manager...");
		_unhookKeyboardHook = _internalContext
			.CoreNativeManager
			.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, _lowLevelKeyboardProc, null, 0);
	}

	private LRESULT LowLevelKeyboardProcWrapper(int nCode, WPARAM wParam, LPARAM lParam)
	{
		try
		{
			return LowLevelKeyboardProc(nCode, wParam, lParam);
		}
		catch (Exception e)
		{
			_context.HandleUncaughtException(nameof(LowLevelKeyboardProc), e);
			return _internalContext.CoreNativeManager.CallNextHookEx(nCode, wParam, lParam);
		}
	}

	/// <summary>
	/// For relevant documentation, see https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelkeyboardproc
	/// </summary>
	/// <param name="nCode"></param>
	/// <param name="wParam"></param>
	/// <param name="lParam"></param>
	/// <returns></returns>
	private LRESULT LowLevelKeyboardProc(int nCode, WPARAM wParam, LPARAM lParam)
	{
		Logger.Verbose($"{nCode} {wParam.Value} {lParam.Value}");
		if (nCode != 0 || ((nuint)wParam != PInvoke.WM_KEYDOWN && (nuint)wParam != PInvoke.WM_SYSKEYDOWN))
		{
			return _internalContext.CoreNativeManager.CallNextHookEx(nCode, wParam, lParam);
		}

		if (_internalContext.CoreNativeManager.PtrToStructure<KBDLLHOOKSTRUCT>(lParam) is not KBDLLHOOKSTRUCT kbdll)
		{
			return _internalContext.CoreNativeManager.CallNextHookEx(nCode, wParam, lParam);
		}
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
				return _internalContext.CoreNativeManager.CallNextHookEx(nCode, wParam, lParam);
			default:
				break;
		}

		KeyModifiers modifiers = GetModifiersPressed();
		if (modifiers == KeyModifiers.None)
		{
			Logger.Verbose("No modifiers");
			return _internalContext.CoreNativeManager.CallNextHookEx(nCode, wParam, lParam);
		}

		if (DoKeyboardEvent(new Keybind(modifiers, key)))
		{
			return (LRESULT)1;
		}

		return _internalContext.CoreNativeManager.CallNextHookEx(nCode, wParam, lParam);
	}

	private bool IsModifierPressed(VIRTUAL_KEY key) =>
		(_internalContext.CoreNativeManager.GetKeyState((int)key) & 0x8000) == 0x8000;

	private KeyModifiers GetModifiersPressed()
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
		ICommand[] commands = _context.KeybindManager.GetCommands(keybind);

		if (commands.Length == 0)
		{
			Logger.Verbose($"No handler for {keybind}");
			return false;
		}

		foreach (ICommand command in commands)
		{
			command.TryExecute();
		}

		return true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_unhookKeyboardHook?.Dispose();
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
