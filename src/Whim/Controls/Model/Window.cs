using System;
using System.ComponentModel;
using Whim.Core.Window;

namespace Whim.Controls.Model;

internal delegate void WindowUnregisteredDelegate(Window window);

/// <summary>
/// Window model used by <see cref="ViewModel.RegisteredWindowsViewModel"/> and <see cref="RegisteredWindows"/>.
/// </summary>
internal class Window : INotifyPropertyChanged
{
	private static string Now() => DateTime.Now.ToString("HH:mm:ss");

	public int Handle { get; }
	public string Title { get; private set; }
	public int ProcessId { get; private set; }
	public string ProcessFileName { get; private set; }
	public string ProcessName { get; private set; }
	public string TimeRegistered { get; }
	public string LastEvent { get; private set; }
	public string LastEventTime { get; private set; }

	public event PropertyChangedEventHandler? PropertyChanged;
	public event WindowUnregisteredDelegate? WindowUnregistered;

	internal Window(IWindow window)
	{
		Handle = window.Handle;
		Title = window.Title;
		ProcessId = window.ProcessId;
		ProcessFileName = window.ProcessFileName;
		ProcessName = window.ProcessName;
		TimeRegistered = Now();
		LastEvent = "Registered";
		LastEventTime = TimeRegistered;

		// register WindowUpdated, WindowFocused, and WindowUnregistered
		window.WindowUpdated += OnWindowUpdated;
		window.WindowFocused += OnWindowFocused;
		window.WindowUnregistered += OnWindowUnregistered;
	}

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	internal void OnWindowFocused(IWindow window) => RegisterEvent(window, "Focused");

	internal void OnWindowUpdated(IWindow window, WindowUpdateType updateType) => RegisterEvent(window, updateType.ToString());

	/// <summary>
	/// Handles the <see cref="Whim.Core.Window.WindowUnregisterDelegate"/> event. This calls
	/// <see cref="RegisterEvent(IWindow, string)"/> to register the event, and triggers its own
	/// <see cref="WindowUnregistered"/> event.
	/// </summary>
	/// <param name="window"></param>
	internal void OnWindowUnregistered(IWindow window) {
		RegisterEvent(window, "Unregistered");
		window.WindowUpdated -= OnWindowUpdated;
		window.WindowFocused -= OnWindowFocused;
		window.WindowUnregistered -= OnWindowUnregistered;
		WindowUnregistered?.Invoke(this);
	}

	/// <summary>
	/// Updates the title and event properties with information from the latest event.
	/// </summary>
	/// <param name="window"></param>
	/// <param name="updateType">
	/// A string describing the latest event/update. This encompasses <see cref="WindowUpdateType"/>
	/// and events like focusing.
	/// </param>
	private void RegisterEvent(IWindow window, string updateType)
	{
		LastEventTime = Now();
		LastEvent = updateType;
		Title = window.Title;

		// two thirds of the properties have changed, so might as well say they're all changed 🤷
		OnPropertyChanged(null);
	}

}
