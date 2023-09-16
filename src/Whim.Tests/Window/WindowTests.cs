using Moq;
using System.ComponentModel;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.Tests;

public class WindowTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IInternalContext> InternalContext { get; } = new();
		public Mock<ICoreNativeManager> CoreNativeManager { get; } = new();
		public Mock<IWindowManager> WindowManager { get; }
		public Mock<IInternalWindowManager> InternalWindowManager { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();

		public Wrapper()
		{
			WindowManager = InternalWindowManager.As<IWindowManager>();

			Context.Setup(c => c.NativeManager).Returns(NativeManager.Object);
			Context.Setup(c => c.WindowManager).Returns(WindowManager.Object);

			InternalContext.Setup(ic => ic.CoreNativeManager).Returns(CoreNativeManager.Object);

			CoreNativeManager
				.Setup(cnm => cnm.GetWindowThreadProcessId(It.IsAny<HWND>(), out It.Ref<uint>.IsAny))
				.Callback(
					(HWND hwnd, out uint processId) =>
					{
						processId = 456;
					}
				);

			CoreNativeManager
				.Setup(cnm => cnm.GetProcessNameAndPath(It.IsAny<int>()))
				.Returns(("processName", "processFileName"));
		}
	}

	[Fact]
	public void Handle()
	{
		// Given
		Wrapper wrapper = new();
		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		HWND handle = window.Handle;

		// Then
		Assert.Equal(123, handle.Value);
	}

	[Fact]
	public void Title()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager.Setup(cnm => cnm.GetWindowText(It.IsAny<HWND>())).Returns("title");

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		string title = window.Title;

		// Then
		Assert.Equal("title", title);
	}

	[Fact]
	public void WindowClass()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.NativeManager.Setup(cnm => cnm.GetClassName(It.IsAny<HWND>())).Returns("windowClass");

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		string windowClass = window.WindowClass;

		// Then
		Assert.Equal("windowClass", windowClass);
	}

	[Fact]
	public void Location()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetWindowRect(It.IsAny<HWND>(), out It.Ref<RECT>.IsAny))
			.Callback(
				(HWND hwnd, out RECT rect) =>
				{
					rect = new RECT()
					{
						left = 0,
						top = 0,
						right = 100,
						bottom = 200
					};
				}
			);

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		ILocation<int> location = window.Location;

		// Then
		Assert.Equal(0, location.X);
		Assert.Equal(0, location.Y);
		Assert.Equal(100, location.Width);
		Assert.Equal(200, location.Height);
	}

	[Fact]
	public void Center()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetWindowRect(It.IsAny<HWND>(), out It.Ref<RECT>.IsAny))
			.Callback(
				(HWND hwnd, out RECT rect) =>
				{
					rect = new RECT()
					{
						left = 0,
						top = 0,
						right = 100,
						bottom = 200
					};
				}
			);

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		IPoint<int> center = window.Center;

		// Then
		Assert.Equal(50, center.X);
		Assert.Equal(100, center.Y);
	}

	[Fact]
	public void ProcessId()
	{
		// Given
		Wrapper wrapper = new();
		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		int processId = window.ProcessId;

		// Then
		Assert.Equal(456, processId);
	}

	[Fact]
	public void ProcessFileName()
	{
		// Given
		Wrapper wrapper = new();
		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		string processFileName = window.ProcessFileName;

		// Then
		Assert.Equal("processFileName", processFileName);
	}

	[Fact]
	public void ProcessFileName_NA()
	{
		// Given
		Wrapper wrapper = new();

		// This is actually wrong - the error is thrown by Path.GetFileName.
		// However, I can't be bothered to mock that out.
		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetProcessNameAndPath(It.IsAny<int>()))
			.Returns((string.Empty, null));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		string processFileName = window.ProcessFileName;

		// Then
		Assert.Equal("--NA--", processFileName);
	}

	[Fact]
	public void ProcessName()
	{
		// Given
		Wrapper wrapper = new();
		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		string processName = window.ProcessName;

		// Then
		Assert.Equal("processName", processName);
	}

	[Fact]
	public void IsFocused()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager.Setup(cnm => cnm.GetForegroundWindow()).Returns(new HWND(123));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		bool isFocused = window.IsFocused;

		// Then
		Assert.True(isFocused);
	}

	[Fact]
	public void IsMinimized()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsWindowMinimized(It.IsAny<HWND>())).Returns(true);

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		bool isMinimized = window.IsMinimized;

		// Then
		Assert.True(isMinimized);
	}

	[Fact]
	public void IsMaximized()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager.Setup(cnm => cnm.IsWindowMaximized(It.IsAny<HWND>())).Returns(true);

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		bool isMaximized = window.IsMaximized;

		// Then
		Assert.True(isMaximized);
	}

	[Fact]
	public void BringToTop()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager.Setup(cnm => cnm.BringWindowToTop(It.IsAny<HWND>()));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		window.BringToTop();

		// Then
		wrapper.CoreNativeManager.Verify(cnm => cnm.BringWindowToTop(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void Close()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.NativeManager.Setup(cnm => cnm.QuitWindow(It.IsAny<HWND>()));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		window.Close();

		// Then
		wrapper.NativeManager.Verify(cnm => cnm.QuitWindow(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void Focus()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager.Setup(cnm => cnm.SetForegroundWindow(It.IsAny<HWND>()));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		window.Focus();

		// Then
		wrapper.CoreNativeManager.Verify(cnm => cnm.SetForegroundWindow(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void FocusForceForeground()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager.Setup(cnm => cnm.SetForegroundWindow(It.IsAny<HWND>()));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		window.FocusForceForeground();

		// Then
		wrapper.CoreNativeManager.Verify(cnm => cnm.SetForegroundWindow(It.IsAny<HWND>()), Times.Once);
		// The following code doesn't work because SendInput accepts a Span.
		wrapper.CoreNativeManager.Verify(cnm => cnm.SendInput(It.IsAny<INPUT[]>(), It.IsAny<int>()), Times.Once);
	}

	[Fact]
	public void Hide()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.NativeManager.Setup(cnm => cnm.HideWindow(It.IsAny<HWND>()));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		window.Hide();

		// Then
		wrapper.NativeManager.Verify(cnm => cnm.HideWindow(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void ShowMaximized()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.NativeManager.Setup(cnm => cnm.ShowWindowMaximized(It.IsAny<HWND>()));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		window.ShowMaximized();

		// Then
		wrapper.NativeManager.Verify(cnm => cnm.ShowWindowMaximized(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void ShowMinimized()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.NativeManager.Setup(cnm => cnm.ShowWindowMinimized(It.IsAny<HWND>()));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		window.ShowMinimized();

		// Then
		wrapper.NativeManager.Verify(cnm => cnm.ShowWindowMinimized(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void ShowNormal()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.NativeManager.Setup(cnm => cnm.ShowWindowNoActivate(It.IsAny<HWND>()));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		window.ShowNormal();

		// Then
		wrapper.NativeManager.Verify(cnm => cnm.ShowWindowNoActivate(It.IsAny<HWND>()), Times.Once);
	}

	[Fact]
	public void CreateWindow_Null()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager.Setup(cnm => cnm.GetProcessNameAndPath(It.IsAny<int>())).Throws(new Win32Exception());

		// When
		IWindow? window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123));

		// Then
		Assert.Null(window);
	}

	[Fact]
	public void Equals_Null()
	{
		// Given
		Wrapper wrapper = new();
		IWindow? window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		bool equals = window.Equals(null);

		// Then
		Assert.False(equals);
	}

	[Fact]
	public void Equals_WrongType()
	{
		// Given
		Wrapper wrapper = new();
		IWindow? window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		bool equals = window.Equals(new object());

		// Then
		Assert.False(equals);
	}

	[Fact]
	public void Equals_NotWindow()
	{
		// Given
		Wrapper wrapper = new();
		IWindow? window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		bool equals = window.Equals(new Mock<IWindow>().Object);

		// Then
		Assert.False(equals);
	}

	[Fact]
	public void Equals_Success()
	{
		// Given
		Wrapper wrapper = new();
		IWindow? window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;
		IWindow? window2 = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		bool equals = window.Equals(window2);

		// Then
		Assert.True(equals);
	}

	[Fact]
	public void Equals_Operator_Success()
	{
		// Given
		Wrapper wrapper = new();
		Window? window =
			Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))! as Window;
		Window? window2 =
			Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))! as Window;

		// When
		bool equals = window == window2;

		// Then
		Assert.True(equals);
	}

	[Fact]
	public void NotEquals_Operator_Success()
	{
		// Given
		Wrapper wrapper = new();
		Window? window =
			Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))! as Window;
		Window? window2 =
			Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(1234))! as Window;

		// When
		bool equals = window != window2;

		// Then
		Assert.True(equals);
	}

	[Fact]
	public void GetHashCode_Success()
	{
		// Given
		Wrapper wrapper = new();
		IWindow? window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		int hashCode = window.GetHashCode();

		// Then
		Assert.Equal(hashCode, 123.GetHashCode());
	}

	#region IsUwp
	[Fact]
	public void IsUwp_True()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetProcessNameAndPath(It.IsAny<int>()))
			.Returns(("processName", "app/ApplicationFrameHost.exe"));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		bool isUwp = window.IsUwp;

		// Then
		Assert.True(isUwp);
	}

	[Fact]
	public void IsUwp_False()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.CoreNativeManager
			.Setup(cnm => cnm.GetProcessNameAndPath(It.IsAny<int>()))
			.Returns(("processName", "processFileName"));

		IWindow window = Window.CreateWindow(wrapper.Context.Object, wrapper.InternalContext.Object, new HWND(123))!;

		// When
		bool isUwp = window.IsUwp;

		// Then
		Assert.False(isUwp);
	}
	#endregion
}
