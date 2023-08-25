using Moq;
using System.Runtime.InteropServices;
using Whim.TestUtils;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

public class MouseHookTests
{
	private class WindowsHookHandle : UnhookWindowsHookExSafeHandle
	{
		public int Calls { get; private set; }

		protected override void Dispose(bool disposing)
		{
			Calls++;
			base.Dispose(disposing);
		}
	}

	private class Wrapper
	{
		public Mock<ICoreNativeManager> CoreNativeManager { get; } = new();
		public HOOKPROC? HookProc { get; private set; }
		public WindowsHookHandle SafeHandle { get; } = new();

		public Wrapper()
		{
			CoreNativeManager
				.Setup(cnm => cnm.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_MOUSE_LL, It.IsAny<HOOKPROC>(), null, 0))
				.Callback(
					(WINDOWS_HOOK_ID id, HOOKPROC proc, SafeHandle handle, uint dwThreadId) =>
					{
						HookProc = proc;
					}
				)
				.Returns(SafeHandle);

			CoreNativeManager.Setup(cnm => cnm.CallNextHookEx(It.IsAny<int>(), It.IsAny<WPARAM>(), It.IsAny<LPARAM>()));
		}

		public Wrapper PtrToStructure(MSLLHOOKSTRUCT? msllhookstruct)
		{
			CoreNativeManager
				.Setup(cnm => cnm.PtrToStructure<MSLLHOOKSTRUCT>(It.IsAny<nint>()))
				.Returns(msllhookstruct);

			return this;
		}
	}

	[Fact]
	public void PostInitialize()
	{
		// Given
		Wrapper wrapper = new();
		using MouseHook mouseHook = new(wrapper.CoreNativeManager.Object);

		// When
		mouseHook.PostInitialize();

		// Then
		Assert.NotNull(wrapper.HookProc);
	}

	[Fact]
	public void OnMouseTriggerEvent_LParamIsZero()
	{
		// Given
		Wrapper wrapper = new();
		using MouseHook mouseHook = new(wrapper.CoreNativeManager.Object);

		// When
		mouseHook.PostInitialize();

		// Then
		WhimAssert.DoesNotRaise<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonDown += h,
			h => mouseHook.MouseLeftButtonDown -= h,
			() => wrapper.HookProc!.Invoke(0, (WPARAM)PInvoke.WM_LBUTTONDOWN, 0)
		);
	}

	[Fact]
	public void OnMouseTriggerEvent_NotMSLLHOOKSTRUCT()
	{
		// Given
		Wrapper wrapper = new Wrapper().PtrToStructure(null);
		using MouseHook mouseHook = new(wrapper.CoreNativeManager.Object);

		// When
		mouseHook.PostInitialize();

		// Then
		WhimAssert.DoesNotRaise<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonDown += h,
			h => mouseHook.MouseLeftButtonDown -= h,
			() => wrapper.HookProc!.Invoke(0, (WPARAM)PInvoke.WM_LBUTTONDOWN, 1)
		);
	}

	[Fact]
	public void OnMouseTriggerEvent_Success_WM_LBUTTONDOWN()
	{
		// Given
		System.Drawing.Point point = new(1, 2);
		MSLLHOOKSTRUCT msllhookstruct = new() { pt = point };
		Wrapper wrapper = new Wrapper().PtrToStructure(msllhookstruct);
		using MouseHook mouseHook = new(wrapper.CoreNativeManager.Object);

		// When
		mouseHook.PostInitialize();

		// Then
		var result = Assert.Raises<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonDown += h,
			h => mouseHook.MouseLeftButtonDown -= h,
			() => wrapper.HookProc!.Invoke(0, (WPARAM)PInvoke.WM_LBUTTONDOWN, 1)
		);
		Assert.Equal(point.X, result.Arguments.Point.X);
		Assert.Equal(point.Y, result.Arguments.Point.Y);
	}

	[Fact]
	public void OnMouseTriggerEvent_Success_WM_LBUTTONUP()
	{
		// Given
		System.Drawing.Point point = new(1, 2);
		MSLLHOOKSTRUCT msllhookstruct = new() { pt = point };
		Wrapper wrapper = new Wrapper().PtrToStructure(msllhookstruct);
		using MouseHook mouseHook = new(wrapper.CoreNativeManager.Object);

		// When
		mouseHook.PostInitialize();

		// Then
		var result = Assert.Raises<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonUp += h,
			h => mouseHook.MouseLeftButtonUp -= h,
			() => wrapper.HookProc!.Invoke(0, (WPARAM)PInvoke.WM_LBUTTONUP, 1)
		);
		Assert.Equal(point.X, result.Arguments.Point.X);
		Assert.Equal(point.Y, result.Arguments.Point.Y);
	}

	[Fact]
	public void OtherKey()
	{
		// Given
		Wrapper wrapper = new Wrapper().PtrToStructure(new MSLLHOOKSTRUCT());
		using MouseHook mouseHook = new(wrapper.CoreNativeManager.Object);

		// When
		mouseHook.PostInitialize();

		// Then
		WhimAssert.DoesNotRaise<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonDown += h,
			h => mouseHook.MouseLeftButtonDown -= h,
			() => wrapper.HookProc!.Invoke(0, (WPARAM)PInvoke.WM_KEYDOWN, 1)
		);

		WhimAssert.DoesNotRaise<MouseEventArgs>(
			h => mouseHook.MouseLeftButtonUp += h,
			h => mouseHook.MouseLeftButtonUp -= h,
			() => wrapper.HookProc!.Invoke(0, (WPARAM)PInvoke.WM_KEYDOWN, 1)
		);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();
		using MouseHook mouseHook = new(wrapper.CoreNativeManager.Object);
		mouseHook.PostInitialize();

		// When
		mouseHook.Dispose();

		// Then
		Assert.Equal(1, wrapper.SafeHandle.Calls);
	}

	[Fact]
	public void AlreadyDisposed()
	{
		// Given
		Wrapper wrapper = new();
		using MouseHook mouseHook = new(wrapper.CoreNativeManager.Object);
		mouseHook.PostInitialize();
		mouseHook.Dispose();

		// When
		mouseHook.Dispose();

		// Then
		Assert.Equal(1, wrapper.SafeHandle.Calls);
	}
}
