using Moq;
using Xunit;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class KeybindHookTests
{
	private class FakeSafeHandle : UnhookWindowsHookExSafeHandle
	{
		public bool HasDisposed { get; private set; }

		private readonly bool _isInvalid;
		public override bool IsInvalid => _isInvalid;

		public FakeSafeHandle(bool isInvalid)
			: base(default, default)
		{
			_isInvalid = isInvalid;
		}

		protected override bool ReleaseHandle()
		{
			return true;
		}

		protected override void Dispose(bool disposing)
		{
			HasDisposed = true;
			base.Dispose(disposing);
		}
	}

	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<ICoreNativeManager> CoreNativeManager { get; } = new();
		public Mock<IKeybindManager> KeybindManager { get; } = new();
		public HOOKPROC? KeyboardHook { get; private set; }
		public FakeSafeHandle Handle { get; }

		public Wrapper(bool isHandleInvalid = false)
		{
			Handle = new FakeSafeHandle(isHandleInvalid);

			Context.SetupGet(context => context.KeybindManager).Returns(KeybindManager.Object);

			CoreNativeManager
				.Setup(cnm => cnm.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, It.IsAny<HOOKPROC>(), null, 0))
				.Callback(
					(WINDOWS_HOOK_ID id, HOOKPROC proc, SafeHandle handle, uint dwThreadId) =>
					{
						KeyboardHook = proc;
					}
				)
				.Returns(Handle);

			CoreNativeManager.Setup(cnm => cnm.CallNextHookEx(0, 0, 0)).Returns((LRESULT)0);
		}

		public void SetupKey(VIRTUAL_KEY[] modifiers, VIRTUAL_KEY key, ICommand[] commands)
		{
			foreach (VIRTUAL_KEY modifier in modifiers)
			{
				CoreNativeManager.Setup(cnm => cnm.GetKeyState((int)modifier)).Returns(-32768);
			}

			CoreNativeManager
				.Setup(coreNativeManager => coreNativeManager.PtrToStructure<KBDLLHOOKSTRUCT>(It.IsAny<nint>()))
				.Returns(new KBDLLHOOKSTRUCT { vkCode = (uint)key });

			KeybindManager.Setup(keybindManager => keybindManager.GetCommands(It.IsAny<IKeybind>())).Returns(commands);
		}
	}

	[Fact]
	public void PostInitialize()
	{
		// Given
		Mock<IContext> context = new();
		Mock<ICoreNativeManager> coreNativeManager = new();
		KeybindHook keybindHook = new(context.Object, coreNativeManager.Object);

		// When
		keybindHook.PostInitialize();

		// Then
		coreNativeManager.Verify(
			c => c.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, It.IsAny<HOOKPROC>(), null, 0),
			Times.Once
		);
	}

	[InlineData(1)]
	[InlineData(-1)]
	[Theory]
	public void KeyboardHook_InvalidNCode(int nCode)
	{
		// Given
		Wrapper wrapper = new();
		KeybindHook keybindHook = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		keybindHook.PostInitialize();
		wrapper.KeyboardHook?.Invoke(nCode, 0, 0);

		// Then
		wrapper.CoreNativeManager.Verify(c => c.PtrToStructure<KBDLLHOOKSTRUCT>(It.IsAny<nint>()), Times.Never);
		wrapper.CoreNativeManager.Verify(c => c.CallNextHookEx(nCode, 0, 0), Times.Once);
	}

	// WM_KEYDOWN and WM_SYSKEYDOWN
	[InlineData(0x0099)]
	[InlineData(0x0101)]
	[InlineData(0x0103)]
	[InlineData(0x0105)]
	[Theory]
	public void KeyboardHook_ValidNCodeButInvalidWParam(uint wParam)
	{
		// Given
		Wrapper wrapper = new();
		KeybindHook keybindHook = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		keybindHook.PostInitialize();
		wrapper.KeyboardHook?.Invoke(0, wParam, 0);

		// Then
		wrapper.CoreNativeManager.Verify(c => c.PtrToStructure<KBDLLHOOKSTRUCT>(It.IsAny<nint>()), Times.Never);
		wrapper.CoreNativeManager.Verify(
			coreNativeManager => coreNativeManager.CallNextHookEx(0, wParam, 0),
			Times.Once
		);
	}

	public static readonly IEnumerable<object[]> IgnoredKeys = new List<object[]>()
	{
		new object[] { VIRTUAL_KEY.VK_LSHIFT },
		new object[] { VIRTUAL_KEY.VK_RSHIFT },
		new object[] { VIRTUAL_KEY.VK_LMENU },
		new object[] { VIRTUAL_KEY.VK_RMENU },
		new object[] { VIRTUAL_KEY.VK_LCONTROL },
		new object[] { VIRTUAL_KEY.VK_RCONTROL },
		new object[] { VIRTUAL_KEY.VK_LWIN },
		new object[] { VIRTUAL_KEY.VK_RWIN }
	};

	[MemberData(nameof(IgnoredKeys))]
	[Theory]
	public void KeyboardHook_IgnoredKey(VIRTUAL_KEY key)
	{
		// Given
		Wrapper wrapper = new();
		KeybindHook keybindHook = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);
		wrapper.SetupKey(new VIRTUAL_KEY[] { key }, VIRTUAL_KEY.None, Array.Empty<ICommand>());

		// When
		keybindHook.PostInitialize();
		wrapper.KeyboardHook?.Invoke(0, PInvoke.WM_KEYDOWN, 0);

		// Then
		wrapper.CoreNativeManager.Verify(
			coreNativeManager => coreNativeManager.CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0),
			Times.Once
		);
	}

	public static readonly IEnumerable<object[]> KeybindsToExecute = new List<object[]>()
	{
		new object[]
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_LSHIFT },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.LShift, VIRTUAL_KEY.VK_A)
		},
		new object[]
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_RSHIFT },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.RShift, VIRTUAL_KEY.VK_A)
		},
		new object[]
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_LMENU, VIRTUAL_KEY.VK_LCONTROL },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_A)
		},
		new object[]
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_RMENU, VIRTUAL_KEY.VK_RCONTROL },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.RAlt | KeyModifiers.RControl, VIRTUAL_KEY.VK_A)
		},
		new object[]
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_LWIN },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_A)
		},
		new object[]
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_RWIN },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.RWin, VIRTUAL_KEY.VK_A)
		},
	};

	[MemberData(nameof(KeybindsToExecute))]
	[Theory]
	public void KeyboardHook_ValidKeybind(VIRTUAL_KEY[] modifiers, VIRTUAL_KEY key, Keybind keybind)
	{
		// Given
		Wrapper wrapper = new();
		KeybindHook keybindHook = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);
		Mock<ICommand>[] commands = Enumerable.Range(0, 3).Select(_ => new Mock<ICommand>()).ToArray();
		wrapper.SetupKey(modifiers, key, commands.Select(c => c.Object).ToArray());

		// When
		keybindHook.PostInitialize();
		LRESULT? result = wrapper.KeyboardHook?.Invoke(0, PInvoke.WM_SYSKEYDOWN, 0);

		// Then
		wrapper.CoreNativeManager.Verify(
			coreNativeManager => coreNativeManager.CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0),
			Times.Never
		);
		Assert.Equal(1, (nint)result!);
		wrapper.KeybindManager.Verify(km => km.GetCommands(keybind), Times.Once);
		foreach (Mock<ICommand> command in commands)
		{
			command.Verify(c => c.TryExecute(), Times.Once);
		}
	}

	[Fact]
	public void KeyboardHook_NoModifiers()
	{
		// Given
		Wrapper wrapper = new();
		KeybindHook keybindHook = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);
		wrapper.SetupKey(Array.Empty<VIRTUAL_KEY>(), VIRTUAL_KEY.VK_A, Array.Empty<ICommand>());

		// When
		keybindHook.PostInitialize();
		LRESULT? result = wrapper.KeyboardHook?.Invoke(0, PInvoke.WM_KEYDOWN, 0);

		// Then
		wrapper.CoreNativeManager.Verify(
			coreNativeManager => coreNativeManager.CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0),
			Times.Once
		);
		Assert.Equal(0, (nint)result!);
		wrapper.KeybindManager.Verify(
			km => km.GetCommands(new Keybind(KeyModifiers.None, VIRTUAL_KEY.VK_A)),
			Times.Never
		);
	}

	[Fact]
	public void KeyboardHook_NoCommands()
	{
		// Given
		Wrapper wrapper = new();
		KeybindHook keybindHook = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);
		wrapper.SetupKey(new[] { VIRTUAL_KEY.VK_LWIN }, VIRTUAL_KEY.VK_U, Array.Empty<ICommand>());

		// When
		keybindHook.PostInitialize();
		LRESULT? result = wrapper.KeyboardHook?.Invoke(0, PInvoke.WM_KEYDOWN, 0);

		// Then
		wrapper.CoreNativeManager.Verify(
			coreNativeManager => coreNativeManager.CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0),
			Times.Once
		);
		Assert.Equal(0, (nint)result!);
		wrapper.KeybindManager.Verify(
			km => km.GetCommands(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_U)),
			Times.Once
		);
	}

	[Fact]
	public void Dispose_HookIsNull()
	{
		// Given
		Wrapper wrapper = new();
		KeybindHook keybindHook = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// When
		keybindHook.Dispose();

		// Then
		Assert.False(wrapper.Handle.HasDisposed);
	}

	[Fact]
	public void Dispose_HookIsInvalid()
	{
		// Given
		Wrapper wrapper = new(true);
		KeybindHook keybindHook = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);
		wrapper.SetupKey(Array.Empty<VIRTUAL_KEY>(), VIRTUAL_KEY.VK_A, Array.Empty<ICommand>());

		// When
		keybindHook.PostInitialize();
		keybindHook.Dispose();

		// Then
		Assert.False(wrapper.Handle.HasDisposed);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();
		KeybindHook keybindHook = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);
		wrapper.SetupKey(Array.Empty<VIRTUAL_KEY>(), VIRTUAL_KEY.VK_A, Array.Empty<ICommand>());

		// When
		keybindHook.PostInitialize();
		keybindHook.Dispose();

		// Then
		Assert.True(wrapper.Handle.HasDisposed);
	}
}
