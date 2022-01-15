using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Whim;

public class Window : IWindow
{
	private readonly IConfigContext _configContext;

	public HWND Handle { get; }

	public string Title { get => Win32Helper.GetWindowText(Handle); }

	public string Class { get => Win32Helper.GetClassName(Handle); }
	public ILocation<int> Location
	{
		get
		{
			PInvoke.GetWindowRect(Handle, out RECT rect);
			return new Location(rect.left,
										 rect.top,
										 rect.right - rect.left,
										 rect.bottom - rect.top);
		}
	}

	public int ProcessId { get; }

	public string ProcessFileName { get; }

	public string ProcessName { get; }

	public bool IsFocused => PInvoke.GetForegroundWindow() == Handle;

	public bool IsMinimized => PInvoke.IsIconic(Handle);

	public bool IsMaximized => PInvoke.IsZoomed(Handle);

	public bool IsMouseMoving { get; internal set; }

	public void BringToTop()
	{
		Logger.Debug(ToString());
		PInvoke.BringWindowToTop(Handle);
	}

	public void Close()
	{
		Logger.Debug(ToString());
		Win32Helper.QuitApplication(Handle);
		_configContext.WindowManager.TriggerWindowUnregistered(new WindowEventArgs(this));
	}

	public void Focus()
	{
		Logger.Debug(ToString());
		if (!IsFocused)
		{
			PInvoke.SetForegroundWindow(Handle);
		}

		// Sometimes we want to let listeners that the window is focused, but
		// other things might have changed, like the monitor the window's on.
		// For example, <see cref="MonitorManager.FocusMonitor"/> relies on
		// <see cref="IWindowManager.WindowFocused"/> to be called.
		// Admittedly, this is a bit of a hack.
		_configContext.WindowManager.TriggerWindowFocused(new WindowEventArgs(this));
	}

	public void Hide()
	{
		Logger.Debug(ToString());
		Win32Helper.HideWindow(Handle);
		_configContext.WindowManager.TriggerWindowUpdated(new WindowUpdateEventArgs(this, WindowUpdateType.Cloaked));
	}

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

	public void ShowMaximized()
	{
		Logger.Debug(ToString());
		Win32Helper.ShowWindowMaximized(Handle);
	}

	public void ShowMinimized()
	{
		Logger.Debug(ToString());
		Win32Helper.ShowWindowMinimized(Handle);
	}

	public void ShowNormal()
	{
		Logger.Debug(ToString());
		Win32Helper.ShowWindowNoActivate(Handle);
	}

	/// <summary>
	/// Constructor for the <see cref="IWindow"/> implementation.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <param name="configContext"></param>
	/// <exception cref="Win32Exception"></exception>
	private Window(HWND hwnd, IConfigContext configContext)
	{
		_configContext = configContext;
		Handle = hwnd;

		unsafe
		{
			uint pid;
			_ = PInvoke.GetWindowThreadProcessId(Handle, &pid);
			ProcessId = (int)pid;
		}

		Process process = Process.GetProcessById(ProcessId);
		ProcessName = process.ProcessName;

		try
		{
			ProcessFileName = Path.GetFileName(process.MainModule?.FileName) ?? "--NA--";
		}
		catch (Win32Exception ex)
		{
			// Win32Exception is thrown when it's not possible to get information about the process.
			// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=net-6.0#remarks
			// The exception will usually have a message of:
			// "Unable to enumerate the process modules."
			ProcessFileName = "--NA--";
			Logger.Debug($"Failed to get process filename for process {ProcessId}");

			// We throw the exception here to implicitly ignore this process.
			throw ex;
		};
	}

	internal static Window? RegisterWindow(HWND hwnd, IConfigContext configContext)
	{
		Logger.Debug($"Registering window {hwnd.Value}");

		try
		{
			return new Window(hwnd, configContext);
		}
		catch (Exception e)
		{
			Logger.Error($"Could not create a Window instance for {hwnd.Value}, {e.Message}");
			return null;
		}
	}

	public override bool Equals(object? obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		return obj is Window window &&
			window.Handle == Handle;
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode();
	}

	public override string ToString() => $"{Title} ({ProcessName}) [{ProcessId}] <{Class}> {{{Handle.Value}}}";
}
