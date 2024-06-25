using System.ComponentModel;
using AutoFixture;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.Tests;

internal class WindowCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		IWindowManager windowManager = Substitute.For<IWindowManager, IInternalWindowManager>();
		ctx.WindowManager.Returns(windowManager);

		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		internalCtx.CoreNativeManager.GetProcessNameAndPath(Arg.Any<int>()).Returns(("processName", "processFileName"));

		internalCtx
			.CoreNativeManager.GetWindowThreadProcessId(Arg.Any<HWND>(), out uint _)
			.Returns(
				(x) =>
				{
					x[1] = (uint)456;
					return (uint)1;
				}
			);

		internalCtx
			.CoreNativeManager.GetWindowRect(Arg.Any<HWND>(), out RECT _)
			.Returns(
				(x) =>
				{
					x[1] = new RECT()
					{
						left = 0,
						top = 0,
						right = 100,
						bottom = 200
					};
					return (BOOL)1;
				}
			);
	}
}

public class WindowTests
{
	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Handle(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		HWND handle = window.Handle;

		// Then
		Assert.Equal(123, handle.Value);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Title(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx.CoreNativeManager.GetWindowText(Arg.Any<HWND>()).Returns("title");

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		string title = window.Title;

		// Then
		Assert.Equal("title", title);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void WindowClass(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		ctx.NativeManager.GetClassName(Arg.Any<HWND>()).Returns("windowClass");

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		string windowClass = window.WindowClass;

		// Then
		Assert.Equal("windowClass", windowClass);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Rectangle(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		IRectangle<int> rect = window.Rectangle;

		// Then
		Assert.Equal(0, rect.X);
		Assert.Equal(0, rect.Y);
		Assert.Equal(100, rect.Width);
		Assert.Equal(200, rect.Height);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void ProcessId(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		int processId = window.ProcessId;

		// Then
		Assert.Equal(456, processId);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void ProcessFileName(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		string? processFileName = window.ProcessFileName;

		// Then
		Assert.Equal("processFileName", processFileName);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void ProcessFileName_NA(IContext ctx, IInternalContext internalCtx)
	{
		// Given

		// This is actually wrong - the error is thrown by Path.GetFileName.
		// However, I can't be bothered to mock that out.
		internalCtx.CoreNativeManager.GetProcessNameAndPath(Arg.Any<int>()).Returns((string.Empty, null));

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		string? processFileName = window.ProcessFileName;

		// Then
		Assert.Null(processFileName);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void IsFocused(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx.CoreNativeManager.GetForegroundWindow().Returns(new HWND(123));

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		bool isFocused = window.IsFocused;

		// Then
		Assert.True(isFocused);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void IsMinimized(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx.CoreNativeManager.IsWindowMinimized(Arg.Any<HWND>()).Returns((BOOL)true);

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		bool isMinimized = window.IsMinimized;

		// Then
		Assert.True(isMinimized);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void IsMaximized(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx.CoreNativeManager.IsWindowMaximized(Arg.Any<HWND>()).Returns((BOOL)true);

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		bool isMaximized = window.IsMaximized;

		// Then
		Assert.True(isMaximized);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void BringToTop(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx.CoreNativeManager.BringWindowToTop(Arg.Any<HWND>());

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		window.BringToTop();

		// Then
		internalCtx.CoreNativeManager.Received(1).BringWindowToTop(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Close(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		ctx.NativeManager.QuitWindow(Arg.Any<HWND>());

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		window.Close();

		// Then
		ctx.NativeManager.Received(1).QuitWindow(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Focus(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx.CoreNativeManager.SetForegroundWindow(Arg.Any<HWND>());

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		window.Focus();

		// Then
		internalCtx.CoreNativeManager.Received(1).SetForegroundWindow(Arg.Any<HWND>());
		// The following code doesn't work because SendInput accepts a Span.
		internalCtx.CoreNativeManager.Received(1).SendInput(Arg.Any<INPUT[]>(), Arg.Any<int>());
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Hide(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		ctx.NativeManager.HideWindow(Arg.Any<HWND>());

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		window.Hide();

		// Then
		ctx.NativeManager.Received(1).HideWindow(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void ShowMaximized(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		ctx.NativeManager.ShowWindowMaximized(Arg.Any<HWND>());

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		window.ShowMaximized();

		// Then
		ctx.NativeManager.Received(1).ShowWindowMaximized(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void ShowMinimized(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		ctx.NativeManager.ShowWindowMinimized(Arg.Any<HWND>());

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		window.ShowMinimized();

		// Then
		ctx.NativeManager.Received(1).ShowWindowMinimized(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void ShowNormal(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		ctx.NativeManager.ShowWindowNoActivate(Arg.Any<HWND>());

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		window.ShowNormal();

		// Then
		ctx.NativeManager.Received(1).ShowWindowNoActivate(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Restore(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		ctx.NativeManager.RestoreWindow(Arg.Any<HWND>());

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		window.Restore();

		// Then
		ctx.NativeManager.Received(1).RestoreWindow(Arg.Any<HWND>());
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void CreateWindow_Null(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx
			.CoreNativeManager.When(x => x.GetProcessNameAndPath(Arg.Any<int>()))
			.Do(x => throw new Win32Exception());

		// When
		IWindow? window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// Then
		Assert.Null(window);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Equals_Null(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWindow? window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		bool equals = window.Equals(null);

		// Then
		Assert.False(equals);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Equals_WrongType(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWindow? window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		bool equals = window.Equals(new object());

		// Then
		Assert.False(equals);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Equals_NotWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWindow? window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		bool equals = window.Equals(Substitute.For<IWindow>());

		// Then
		Assert.False(equals);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Equals_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWindow? window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;
		IWindow? window2 = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		bool equals = window.Equals(window2);

		// Then
		Assert.True(equals);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void Equals_Operator_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		Window? window = Window.CreateWindow(ctx, internalCtx, new HWND(123))!.OrDefault()! as Window;
		Window? window2 = Window.CreateWindow(ctx, internalCtx, new HWND(123))!.OrDefault()! as Window;

		// When
		bool equals = window == window2;

		// Then
		Assert.True(equals);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void NotEquals_Operator_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		Window? window = Window.CreateWindow(ctx, internalCtx, new HWND(123))!.OrDefault()! as Window;
		Window? window2 = Window.CreateWindow(ctx, internalCtx, new HWND(1234))!.OrDefault()! as Window;

		// When
		bool equals = window != window2;

		// Then
		Assert.True(equals);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void GetHashCode_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWindow? window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		int hashCode = window.GetHashCode();

		// Then
		Assert.Equal(hashCode, 123.GetHashCode());
	}

	#region IsUwp
	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void IsUwp_True(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx
			.CoreNativeManager.GetProcessNameAndPath(Arg.Any<int>())
			.Returns(("processName", "app/ApplicationFrameHost.exe"));

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		bool isUwp = window.IsUwp;

		// Then
		Assert.True(isUwp);
	}

	[Theory, AutoSubstituteData<WindowCustomization>]
	internal void IsUwp_False(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		internalCtx
			.CoreNativeManager.GetProcessNameAndPath(Arg.Any<int>())
			.Returns(("processName", "processFileName"));

		IWindow window = Window.CreateWindow(ctx, internalCtx, new HWND(123)).OrDefault()!;

		// When
		bool isUwp = window.IsUwp;

		// Then
		Assert.False(isUwp);
	}
	#endregion
}
