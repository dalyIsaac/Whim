using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Whim.Core;

public class Window : IWindow
{
	private readonly IConfigContext _configContext;
	private const int _bufferCapacity = 255;
	private readonly Pointer _pointer;

	public HWND Handle { get => _pointer.Handle; }

	public string Title
	{
		get
		{
			unsafe
			{
				fixed (char* buffer = new char[_bufferCapacity])
				{
					int length = PInvoke.GetWindowText(_pointer.Handle, buffer, _bufferCapacity + 1);
					return length > 0 ? new string(buffer) : "";
				}
			}
		}
	}

	public string Class
	{
		get
		{
			unsafe
			{
				fixed (char* buffer = new char[_bufferCapacity])
				{
					int length = PInvoke.GetClassName(_pointer.Handle, buffer, _bufferCapacity + 1);
					return length > 0 ? new string(buffer) : "";
				}
			}
		}
	}

	public ILocation Location
	{
		get
		{
			PInvoke.GetWindowRect(_pointer.Handle, out RECT rect);
			return new Location(rect.left,
										 rect.top,
										 rect.right - rect.left,
										 rect.bottom - rect.top);
		}
	}

	public int ProcessId { get; }

	public string ProcessFileName { get; }

	public string ProcessName { get; }

	public bool IsFocused => PInvoke.GetForegroundWindow() == _pointer.Handle;

	public bool IsMinimized => PInvoke.IsIconic(_pointer.Handle);

	public bool IsMaximized => PInvoke.IsZoomed(_pointer.Handle);

	public bool IsMouseMoving { get; internal set; }

	public event WindowUpdateEventHandler? WindowUpdated;
	public event WindowFocusEventHandler? WindowFocused;
	public event WindowUnregisterEventHandler? WindowUnregistered;

	public void BringToTop()
	{
		Logger.Debug(ToString());
		PInvoke.BringWindowToTop(_pointer.Handle);
	}

	public void Close()
	{
		Logger.Debug(ToString());
		Win32Helper.QuitApplication(_pointer.Handle);
		WindowUnregistered?.Invoke(this, new WindowEventArgs(this));
	}

	public void Focus()
	{
		Logger.Debug(ToString());
		if (IsFocused)
		{
			Logger.Debug($"Already focused {this}");
		}

		PInvoke.SetForegroundWindow(_pointer.Handle);
		WindowFocused?.Invoke(this, new WindowEventArgs(this));
	}

	public void Hide()
	{
		Logger.Debug(ToString());
		Win32Helper.HideWindow(_pointer.Handle);
		WindowUpdated?.Invoke(this, new WindowUpdateEventArgs(this, WindowUpdateType.Cloaked));
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
		Win32Helper.ShowWindowMaximized(_pointer.Handle);
	}

	public void ShowMinimized()
	{
		Logger.Debug(ToString());
		Win32Helper.ShowWindowMinimized(_pointer.Handle);
	}

	public void ShowNormal()
	{
		Logger.Debug(ToString());
		Win32Helper.ShowWindowNoActivate(_pointer.Handle);
	}

	/// <summary>
	/// Constructor for the <see cref="IWindow"/> implementation.
	/// </summary>
	/// <param name="pointer"></param>
	/// <param name="configContext"></param>
	/// <exception cref="Win32Exception"></exception>
	private Window(Pointer pointer, IConfigContext configContext)
	{
		_configContext = configContext;
		_pointer = pointer;

		unsafe
		{
			uint pid;
			_ = PInvoke.GetWindowThreadProcessId(_pointer.Handle, &pid);
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

	internal static Window? RegisterWindow(Pointer pointer, IConfigContext configContext)
	{
		Logger.Debug($"Registering window {pointer}");

		try
		{
			return new Window(pointer, configContext);
		}
		catch (Exception e)
		{
			Logger.Error($"Could not create a Window instance for {pointer}, {e.Message}");
			return null;
		}
	}

	internal void UnregisterWindow()
	{
		Logger.Debug(ToString());
		WindowUnregistered?.Invoke(this, new WindowEventArgs(this));
	}

	// NOTE: when writing docs, make a note that register and unregister are handled
	// separately here.
	void IWindow.HandleEvent(uint eventType)
	{
		Logger.Debug($"{this}, {eventType}");
		switch (eventType)
		{
			case PInvoke.EVENT_OBJECT_CLOAKED:
				UpdateWindow(WindowUpdateType.Cloaked);
				break;
			case PInvoke.EVENT_OBJECT_UNCLOAKED:
				UpdateWindow(WindowUpdateType.Uncloaked);
				break;
			case PInvoke.EVENT_SYSTEM_MINIMIZESTART:
				UpdateWindow(WindowUpdateType.MinimizeStart);
				break;
			case PInvoke.EVENT_SYSTEM_MINIMIZEEND:
				UpdateWindow(WindowUpdateType.MinimizeEnd);
				break;
			case PInvoke.EVENT_SYSTEM_FOREGROUND:
				UpdateWindow(WindowUpdateType.Foreground);
				break;
			case PInvoke.EVENT_SYSTEM_MOVESIZESTART:
				StartWindowMove();
				break;
			case PInvoke.EVENT_SYSTEM_MOVESIZEEND:
				EndWindowMove();
				break;
			case PInvoke.EVENT_OBJECT_LOCATIONCHANGE:
				WindowMove();
				break;
		}
	}

	private void UpdateWindow(WindowUpdateType type)
	{
		Logger.Debug($"{this}, {type}");
		WindowUpdated?.Invoke(this, new WindowUpdateEventArgs(this, type));
	}


	private void WindowMove()
	{
		// TODO: mouse handlers
	}

	private void EndWindowMove()
	{
		// TODO: mouse handlers
	}

	private void StartWindowMove()
	{
		// TODO: mouse handlers
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

	public override string ToString() => $"{Title} ({ProcessName}) [{_pointer}]";
}
