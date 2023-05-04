using Moq;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class WindowMessageMonitorTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<ICoreNativeManager> CoreNativeManager { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public SUBCLASSPROC? SubclassProc { get; private set; }

		public Wrapper()
		{
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);

			CoreNativeManager
				.Setup(
					cnm =>
						cnm.SetWindowSubclass(
							It.IsAny<HWND>(),
							It.IsAny<SUBCLASSPROC>(),
							It.IsAny<nuint>(),
							It.IsAny<nuint>()
						)
				)
				.Callback(
					(HWND hwnd, SUBCLASSPROC subclassProc, nuint subclassId, nuint refData) =>
						SubclassProc = subclassProc
				);
		}
	}

	[Fact]
	public void Constructor()
	{
		// Given
		Wrapper wrapper = new();

		// When
		WindowMessageMonitor windowMessageMonitor = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// Then
		wrapper.CoreNativeManager.Verify(
			x => x.SetWindowSubclass(It.IsAny<HWND>(), It.IsAny<SUBCLASSPROC>(), It.IsAny<nuint>(), It.IsAny<nuint>()),
			Times.Once
		);
		wrapper.CoreNativeManager.Verify(
			x => x.WTSRegisterSessionNotification(It.IsAny<HWND>(), It.IsAny<uint>()),
			Times.Once
		);
	}

	[Fact]
	public void WindowProc_WM_DISPLAYCHANGE()
	{
		// Given
		Wrapper wrapper = new();

		// When
		WindowMessageMonitor windowMessageMonitor = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// Then
		Assert.Raises<WindowMessageMonitorEventArgs>(
			h => windowMessageMonitor.DisplayChanged += h,
			h => windowMessageMonitor.DisplayChanged -= h,
			() => wrapper.SubclassProc?.Invoke((HWND)0, PInvoke.WM_DISPLAYCHANGE, (WPARAM)1, (LPARAM)1, 0, 0)
		);
	}

	[Fact]
	public void WindowProc_WM_WTSESSION_CHANGE()
	{
		// Given
		Wrapper wrapper = new();

		// When
		WindowMessageMonitor windowMessageMonitor = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// Then
		Assert.Raises<WindowMessageMonitorEventArgs>(
			h => windowMessageMonitor.SessionChanged += h,
			h => windowMessageMonitor.SessionChanged -= h,
			() => wrapper.SubclassProc?.Invoke((HWND)0, PInvoke.WM_WTSSESSION_CHANGE, (WPARAM)1, (LPARAM)1, 0, 0)
		);
	}

	[Fact]
	public void WindowProc_WM_SETTINGCHANGE_SPI_GETWORKAREA()
	{
		// Given
		Wrapper wrapper = new();

		// When
		WindowMessageMonitor windowMessageMonitor = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// Then
		Assert.Raises<WindowMessageMonitorEventArgs>(
			h => windowMessageMonitor.WorkAreaChanged += h,
			h => windowMessageMonitor.WorkAreaChanged -= h,
			() =>
				wrapper.SubclassProc?.Invoke(
					(HWND)0,
					PInvoke.WM_SETTINGCHANGE,
					new WPARAM((nuint)SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA),
					(LPARAM)1,
					0,
					0
				)
		);
	}

	[Fact]
	public void WindowProc_WM_SETTINGCHANGE_SPI_SETLOGICALDPIOVERRIDE()
	{
		// Given
		Wrapper wrapper = new();

		// When
		WindowMessageMonitor windowMessageMonitor = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);

		// Then
		Assert.Raises<WindowMessageMonitorEventArgs>(
			h => windowMessageMonitor.DpiChanged += h,
			h => windowMessageMonitor.DpiChanged -= h,
			() =>
				wrapper.SubclassProc?.Invoke(
					(HWND)0,
					PInvoke.WM_SETTINGCHANGE,
					new WPARAM((nuint)SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETLOGICALDPIOVERRIDE),
					(LPARAM)1,
					0,
					0
				)
		);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();

		// When
		WindowMessageMonitor windowMessageMonitor = new(wrapper.Context.Object, wrapper.CoreNativeManager.Object);
		windowMessageMonitor.Dispose();

		// Then
		wrapper.CoreNativeManager.Verify(
			x => x.RemoveWindowSubclass(It.IsAny<HWND>(), It.IsAny<SUBCLASSPROC>(), It.IsAny<nuint>()),
			Times.Once
		);
		wrapper.CoreNativeManager.Verify(x => x.WTSUnRegisterSessionNotification(It.IsAny<HWND>()), Times.Once);
	}
}
