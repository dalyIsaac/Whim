using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
// We do use this in non-DEBUG.
#pragma warning disable IDE0005 // Using directive is unnecessary.
using System.Reflection;
#pragma warning restore IDE0005 // Using directive is unnecessary.
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Composition;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.System.WinRT;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal partial class NativeManager : INativeManager
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue =
		Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

	/// <summary>
	/// Initializes a new instance of the <see cref="NativeManager"/> class.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="internalContext"></param>
	internal NativeManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	private const int _bufferCapacity = 255;

	public void QuitWindow(HWND hwnd)
	{
		Logger.Debug($"Quitting application with HWND {hwnd}");
		PInvoke.SendNotifyMessage(hwnd, PInvoke.WM_SYSCOMMAND, new WPARAM(PInvoke.SC_CLOSE), 0);
	}

	public void ForceForegroundWindow(HWND hwnd)
	{
		Logger.Debug($"Forcing window HWND {hwnd} to foreground");
		// Implementation courtesy of https://github.com/workspacer/workspacer/commit/1c02613cea485f1ae97f70d6399f7124aeb31297
		// keybd_event synthesizes a keystroke - see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-keybd_event
		PInvoke.keybd_event(0, 0, 0, 0);
		PInvoke.SetForegroundWindow(hwnd);
	}

	public bool HideWindow(HWND hwnd)
	{
		Logger.Debug($"Hiding window HWND {hwnd}");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_HIDE);
	}

	public bool ShowWindowMaximized(HWND hwnd)
	{
		Logger.Debug($"Showing window HWND {hwnd} maximized");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMAXIMIZED);
	}

	public bool ShowWindowMinimized(HWND hwnd)
	{
		Logger.Debug($"Showing window HWND {hwnd} minimized");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWMINIMIZED);
	}

	public bool MinimizeWindow(HWND hwnd)
	{
		Logger.Debug($"Minimizing window HWND {hwnd}");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_MINIMIZE);
	}

	public bool ShowWindowNoActivate(HWND hwnd)
	{
		Logger.Debug($"Showing window HWND {hwnd} no activate");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOWNOACTIVATE);
	}

	public bool RestoreWindow(HWND hwnd)
	{
		Logger.Debug($"Restoring window HWND {hwnd}");
		return (bool)PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_RESTORE);
	}

	public string GetClassName(HWND hwnd)
	{
		unsafe
		{
			fixed (char* buffer = new char[_bufferCapacity])
			{
				int length = PInvoke.GetClassName(hwnd, buffer, _bufferCapacity + 1);
				return length > 0 ? new string(buffer) : "";
			}
		}
	}

	public void HideCaptionButtons(HWND hwnd)
	{
		int style = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);

		// Hide the title bar and caption buttons
		style &= ~(int)WINDOW_STYLE.WS_CAPTION & ~(int)WINDOW_STYLE.WS_THICKFRAME;

		_ = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);
	}

	public void PreventWindowActivation(HWND hwnd)
	{
		int exStyle = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);

		// Prevent the window from being activated
		exStyle |= (int)WINDOW_EX_STYLE.WS_EX_NOACTIVATE;

		_ = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);
	}

	public IRectangle<int>? GetWindowOffset(HWND hwnd)
	{
		if (!PInvoke.GetWindowRect(hwnd, out RECT windowRect))
		{
			Logger.Error($"Could not get the window rect for {hwnd}");
			return null;
		}

		IRectangle<int>? extendedFrameRectangle = DwmGetWindowRectangle(hwnd);
		if (extendedFrameRectangle == null)
		{
			return null;
		}

		return new Rectangle<int>()
		{
			X = windowRect.left - extendedFrameRectangle.X,
			Y = windowRect.top - extendedFrameRectangle.Y,
			Width = windowRect.right - windowRect.left - extendedFrameRectangle.Width,
			Height = windowRect.bottom - windowRect.top - extendedFrameRectangle.Height
		};
	}

	public IRectangle<int>? DwmGetWindowRectangle(HWND hwnd)
	{
		unsafe
		{
			RECT extendedFrameRect = new();
			uint size = (uint)Marshal.SizeOf<RECT>();
			HRESULT res = PInvoke.DwmGetWindowAttribute(
				hwnd,
				DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
				&extendedFrameRect,
				size
			);

			if (res.Failed)
			{
				Logger.Error($"Could not get the extended frame rect for {hwnd}");
				return null;
			}

			return new Rectangle<int>()
			{
				X = extendedFrameRect.left,
				Y = extendedFrameRect.top,
				Width = extendedFrameRect.right - extendedFrameRect.left,
				Height = extendedFrameRect.bottom - extendedFrameRect.top
			};
		}
	}

	public void SetWindowCorners(
		HWND hwnd,
		DWM_WINDOW_CORNER_PREFERENCE preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND
	)
	{
		unsafe
		{
			HRESULT res = PInvoke.DwmSetWindowAttribute(
				hwnd,
				DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
				&preference,
				sizeof(DWM_WINDOW_CORNER_PREFERENCE)
			);
			if (res.Failed)
			{
				Logger.Error($"Failed to set window corners for {hwnd}");
			}
		}
	}

	/// <inheritdoc />
	public DeferWindowPosHandle DeferWindowPos() => new(_context, _internalContext);

	/// <inheritdoc />
	public DeferWindowPosHandle DeferWindowPos(IEnumerable<SetWindowPosState> windowStates) =>
		new(_context, _internalContext, windowStates);

	/// <inheritdoc />
	public string? GetUwpAppProcessPath(IWindow window)
	{
		if (!window.IsUwp)
		{
			Logger.Error("Cannot get UWP app process path for non-UWP window");
			return null;
		}

		_internalContext.CoreNativeManager.GetWindowThreadProcessId(window.Handle, out uint pid);

		// UWP apps are hosted inside a ApplicationFrameHost process.
		// We need to find the child window which does NOT belong to this process.
		foreach (HWND childHwnd in _internalContext.CoreNativeManager.GetChildWindows(window.Handle))
		{
			_internalContext.CoreNativeManager.GetWindowThreadProcessId(childHwnd, out uint childPid);

			if (childPid != pid)
			{
				return _internalContext.CoreNativeManager.GetProcessNameAndPath((int)childPid)?.ProcessPath;
			}
		}

		Logger.Error("Cannot find a path to Uwp App executable file for HWND ${hwnd}");
		return null;
	}

	private static Compositor? _compositor;
	private static readonly object _compositorLock = new();

	public Compositor Compositor
	{
		get
		{
			if (_compositor == null)
			{
				lock (_compositorLock)
				{
#pragma warning disable CA1508 // Avoid dead conditional code, because of the lock.
					if (_compositor == null)
					{
						if (DispatcherQueue.GetForCurrentThread() == null)
						{
							InitializeCoreDispatcher();
						}

						_compositor = new Compositor();
					}
#pragma warning restore CA1508 // Avoid dead conditional code
				}
			}

			return _compositor;
		}
	}

	private static DispatcherQueueController InitializeCoreDispatcher()
	{
		// Ideally this would be replaced by the dispatcher from Microsoft.UI.
		// However, ICompositionSupportsSystemBackdrop.SystemBackdrop uses Windows.UI.
		DispatcherQueueOptions options =
			new()
			{
				apartmentType = DISPATCHERQUEUE_THREAD_APARTMENTTYPE.DQTAT_COM_STA,
				threadType = DISPATCHERQUEUE_THREAD_TYPE.DQTYPE_THREAD_CURRENT,
				dwSize = (uint)Marshal.SizeOf(typeof(DispatcherQueueOptions))
			};

		PInvoke.CreateDispatcherQueueController(options, out nint raw);

		return DispatcherQueueController.FromAbi(raw);
	}

	[LibraryImport("UXTheme.dll", EntryPoint = "#138", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool _ShouldSystemUseDarkMode();

	public bool ShouldSystemUseDarkMode() => _ShouldSystemUseDarkMode();

	public TransparentWindowController CreateTransparentWindowController(Microsoft.UI.Xaml.Window window) =>
		new(_context, _internalContext, window);

	public void SetWindowExTransparent(HWND hwnd)
	{
		int exStyle = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
		_ = PInvoke.SetWindowLong(
			hwnd,
			WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
			exStyle | (int)WINDOW_EX_STYLE.WS_EX_LAYERED
		);
	}

	public void RemoveWindowExTransparent(HWND hwnd)
	{
		int exStyle = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
		_ = PInvoke.SetWindowLong(
			hwnd,
			WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
			exStyle & ~(int)WINDOW_EX_STYLE.WS_EX_LAYERED
		);
	}

	public bool TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueueHandler callback) =>
		_dispatcherQueue.TryEnqueue(callback);

	public string GetWhimVersion()
	{
#if DEBUG
		// An arbitrary version number for debugging.
		return "v0.1.263-alpha+bc5c56c4";
#else
		return Assembly.GetExecutingAssembly().GetName().Version!.ToString();
#endif
	}

	public async Task DownloadFileAsync(Uri uri, string destinationPath)
	{
		using HttpClient httpClient = new();
		using HttpResponseMessage response = await httpClient.GetAsync(uri).ConfigureAwait(false);

		// Save the asset to a temporary file.
		using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using Stream streamToWriteTo = File.Open(destinationPath, System.IO.FileMode.Create);
		await streamToReadFrom.CopyToAsync(streamToWriteTo).ConfigureAwait(false);
	}

	public async Task<int> RunFileAsync(string path)
	{
		using Process process = new();
		process.StartInfo.FileName = path;
		process.Start();
		await process.WaitForExitAsync().ConfigureAwait(false);
		return process.ExitCode;
	}
}
