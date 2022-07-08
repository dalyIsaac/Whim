using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Whim;

internal class Window : IWindow
{
	/// <inheritdoc/>
	public HWND Handle { get; }

	/// <inheritdoc/>
	public string Title => Win32Helper.GetWindowText(Handle);

	/// <inheritdoc/>
	public string WindowClass => Win32Helper.GetClassName(Handle);

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public int ProcessId { get; }

	/// <inheritdoc/>
	public string ProcessFileName { get; }

	/// <inheritdoc/>
	public string ProcessName { get; }

	/// <inheritdoc/>
	public bool IsFocused => PInvoke.GetForegroundWindow() == Handle;

	/// <inheritdoc/>
	public bool IsMinimized => PInvoke.IsIconic(Handle);

	/// <inheritdoc/>
	public bool IsMaximized => PInvoke.IsZoomed(Handle);

	/// <inheritdoc/>
	public bool IsMouseMoving { get; set; }

	/// <inheritdoc/>
	public void BringToTop()
	{
		Logger.Debug(ToString());
		PInvoke.BringWindowToTop(Handle);
	}

	/// <inheritdoc/>
	public void Close()
	{
		Logger.Debug(ToString());
		Win32Helper.QuitApplication(Handle);
	}

	/// <inheritdoc/>
	public void Focus()
	{
		Logger.Debug(ToString());
		if (!IsFocused)
		{
			PInvoke.SetForegroundWindow(Handle);
		}
	}

	/// <inheritdoc/>
	public void FocusForceForeground()
	{
		Logger.Debug(ToString());
		PInvoke.SetForegroundWindow(Handle);
	}

	/// <inheritdoc/>
	public void Hide()
	{
		Logger.Debug(ToString());
		Win32Helper.HideWindow(Handle);
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
		Win32Helper.ShowWindowMaximized(Handle);
	}

	/// <inheritdoc/>
	public void ShowMinimized()
	{
		Logger.Debug(ToString());
		Win32Helper.ShowWindowMinimized(Handle);
	}

	/// <inheritdoc/>
	public void ShowNormal()
	{
		Logger.Debug(ToString());
		Win32Helper.ShowWindowNoActivate(Handle);
	}

	/// <summary>
	/// Constructor for the <see cref="IWindow"/> implementation.
	/// </summary>
	/// <param name="hwnd"></param>
	/// <exception cref="Win32Exception"></exception>
	internal Window(HWND hwnd)
	{
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

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		return obj is Window window &&
			window.Handle == Handle;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return Handle.GetHashCode();
	}

	/// <inheritdoc/>
	public override string ToString() => $"{Title} ({ProcessName}) [{ProcessId}] <{WindowClass}> {{{Handle.Value}}}";
}
