using System.ComponentModel;
using System.IO;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Whim;

internal class Window : IWindow
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	public required HWND Handle { get; init; }

	public string Title => _internalContext.CoreNativeManager.GetWindowText(Handle);

	public string WindowClass => _context.NativeManager.GetClassName(Handle);

	public bool IsUwp => ProcessFileName == "ApplicationFrameHost.exe";

	public IRectangle<int> Rectangle
	{
		get
		{
			_internalContext.CoreNativeManager.GetWindowRect(Handle, out RECT rect);
			return new Rectangle<int>()
			{
				X = rect.left,
				Y = rect.top,
				Width = rect.right - rect.left,
				Height = rect.bottom - rect.top,
			};
		}
	}

	public required int ProcessId { get; init; }

	public required string? ProcessFileName { get; init; }

	public required string? ProcessFilePath { get; init; }

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
		Handle.Focus(_internalContext);

		// We manually call OnWindowFocused as an already focused window may have switched to a
		// different workspace.
		_internalContext.WindowManager.OnWindowFocused(this);
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

	public void Restore()
	{
		Logger.Debug(ToString());
		_context.NativeManager.RestoreWindow(Handle);
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
	/// Tries to get an existing <see cref="IWindow"/> with the given <paramref name="hwnd"/> if one exists.
	/// Otherwise, tries to create a new <see cref="IWindow"/> with the given <paramref name="hwnd"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="internalContext"></param>
	/// <param name="hwnd">The handle of the window.</param>
	/// <returns></returns>
	public static Result<IWindow> GetOrCreateWindow(IContext context, IInternalContext internalContext, HWND hwnd)
	{
		Logger.Verbose($"Adding window {hwnd}");

		Result<IWindow> res = context.Store.Pick(PickWindowByHandle(hwnd));
		if (res.IsSuccessful)
		{
			return res;
		}

		return CreateWindow(context, internalContext, hwnd);
	}

	/// <summary>
	/// Tries to create a new <see cref="IWindow"/> with the given <paramref name="hwnd"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="internalContext"></param>
	/// <param name="hwnd">The handle of the window.</param>
	/// <returns></returns>
	public static Result<IWindow> CreateWindow(IContext context, IInternalContext internalContext, HWND hwnd)
	{
		_ = internalContext.CoreNativeManager.GetWindowThreadProcessId(hwnd, out uint pid);
		int processId = (int)pid;
		string? processPath;
		string? processFileName;

		try
		{
			(string ProcessName, string? ProcessPath)? result = internalContext.CoreNativeManager.GetProcessNameAndPath(
				processId
			);
			processPath = result?.ProcessPath;

			processFileName = Path.GetFileName(processPath);
		}
		catch (Win32Exception ex)
		{
			// Win32Exception is thrown when it's not possible to get information about the process.
			// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=net-6.0#remarks
			// The exception will usually have a message of:
			// "Unable to enumerate the process modules."		// This will be thrown by Path.GetFileName.
			return new Result<IWindow>(
				new WhimError($"Could not create a Window instance for {hwnd.Value}", ex)
			);
		}

		return new Window(context, internalContext)
		{
			Handle = hwnd,
			ProcessId = processId,
			ProcessFileName = processFileName,
			ProcessFilePath = processPath,
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

	public override string ToString() => $"{Title} ({ProcessFileName}) [{ProcessId}] <{WindowClass}> {{{Handle}}}";

	public BitmapImage? GetIcon() => this.GetIcon(_context, _internalContext);
}
