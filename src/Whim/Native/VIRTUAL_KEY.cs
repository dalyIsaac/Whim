#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Windows.Win32
{
	namespace UI.Input.KeyboardAndMouse
	{
		/// <summary>
		/// Symbolic constant names and virtual-key codes used by the system.
		/// </summary>
		public enum VIRTUAL_KEY : ushort
		{
			/// <summary>
			/// A non-existent virtual-key.
			/// </summary>
			None,

			/// <summary>
			/// 0 key
			/// </summary>
			VK_0 = 0x30,

			/// <summary>
			/// 1 key
			/// </summary>
			VK_1 = 0x31,

			/// <summary>
			/// 2 key
			/// </summary>
			VK_2 = 0x32,

			/// <summary>
			/// 3 key
			/// </summary>
			VK_3 = 0x33,

			/// <summary>
			/// 4 key
			/// </summary>
			VK_4 = 0x34,

			/// <summary>
			/// 5 key
			/// </summary>
			VK_5 = 0x35,

			/// <summary>
			/// 6 key
			/// </summary>
			VK_6 = 0x36,

			/// <summary>
			/// 7 key
			/// </summary>
			VK_7 = 0x37,

			/// <summary>
			/// 8 key
			/// </summary>
			VK_8 = 0x38,

			/// <summary>
			/// 9 key
			/// </summary>
			VK_9 = 0x39,

			/// <summary>
			/// A key
			/// </summary>
			VK_A = 0x41,

			/// <summary>
			/// B key
			/// </summary>
			VK_B = 0x42,

			/// <summary>
			/// C key
			/// </summary>
			VK_C = 0x43,

			/// <summary>
			/// D key
			/// </summary>
			VK_D = 0x44,

			/// <summary>
			/// E key
			/// </summary>
			VK_E = 0x45,

			/// <summary>
			/// F key
			/// </summary>
			VK_F = 0x46,

			/// <summary>
			/// G key
			/// </summary>
			VK_G = 0x47,

			/// <summary>
			/// H key
			/// </summary>
			VK_H = 0x48,

			/// <summary>
			/// I key
			/// </summary>
			VK_I = 0x49,

			/// <summary>
			/// J key
			/// </summary>
			VK_J = 0x4A,

			/// <summary>
			/// K key
			/// </summary>
			VK_K = 0x4B,

			/// <summary>
			/// L key
			/// </summary>
			VK_L = 0x4C,

			/// <summary>
			/// M key
			/// </summary>
			VK_M = 0x4D,

			/// <summary>
			/// N key
			/// </summary>
			VK_N = 0x4E,

			/// <summary>
			/// O key
			/// </summary>
			VK_O = 0x4F,

			/// <summary>
			/// P key
			/// </summary>
			VK_P = 0x50,

			/// <summary>
			/// Q key
			/// </summary>
			VK_Q = 0x51,

			/// <summary>
			/// R key
			/// </summary>
			VK_R = 0x52,

			/// <summary>
			/// S key
			/// </summary>
			VK_S = 0x53,

			/// <summary>
			/// T key
			/// </summary>
			VK_T = 0x54,

			/// <summary>
			/// U key
			/// </summary>
			VK_U = 0x55,

			/// <summary>
			/// V key
			/// </summary>
			VK_V = 0x56,

			/// <summary>
			/// W key
			/// </summary>
			VK_W = 0x57,

			/// <summary>
			/// X key
			/// </summary>
			VK_X = 0x58,

			/// <summary>
			/// Y key
			/// </summary>
			VK_Y = 0x59,

			/// <summary>
			/// Z key
			/// </summary>
			VK_Z = 0x5A,

			/// <summary>
			/// Left mouse button
			/// </summary>
			VK_LBUTTON = 0x01,

			/// <summary>
			/// Right mouse button
			/// </summary>
			VK_RBUTTON = 0x02,

			/// <summary>
			/// Control-break processing
			/// </summary>
			VK_CANCEL = 0x03,

			/// <summary>
			/// Middle mouse button (three-button mouse)
			/// </summary>
			VK_MBUTTON = 0x04,

			/// <summary>
			/// X1 mouse button
			/// </summary>
			VK_XBUTTON1 = 0x05,

			/// <summary>
			/// X2 mouse button
			/// </summary>
			VK_XBUTTON2 = 0x06,

			/// <summary>
			/// BACKSPACE key
			/// </summary>
			VK_BACK = 0x08,

			/// <summary>
			/// TAB key
			/// </summary>
			VK_TAB = 0x09,

			/// <summary>
			/// CLEAR key
			/// </summary>
			VK_CLEAR = 0x0C,

			/// <summary>
			/// ENTER key
			/// </summary>
			VK_RETURN = 0x0D,

			/// <summary>
			/// SHIFT key
			/// </summary>
			VK_SHIFT = 0x10,

			/// <summary>
			/// CTRL key
			/// </summary>
			VK_CONTROL = 0x11,

			/// <summary>
			/// ALT key
			/// </summary>
			VK_MENU = 0x12,

			/// <summary>
			/// PAUSE key
			/// </summary>
			VK_PAUSE = 0x13,

			/// <summary>
			/// CAPS LOCK key
			/// </summary>
			VK_CAPITAL = 0x14,

			/// <summary>
			/// IME Kana mode
			/// </summary>
			VK_KANA = 0x15,

			/// <summary>
			/// IME Hanguel mode (maintained for compatibility; use VK_HANGUL)
			/// </summary>
			VK_HANGEUL = 0x15,

			/// <summary>
			/// IME Hangul mode
			/// </summary>
			VK_HANGUL = 0x15,

			/// <summary>
			/// IME On
			/// </summary>
			VK_IME_ON = 0x16,

			/// <summary>
			/// IME Junja mode
			/// </summary>
			VK_JUNJA = 0x17,

			/// <summary>
			/// IME final mode
			/// </summary>
			VK_FINAL = 0x18,

			/// <summary>
			/// IME Hanja mode
			/// </summary>
			VK_HANJA = 0x19,

			/// <summary>
			/// IME Kanji mode
			/// </summary>
			VK_KANJI = 0x19,

			/// <summary>
			/// IME Off
			/// </summary>
			VK_IME_OFF = 0x1A,

			/// <summary>
			/// ESC key
			/// </summary>
			VK_ESCAPE = 0x1B,

			/// <summary>
			/// IME convert
			/// </summary>
			VK_CONVERT = 0x1C,

			/// <summary>
			/// IME nonconvert
			/// </summary>
			VK_NONCONVERT = 0x1D,

			/// <summary>
			/// IME accept
			/// </summary>
			VK_ACCEPT = 0x1E,

			/// <summary>
			/// IME mode change request
			/// </summary>
			VK_MODECHANGE = 0x1F,

			/// <summary>
			/// SPACEBAR
			/// </summary>
			VK_SPACE = 0x20,

			/// <summary>
			/// PAGE UP key
			/// </summary>
			VK_PRIOR = 0x21,

			/// <summary>
			/// PAGE DOWN key
			/// </summary>
			VK_NEXT = 0x22,

			/// <summary>
			/// END key
			/// </summary>
			VK_END = 0x23,

			/// <summary>
			/// HOME key
			/// </summary>
			VK_HOME = 0x24,

			/// <summary>
			/// LEFT ARROW key
			/// </summary>
			VK_LEFT = 0x25,

			/// <summary>
			/// UP ARROW key
			/// </summary>
			VK_UP = 0x26,

			/// <summary>
			/// RIGHT ARROW key
			/// </summary>
			VK_RIGHT = 0x27,

			/// <summary>
			/// DOWN ARROW key
			/// </summary>
			VK_DOWN = 0x28,

			/// <summary>
			/// SELECT key
			/// </summary>
			VK_SELECT = 0x29,

			/// <summary>
			/// PRINT key
			/// </summary>
			VK_PRINT = 0x2A,

			/// <summary>
			/// EXECUTE key
			/// </summary>
			VK_EXECUTE = 0x2B,

			/// <summary>
			/// PRINT SCREEN key
			/// </summary>
			VK_SNAPSHOT = 0x2C,

			/// <summary>
			/// INS key
			/// </summary>
			VK_INSERT = 0x2D,

			/// <summary>
			/// DEL key
			/// </summary>
			VK_DELETE = 0x2E,

			/// <summary>
			/// HELP key
			/// </summary>
			VK_HELP = 0x2F,

			/// <summary>
			/// Left Windows key (Natural keyboard)
			/// </summary>
			VK_LWIN = 0x5B,

			/// <summary>
			/// Right Windows key (Natural keyboard)
			/// </summary>
			VK_RWIN = 0x5C,

			/// <summary>
			/// Applications key (Natural keyboard)
			/// </summary>
			VK_APPS = 0x5D,

			/// <summary>
			/// Computer Sleep key
			/// </summary>
			VK_SLEEP = 0x5F,

			/// <summary>
			/// Numeric keypad 0 key
			/// </summary>
			VK_NUMPAD0 = 0x60,

			/// <summary>
			/// Numeric keypad 1 key
			/// </summary>
			VK_NUMPAD1 = 0x61,

			/// <summary>
			/// Numeric keypad 2 key
			/// </summary>
			VK_NUMPAD2 = 0x62,

			/// <summary>
			/// Numeric keypad 3 key
			/// </summary>
			VK_NUMPAD3 = 0x63,

			/// <summary>
			/// Numeric keypad 4 key
			/// </summary>
			VK_NUMPAD4 = 0x64,

			/// <summary>
			/// Numeric keypad 5 key
			/// </summary>
			VK_NUMPAD5 = 0x65,

			/// <summary>
			/// Numeric keypad 6 key
			/// </summary>
			VK_NUMPAD6 = 0x66,

			/// <summary>
			/// Numeric keypad 7 key
			/// </summary>
			VK_NUMPAD7 = 0x67,

			/// <summary>
			/// Numeric keypad 8 key
			/// </summary>
			VK_NUMPAD8 = 0x68,

			/// <summary>
			/// Numeric keypad 9 key
			/// </summary>
			VK_NUMPAD9 = 0x69,

			/// <summary>
			/// Multiply key
			/// </summary>
			VK_MULTIPLY = 0x6A,

			/// <summary>
			/// Add key
			/// </summary>
			VK_ADD = 0x6B,

			/// <summary>
			/// Separator key
			/// </summary>
			VK_SEPARATOR = 0x6C,

			/// <summary>
			/// Subtract key
			/// </summary>
			VK_SUBTRACT = 0x6D,

			/// <summary>
			/// Decimal key
			/// </summary>
			VK_DECIMAL = 0x6E,

			/// <summary>
			/// Divide key
			/// </summary>
			VK_DIVIDE = 0x6F,

			/// <summary>
			/// F1 key
			/// </summary>
			VK_F1 = 0x70,

			/// <summary>
			/// F2 key
			/// </summary>
			VK_F2 = 0x71,

			/// <summary>
			/// F3 key
			/// </summary>
			VK_F3 = 0x72,

			/// <summary>
			/// F4 key
			/// </summary>
			VK_F4 = 0x73,

			/// <summary>
			/// F5 key
			/// </summary>
			VK_F5 = 0x74,

			/// <summary>
			/// F6 key
			/// </summary>
			VK_F6 = 0x75,

			/// <summary>
			/// F7 key
			/// </summary>
			VK_F7 = 0x76,

			/// <summary>
			/// F8 key
			/// </summary>
			VK_F8 = 0x77,

			/// <summary>
			/// F9 key
			/// </summary>
			VK_F9 = 0x78,

			/// <summary>
			/// F10 key
			/// </summary>
			VK_F10 = 0x79,

			/// <summary>
			/// F11 key
			/// </summary>
			VK_F11 = 0x7A,

			/// <summary>
			/// F12 key
			/// </summary>
			VK_F12 = 0x7B,

			/// <summary>
			/// F13 key
			/// </summary>
			VK_F13 = 0x7C,

			/// <summary>
			/// F14 key
			/// </summary>
			VK_F14 = 0x7D,

			/// <summary>
			/// F15 key
			/// </summary>
			VK_F15 = 0x7E,

			/// <summary>
			/// F16 key
			/// </summary>
			VK_F16 = 0x7F,

			/// <summary>
			/// F17 key
			/// </summary>
			VK_F17 = 0x80,

			/// <summary>
			/// F18 key
			/// </summary>
			VK_F18 = 0x81,

			/// <summary>
			/// F19 key
			/// </summary>
			VK_F19 = 0x82,

			/// <summary>
			/// F20 key
			/// </summary>
			VK_F20 = 0x83,

			/// <summary>
			/// F21 key
			/// </summary>
			VK_F21 = 0x84,

			/// <summary>
			/// F22 key
			/// </summary>
			VK_F22 = 0x85,

			/// <summary>
			/// F23 key
			/// </summary>
			VK_F23 = 0x86,

			/// <summary>
			/// F24 key
			/// </summary>
			VK_F24 = 0x87,

			VK_NAVIGATION_VIEW = 0x88,
			VK_NAVIGATION_MENU = 0x89,
			VK_NAVIGATION_UP = 0x8A,
			VK_NAVIGATION_DOWN = 0x8B,
			VK_NAVIGATION_LEFT = 0x8C,
			VK_NAVIGATION_RIGHT = 0x8D,
			VK_NAVIGATION_ACCEPT = 0x8E,
			VK_NAVIGATION_CANCEL = 0x8F,

			/// <summary>
			/// NUM LOCK key
			/// </summary>
			VK_NUMLOCK = 0x90,

			/// <summary>
			/// SCROLL LOCK key
			/// </summary>
			VK_SCROLL = 0x91,
			VK_OEM_NEC_EQUAL = 0x92,
			VK_OEM_FJ_JISHO = 0x92,
			VK_OEM_FJ_MASSHOU = 0x93,
			VK_OEM_FJ_TOUROKU = 0x94,
			VK_OEM_FJ_LOYA = 0x95,
			VK_OEM_FJ_ROYA = 0x96,

			/// <summary>
			/// Left SHIFT key
			/// </summary>
			VK_LSHIFT = 0xA0,

			/// <summary>
			/// Right SHIFT key
			/// </summary>
			VK_RSHIFT = 0xA1,

			/// <summary>
			/// Left CONTROL key
			/// </summary>
			VK_LCONTROL = 0xA2,

			/// <summary>
			/// Right CONTROL key
			/// </summary>
			VK_RCONTROL = 0xA3,

			/// <summary>
			/// Left ALT key
			/// </summary>
			VK_LMENU = 0xA4,

			/// <summary>
			/// Right ALT key
			/// </summary>
			VK_RMENU = 0xA5,

			/// <summary>
			/// Browser Back key
			/// </summary>
			VK_BROWSER_BACK = 0xA6,

			/// <summary>
			/// Browser Forward key
			/// </summary>
			VK_BROWSER_FORWARD = 0xA7,

			/// <summary>
			/// Browser Refresh key
			/// </summary>
			VK_BROWSER_REFRESH = 0xA8,

			/// <summary>
			/// Browser Stop key
			/// </summary>
			VK_BROWSER_STOP = 0xA9,

			/// <summary>
			/// Browser Search key
			/// </summary>
			VK_BROWSER_SEARCH = 0xAA,

			/// <summary>
			/// Browser Favorites key
			/// </summary>
			VK_BROWSER_FAVORITES = 0xAB,

			/// <summary>
			/// Browser Start and Home key
			/// </summary>
			VK_BROWSER_HOME = 0xAC,

			/// <summary>
			/// Volume Mute key
			/// </summary>
			VK_VOLUME_MUTE = 0xAD,

			/// <summary>
			/// Volume Down key
			/// </summary>
			VK_VOLUME_DOWN = 0xAE,

			/// <summary>
			/// Volume Up key
			/// </summary>
			VK_VOLUME_UP = 0xAF,

			/// <summary>
			/// Next Track key
			/// </summary>
			VK_MEDIA_NEXT_TRACK = 0xB0,

			/// <summary>
			/// Previous Track key
			/// </summary>
			VK_MEDIA_PREV_TRACK = 0xB1,

			/// <summary>
			/// Stop Media key
			/// </summary>
			VK_MEDIA_STOP = 0xB2,

			/// <summary>
			/// Play/Pause Media key
			/// </summary>
			VK_MEDIA_PLAY_PAUSE = 0xB3,

			/// <summary>
			/// Start Mail key
			/// </summary>
			VK_LAUNCH_MAIL = 0xB4,

			/// <summary>
			/// Select Media key
			/// </summary>
			VK_LAUNCH_MEDIA_SELECT = 0xB5,

			/// <summary>
			/// Start Application 1 key
			/// </summary>
			VK_LAUNCH_APP1 = 0xB6,

			/// <summary>
			/// Start Application 2 key
			/// </summary>
			VK_LAUNCH_APP2 = 0xB7,
			VK_OEM_1 = 0xBA,
			VK_OEM_PLUS = 0xBB,
			VK_OEM_COMMA = 0xBC,
			VK_OEM_MINUS = 0xBD,
			VK_OEM_PERIOD = 0xBE,
			VK_OEM_2 = 0xBF,
			VK_OEM_3 = 0xC0,
			VK_GAMEPAD_A = 0xC3,
			VK_GAMEPAD_B = 0xC4,
			VK_GAMEPAD_X = 0xC5,
			VK_GAMEPAD_Y = 0xC6,
			VK_GAMEPAD_RIGHT_SHOULDER = 0xC7,
			VK_GAMEPAD_LEFT_SHOULDER = 0xC8,
			VK_GAMEPAD_LEFT_TRIGGER = 0xC9,
			VK_GAMEPAD_RIGHT_TRIGGER = 0xCA,
			VK_GAMEPAD_DPAD_UP = 0xCB,
			VK_GAMEPAD_DPAD_DOWN = 0xCC,
			VK_GAMEPAD_DPAD_LEFT = 0xCD,
			VK_GAMEPAD_DPAD_RIGHT = 0xCE,
			VK_GAMEPAD_MENU = 0xCF,
			VK_GAMEPAD_VIEW = 0xD0,
			VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON = 0xD1,
			VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON = 0xD2,
			VK_GAMEPAD_LEFT_THUMBSTICK_UP = 0xD3,
			VK_GAMEPAD_LEFT_THUMBSTICK_DOWN = 0xD4,
			VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT = 0xD5,
			VK_GAMEPAD_LEFT_THUMBSTICK_LEFT = 0xD6,
			VK_GAMEPAD_RIGHT_THUMBSTICK_UP = 0xD7,
			VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN = 0xD8,
			VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT = 0xD9,
			VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT = 0xDA,
			VK_OEM_4 = 0xDB,
			VK_OEM_5 = 0xDC,
			VK_OEM_6 = 0xDD,
			VK_OEM_7 = 0xDE,
			VK_OEM_8 = 0xDF,
			VK_OEM_AX = 0xE1,
			VK_OEM_102 = 0xE2,
			VK_ICO_HELP = 0xE3,
			VK_ICO_00 = 0xE4,
			VK_PROCESSKEY = 0xE5,
			VK_ICO_CLEAR = 0xE6,
			VK_PACKET = 0xE7,
			VK_OEM_RESET = 0xE9,
			VK_OEM_JUMP = 0xEA,
			VK_OEM_PA1 = 0xEB,
			VK_OEM_PA2 = 0xEC,
			VK_OEM_PA3 = 0xED,
			VK_OEM_WSCTRL = 0xEE,
			VK_OEM_CUSEL = 0xEF,
			VK_OEM_ATTN = 0xF0,
			VK_OEM_FINISH = 0xF1,
			VK_OEM_COPY = 0xF2,
			VK_OEM_AUTO = 0xF3,
			VK_OEM_ENLW = 0xF4,
			VK_OEM_BACKTAB = 0xF5,

			/// <summary>
			/// Attn key
			/// </summary>
			VK_ATTN = 0xF6,

			/// <summary>
			/// CrSel key
			/// </summary>
			VK_CRSEL = 0xF7,

			/// <summary>
			/// ExSel key
			/// </summary>
			VK_EXSEL = 0xF8,

			/// <summary>
			/// Erase EOF key
			/// </summary>
			VK_EREOF = 0xF9,

			/// <summary>
			/// Play key
			/// </summary>
			VK_PLAY = 0xFA,

			/// <summary>
			/// Zoom key
			/// </summary>
			VK_ZOOM = 0xFB,

			/// <summary>
			/// Reserved
			/// </summary>
			VK_NONAME = 0xFC,

			/// <summary>
			/// PA1 key
			/// </summary>
			VK_PA1 = 0xFD,

			/// <summary>
			/// Clear key
			/// </summary>
			VK_OEM_CLEAR = 0xFE,
		}
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
