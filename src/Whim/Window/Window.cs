using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

internal class Window : IWindow
{
	private readonly IConfigContext _configContext;
	private readonly ICoreNativeManager _coreNativeManager;

	/// <inheritdoc/>
	public required HWND Handle { get; init; }

	/// <inheritdoc/>
	public string Title => _configContext.NativeManager.GetWindowText(Handle);

	/// <inheritdoc/>
	public string WindowClass => _configContext.NativeManager.GetClassName(Handle);

	/// <inheritdoc/>
	public ILocation<int> Location
	{
		get
		{
			_coreNativeManager.GetWindowRect(Handle, out RECT rect);
			return new Location<int>()
			{
				X = rect.left,
				Y = rect.top,
				Width = rect.right - rect.left,
				Height = rect.bottom - rect.top
			};
		}
	}

	/// <inheritdoc/>
	public IPoint<int> Center
	{
		get
		{
			ILocation<int> location = Location;
			return new Point<int>() { X = location.X + (location.Width / 2), Y = location.Y + (location.Height / 2) };
		}
	}

	/// <inheritdoc/>
	public required int ProcessId { get; init; }

	/// <inheritdoc/>
	public required string ProcessFileName { get; init; }

	/// <inheritdoc/>
	public required string ProcessName { get; init; }

	/// <inheritdoc/>
	public bool IsFocused => _coreNativeManager.GetForegroundWindow() == Handle;

	/// <inheritdoc/>
	public bool IsMinimized => _coreNativeManager.IsWindowMinimized(Handle);

	/// <inheritdoc/>
	public bool IsMaximized => _coreNativeManager.IsWindowMaximized(Handle);

	/// <inheritdoc/>
	public bool IsMouseMoving { get; set; }

	/// <inheritdoc/>
	public void BringToTop()
	{
		Logger.Debug(ToString());
		_coreNativeManager.BringWindowToTop(Handle);
	}

	/// <inheritdoc/>
	public void Close()
	{
		Logger.Debug(ToString());
		_configContext.NativeManager.QuitWindow(Handle);
	}

	/// <inheritdoc/>
	public void Focus()
	{
		Logger.Debug(ToString());
		if (!IsFocused)
		{
			_coreNativeManager.SetForegroundWindow(Handle);
		}

		// We manually call OnWindowFocused as an already focused window may have switched to a
		// different workspace.
		(_configContext.WindowManager as WindowManager)?.OnWindowFocused(this);
	}

	/// <inheritdoc/>
	public void FocusForceForeground()
	{
		Logger.Debug(ToString());
		// Use SendInput hack to allow Activate to work - required to resolve focus issue https://github.com/microsoft/PowerToys/issues/4270
		unsafe
		{
			INPUT input = new() { type = INPUT_TYPE.INPUT_MOUSE };
			INPUT[] inputs = new[] { input };
			// Send empty mouse event. This makes this thread the last to send input, and hence allows it to pass foreground permission checks
			_ = _coreNativeManager.SendInput(inputs, sizeof(INPUT));
		}

		_coreNativeManager.SetForegroundWindow(Handle);

		// We manually call OnWindowFocused as an already focused window may have switched to a
		// different workspace.
		(_configContext.WindowManager as WindowManager)?.OnWindowFocused(this);
	}

	/// <inheritdoc/>
	public void Hide()
	{
		Logger.Debug(ToString());
		_configContext.NativeManager.HideWindow(Handle);
	}

	/// <inheritdoc/>
	public void ShowInCurrentState()
	{
		Logger.Debug(ToString());
		if (IsMinimized)
		{
			ShowMinimized();
		}
		else if (IsMaximized)
		{
			ShowMaximized();
		}
		else
		{
			ShowNormal();
		}
	}

	/// <inheritdoc/>
	public void ShowMaximized()
	{
		Logger.Debug(ToString());
		_configContext.NativeManager.ShowWindowMaximized(Handle);
	}

	/// <inheritdoc/>
	public void ShowMinimized()
	{
		Logger.Debug(ToString());
		_configContext.NativeManager.ShowWindowMinimized(Handle);
	}

	/// <inheritdoc/>
	public void ShowNormal()
	{
		Logger.Debug(ToString());
		_configContext.NativeManager.ShowWindowNoActivate(Handle);
	}

	/// <summary>
	/// Constructor for the <see cref="IWindow"/> implementation.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="coreNativeManager"></param>
	/// <exception cref="Win32Exception"></exception>
	private Window(IConfigContext configContext, ICoreNativeManager coreNativeManager)
	{
		_configContext = configContext;
		_coreNativeManager = coreNativeManager;
	}

	/// <summary>
	/// Tries to create a new <see cref="IWindow"/> with the given <paramref name="hwnd"/>.
	/// Otherwise, returns <see langword="null"/>.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="coreNativeManager"></param>
	/// <param name="hwnd">The handle of the window.</param>
	/// <returns></returns>
	public static IWindow? CreateWindow(IConfigContext configContext, ICoreNativeManager coreNativeManager, HWND hwnd)
	{
		string processName;
		string processFileName;

		_ = coreNativeManager.GetWindowThreadProcessId(hwnd, out uint pid);
		int processId = (int)pid;

		Process process = Process.GetProcessById(processId);
		processName = process.ProcessName;

		try
		{
			processFileName = Path.GetFileName(process.MainModule?.FileName) ?? "--NA--";
		}
		catch (Win32Exception ex)
		{
			// Win32Exception is thrown when it's not possible to get information about the process.
			// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=net-6.0#remarks
			// The exception will usually have a message of:
			// "Unable to enumerate the process modules."
			Logger.Error($"Could not create a Window instance for {hwnd.Value}, {ex.Message}");
			return null;
		}

		return new Window(configContext, coreNativeManager)
		{
			Handle = hwnd,
			ProcessId = processId,
			ProcessName = processName,
			ProcessFileName = processFileName,
		};
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		return obj is Window window && window.Handle == Handle;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return Handle.GetHashCode();
	}

	/// <inheritdoc/>
	public override string ToString() => $"{Title} ({ProcessName}) [{ProcessId}] <{WindowClass}> {{{Handle.Value}}}";
}
