using System.Linq;
using AutoFixture;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim.Tests;

public class KeybindHookCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();
		internalCtx.CoreNativeManager.CallNextHookEx(0, 0, 0).Returns((LRESULT)0);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class KeybindHookTests
{
	private class FakeSafeHandle(bool isInvalid) : UnhookWindowsHookExSafeHandle(default, default)
	{
		public bool HasDisposed { get; private set; }

		private readonly bool _isInvalid = isInvalid;
		public override bool IsInvalid => _isInvalid;

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

	private class CaptureKeybindHook
	{
		public HOOKPROC? LowLevelKeyboardProc { get; private set; }
		public FakeSafeHandle? Handle { get; private set; }

		public static CaptureKeybindHook Create(IInternalContext internalCtx)
		{
			CaptureKeybindHook captureKeybindHook = new();
			internalCtx
				.CoreNativeManager.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, Arg.Any<HOOKPROC>(), null, 0)
				.Returns(
					(callInfo) =>
					{
						captureKeybindHook.LowLevelKeyboardProc = callInfo.ArgAt<HOOKPROC>(1);
						captureKeybindHook.Handle = new FakeSafeHandle(false);
						return captureKeybindHook.Handle;
					}
				);

			return captureKeybindHook;
		}
	}

	private static void SetupKey(
		IContext ctx,
		IInternalContext internalCtx,
		VIRTUAL_KEY[] modifiers,
		VIRTUAL_KEY key,
		ICommand[] commands
	)
	{
		foreach (VIRTUAL_KEY modifier in modifiers)
		{
			internalCtx.CoreNativeManager.GetKeyState((int)modifier).Returns((short)-32768);
		}

		internalCtx
			.CoreNativeManager.PtrToStructure<KBDLLHOOKSTRUCT>(Arg.Any<nint>())
			.Returns(new KBDLLHOOKSTRUCT { vkCode = (uint)key });

		ctx.KeybindManager.GetCommands(Arg.Any<IKeybind>()).Returns(commands);
		ctx.KeybindManager.Modifiers.Returns(modifiers);
	}

	[Theory, AutoSubstituteData<KeybindHookCustomization>]
	internal void PostInitialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);

		// When
		keybindHook.PostInitialize();

		// Then
		internalCtx
			.CoreNativeManager.Received(1)
			.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, Arg.Any<HOOKPROC>(), null, 0);
	}

	[InlineAutoSubstituteData<KeybindHookCustomization>(1)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(-1)]
	[Theory]
	internal void LowLevelKeyboardProc_InvalidNCode(int nCode, IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);

		// When
		keybindHook.PostInitialize();
		capture.LowLevelKeyboardProc?.Invoke(nCode, 0, 0);

		// Then
		internalCtx.CoreNativeManager.DidNotReceive().PtrToStructure<KBDLLHOOKSTRUCT>(Arg.Any<nint>());
		internalCtx.CoreNativeManager.Received(1).CallNextHookEx(nCode, 0, 0);
	}

	[Theory, AutoSubstituteData<KeybindHookCustomization>]
	internal void LowLevelKeyboardProc_NotPtrToStructure(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);
		internalCtx
			.CoreNativeManager.PtrToStructure<KBDLLHOOKSTRUCT>(Arg.Any<nint>())
			.Returns(null as KBDLLHOOKSTRUCT?);

		// When
		keybindHook.PostInitialize();
		capture.LowLevelKeyboardProc?.Invoke(0, PInvoke.WM_KEYDOWN, 0);

		// Then
		internalCtx.CoreNativeManager.Received(1).PtrToStructure<KBDLLHOOKSTRUCT>(Arg.Any<nint>());
		internalCtx.CoreNativeManager.Received(1).CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0);
	}

	// WM_KEYDOWN and WM_SYSKEYDOWN
	[InlineAutoSubstituteData<KeybindHookCustomization>(0x0099)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(0x0101)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(0x0103)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(0x0105)]
	[Theory]
	internal void LowLevelKeyboardProc_ValidNCodeButInvalidWParam(
		uint wParam,
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);

		// When
		keybindHook.PostInitialize();
		capture.LowLevelKeyboardProc?.Invoke(0, wParam, 0);

		// Then
		internalCtx.CoreNativeManager.DidNotReceive().PtrToStructure<KBDLLHOOKSTRUCT>(Arg.Any<nint>());
		internalCtx.CoreNativeManager.Received(1).CallNextHookEx(0, wParam, 0);
	}

	[InlineAutoSubstituteData<KeybindHookCustomization>(VIRTUAL_KEY.VK_LSHIFT)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(VIRTUAL_KEY.VK_RSHIFT)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(VIRTUAL_KEY.VK_LMENU)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(VIRTUAL_KEY.VK_RMENU)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(VIRTUAL_KEY.VK_LCONTROL)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(VIRTUAL_KEY.VK_RCONTROL)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(VIRTUAL_KEY.VK_LWIN)]
	[InlineAutoSubstituteData<KeybindHookCustomization>(VIRTUAL_KEY.VK_RWIN)]
	[Theory]
	internal void LowLevelKeyboardProc_IgnoredKey(VIRTUAL_KEY key, IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);
		SetupKey(ctx, internalCtx, [key], VIRTUAL_KEY.None, []);

		// When
		keybindHook.PostInitialize();
		capture.LowLevelKeyboardProc?.Invoke(0, PInvoke.WM_KEYDOWN, 0);

		// Then
		internalCtx.CoreNativeManager.Received(1).CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0);
	}

	public static readonly TheoryData<VIRTUAL_KEY[], VIRTUAL_KEY, Keybind> KeybindsToExecute = new()
	{
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_LSHIFT },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.LShift, VIRTUAL_KEY.VK_A)
		},
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_RSHIFT },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.RShift, VIRTUAL_KEY.VK_A)
		},
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_LMENU, VIRTUAL_KEY.VK_LCONTROL },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_A)
		},
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_RMENU, VIRTUAL_KEY.VK_RCONTROL },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.RAlt | KeyModifiers.RControl, VIRTUAL_KEY.VK_A)
		},
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_LWIN },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_A)
		},
		{
			new VIRTUAL_KEY[] { VIRTUAL_KEY.VK_RWIN },
			VIRTUAL_KEY.VK_A,
			new Keybind(KeyModifiers.RWin, VIRTUAL_KEY.VK_A)
		},
	};

	[MemberData(nameof(KeybindsToExecute))]
	[Theory]
	internal void LowLevelKeyboardProc_ValidKeybind(VIRTUAL_KEY[] modifiers, VIRTUAL_KEY key, Keybind keybind)
	{
		// Given
		IContext ctx = Substitute.For<IContext>();
		IInternalContext internalCtx = Substitute.For<IInternalContext>();

		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);
		ICommand[] commands = Enumerable.Range(0, 3).Select(_ => Substitute.For<ICommand>()).ToArray();
		SetupKey(ctx, internalCtx, modifiers, key, commands.Select(c => c).ToArray());

		// When
		keybindHook.PostInitialize();
		LRESULT? result = capture.LowLevelKeyboardProc?.Invoke(0, PInvoke.WM_SYSKEYDOWN, 0);

		// Then
		internalCtx.CoreNativeManager.DidNotReceive().CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0);
		Assert.Equal(1, (nint)result!);
		ctx.KeybindManager.Received(1).GetCommands(keybind);
		foreach (ICommand command in commands)
		{
			command.Received(1).TryExecute();
		}
	}

	[Theory, AutoSubstituteData<KeybindHookCustomization>]
	internal void LowLevelKeyboardProc_NoModifiers(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);
		SetupKey(ctx, internalCtx, [], VIRTUAL_KEY.VK_A, []);

		// When
		keybindHook.PostInitialize();
		LRESULT? result = capture.LowLevelKeyboardProc?.Invoke(0, PInvoke.WM_KEYDOWN, 0);

		// Then
		internalCtx.CoreNativeManager.Received(1).CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0);
		Assert.Equal(0, (nint)result!);
		ctx.KeybindManager.Received(1).GetCommands(new Keybind(KeyModifiers.None, VIRTUAL_KEY.VK_A));
	}

	[Theory, AutoSubstituteData<KeybindHookCustomization>]
	internal void LowLevelKeyboardProc_NoCommands(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);
		SetupKey(ctx, internalCtx, [VIRTUAL_KEY.VK_LWIN], VIRTUAL_KEY.VK_U, []);

		// When
		keybindHook.PostInitialize();
		LRESULT? result = capture.LowLevelKeyboardProc?.Invoke(0, PInvoke.WM_KEYDOWN, 0);

		// Then
		internalCtx.CoreNativeManager.Received(1).CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0);
		Assert.Equal(0, (nint)result!);
		ctx.KeybindManager.Received(1).GetCommands(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_U));
	}

	[Theory, AutoSubstituteData<KeybindHookCustomization>]
	internal void LowLevelKeyboardProc_HandleException(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);

		// When
		keybindHook.PostInitialize();
		internalCtx
			.CoreNativeManager.PtrToStructure<KBDLLHOOKSTRUCT>(Arg.Any<nint>())
			.Returns(_ => throw new Exception());
		LRESULT? result = capture.LowLevelKeyboardProc?.Invoke(0, PInvoke.WM_SYSKEYDOWN, 0);

		// Then
		internalCtx.CoreNativeManager.DidNotReceive().CallNextHookEx(0, PInvoke.WM_KEYDOWN, 0);
	}

	[Theory, AutoSubstituteData<KeybindHookCustomization>]
	internal void Dispose_HookIsNull(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);

		// When
		keybindHook.Dispose();

		// Then
		Assert.Null(capture.Handle);
	}

	[Theory, AutoSubstituteData<KeybindHookCustomization>]
	internal void Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		CaptureKeybindHook capture = CaptureKeybindHook.Create(internalCtx);
		KeybindHook keybindHook = new(ctx, internalCtx);
		SetupKey(ctx, internalCtx, [], VIRTUAL_KEY.VK_A, []);

		// When
		keybindHook.PostInitialize();
		keybindHook.Dispose();

		// Then
		Assert.True(capture.Handle?.HasDisposed);
	}
}
