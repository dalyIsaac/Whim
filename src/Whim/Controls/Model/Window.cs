using System;
using System.ComponentModel;
using Whim.Core;

namespace Whim.Controls.Model;


/// <summary>
/// Window model used by <see cref="ViewModel.RegisteredWindowsViewModel"/> and <see cref="RegisteredWindows"/>.
/// </summary>
internal class Window : INotifyPropertyChanged
{
	private static string Now() => DateTime.Now.ToString("HH:mm:ss");

	public int Handle { get; }
	public string Title { get; private set; }
	public int ProcessId { get; }
	public string ProcessFileName { get; }
	public string ProcessName { get; }
	public string TimeRegistered { get; }
	public string LastEvent { get; private set; }
	public string LastEventTime { get; private set; }

	private string _workspaceName = "🔃";
	public string WorkspaceName
	{
		get => _workspaceName;
		set
		{
			_workspaceName = value;
			OnPropertyChanged(nameof(WorkspaceName));
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public event EventHandler<WindowEventArgs>? WindowUnregistered;

	internal Window(IWindow window)
	{
		Handle = (int)window.Handle.Value;
		Title = window.Title;
		ProcessId = window.ProcessId;
		ProcessFileName = window.ProcessFileName;
		ProcessName = window.ProcessName;
		TimeRegistered = Now();
		LastEvent = "Registered";
		LastEventTime = TimeRegistered;

		// register WindowUpdated, WindowFocused, and WindowUnregistered
		window.WindowUpdated += WindowUpdatedEventHandler;
		window.WindowFocused += WindowUpdatedEventHandler;
		window.WindowUnregistered += WindowUnregisteredEventHandler;
	}

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	internal void WindowUpdatedEventHandler(object sender, Core.WindowEventArgs args) => RegisterEvent(args.Window, "Focused");

	internal void WindowUpdatedEventHandler(object sender, Core.WindowUpdateEventArgs args) => RegisterEvent(args.Window, args.UpdateType.ToString());

	/// <summary>
	/// Handles the <see cref="Whim.Core.Window.WindowUnregisterEventHandler"/> event. This calls
	/// <see cref="RegisterEvent(IWindow, string)"/> to register the event, and triggers its own
	/// <see cref="WindowUnregistered"/> event.
	/// </summary>
	/// <param name="window"></param>
	internal void WindowUnregisteredEventHandler(object sender, Core.WindowEventArgs args)
	{
		RegisterEvent(args.Window, "Unregistered");
		args.Window.WindowUpdated -= WindowUpdatedEventHandler;
		args.Window.WindowFocused -= WindowUpdatedEventHandler;
		args.Window.WindowUnregistered -= WindowUnregisteredEventHandler;
		WindowUnregistered?.Invoke(this, new WindowEventArgs(this));
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

		OnPropertyChanged(nameof(LastEventTime));
		OnPropertyChanged(nameof(LastEvent));
		OnPropertyChanged(nameof(Title));
	}
}
