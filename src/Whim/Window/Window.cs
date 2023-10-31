using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.IO;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim;

internal class Window : IWindow
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	public required HWND Handle { get; init; }

	public string Title => _internalContext.CoreNativeManager.GetWindowText(Handle);

	public string WindowClass => _context.NativeManager.GetClassName(Handle);

	public bool IsUwp => ProcessFileName == "ApplicationFrameHost.exe";

	public ILocation<int> Location
	{
		get
		{
			_internalContext.CoreNativeManager.GetWindowRect(Handle, out RECT rect);
			return new Location<int>()
			{
				X = rect.left,
				Y = rect.top,
				Width = rect.right - rect.left,
				Height = rect.bottom - rect.top
			};
		}
	}

	public IPoint<int> Center
	{
		get
		{
			ILocation<int> location = Location;
			return new Point<int>() { X = location.X + (location.Width / 2), Y = location.Y + (location.Height / 2) };
		}
	}

	public required int ProcessId { get; init; }

	public required string? ProcessFileName { get; init; }

	public required string? ProcessFilePath { get; init; }

	public required string? ProcessName { get; init; }

	public bool IsFocused => _internalContext.CoreNativeManager.GetForegroundWindow() == Handle;

	public bool IsMinimized => _internalContext.CoreNativeManager.IsWindowMinimized(Handle);

	public bool IsMaximized => _internalContext.CoreNativeManager.IsWindowMaximized(Handle);

	public void BringToTop()
	{
		Logger.Debug(ToString());
		_internalContext.CoreNativeManager.BringWindowToTop(Handle);
	}

	public void Close()
	{
		Logger.Debug(ToString());
		_context.NativeManager.QuitWindow(Handle);
	}

	public void Focus()
	{
		Logger.Debug(ToString());
		if (!IsFocused)
		{
			_internalContext.CoreNativeManager.SetForegroundWindow(Handle);
		}

		// We manually call OnWindowFocused as an already focused window may have switched to a
		// different workspace.
		((IInternalWindowManager)_context.WindowManager).OnWindowFocused(this);
	}

	public void FocusForceForeground()
	{
		Logger.Debug(ToString());
		// Use SendInput hack to allow Activate to work - required to resolve focus issue https://github.com/microsoft/PowerToys/issues/4270
		unsafe
		{
			INPUT input = new() { type = INPUT_TYPE.INPUT_MOUSE };
			// Send empty mouse event. This makes this thread the last to send input, and hence allows it to pass foreground permission checks
			_ = _internalContext.CoreNativeManager.SendInput(new[] { input }, sizeof(INPUT));
		}

		_internalContext.CoreNativeManager.SetForegroundWindow(Handle);

		// We manually call OnWindowFocused as an already focused window may have switched to a
		// different workspace.
		((IInternalWindowManager)_context.WindowManager).OnWindowFocused(this);
	}

	public void Hide()
	{
		Logger.Debug(ToString());
		_context.NativeManager.HideWindow(Handle);
	}

	public void ShowMaximized()
	{
		Logger.Debug(ToString());
		_context.NativeManager.ShowWindowMaximized(Handle);
	}

	public void ShowMinimized()
	{
		Logger.Debug(ToString());
		_context.NativeManager.ShowWindowMinimized(Handle);
	}

	public void ShowNormal()
	{
		Logger.Debug(ToString());
		_context.NativeManager.ShowWindowNoActivate(Handle);
	}

	/// <summary>
	/// Constructor for the <see cref="IWindow"/> implementation.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="internalContext"></param>
	/// <exception cref="Win32Exception"></exception>
	private Window(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	/// <summary>
	/// Tries to create a new <see cref="IWindow"/> with the given <paramref name="hwnd"/>.
	/// Otherwise, returns <see langword="null"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="internalContext"></param>
	/// <param name="hwnd">The handle of the window.</param>
	/// <returns></returns>
	public static IWindow? CreateWindow(IContext context, IInternalContext internalContext, HWND hwnd)
	{
		_ = internalContext.CoreNativeManager.GetWindowThreadProcessId(hwnd, out uint pid);
		int processId = (int)pid;
		string? processName;
		string? processPath;
		string? processFileName;

		try
		{
			(string ProcessName, string? ProcessPath)? result = internalContext.CoreNativeManager.GetProcessNameAndPath(
				processId
			);
			processName = result?.ProcessName;
			processPath = result?.ProcessPath;

			processFileName = Path.GetFileName(processPath);
		}
		catch (Win32Exception ex)
		{
			// Win32Exception is thrown when it's not possible to get information about the process.
			// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=net-6.0#remarks
			// The exception will usually have a message of:
			// "Unable to enumerate the process modules."
			// This will be thrown by Path.GetFileName.
			Logger.Error($"Could not create a Window instance for {hwnd.Value}, {ex.Message}");
			return null;
		}

		return new Window(context, internalContext)
		{
			Handle = hwnd,
			ProcessId = processId,
			ProcessName = processName,
			ProcessFileName = processFileName,
			ProcessFilePath = processPath
		};
	}

	public override bool Equals(object? obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		// It won't reach here if the type is different
		return ((Window)obj).Handle == Handle;
	}

	public static bool operator ==(Window? left, Window? right) => Equals(left, right);

	public static bool operator !=(Window? left, Window? right) => !Equals(left, right);

	public override int GetHashCode()
	{
		return Handle.GetHashCode();
	}

	public override string ToString() => $"{Title} ({ProcessName}) [{ProcessId}] <{WindowClass}> {{{Handle.Value}}}";

	public BitmapImage? GetIcon() => this.GetIcon(_context, _internalContext);
}
