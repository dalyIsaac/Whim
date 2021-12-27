using System;
using System.ComponentModel;

namespace Whim.Dashboard.Controls.Model;


/// <summary>
/// Window model used by <see cref="ViewModel.RegisteredWindowsViewModel"/> and <see cref="RegisteredWindows"/>.
/// </summary>
internal class Window : INotifyPropertyChanged
{
	private static string Now() => DateTime.Now.ToString("HH:mm:ss");

	public int Handle { get; }
	public string Title { get; private set; }
	public string Class { get; }
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
		Class = window.Class;
		ProcessId = window.ProcessId;
		ProcessFileName = window.ProcessFileName;
		ProcessName = window.ProcessName;
		TimeRegistered = Now();
		LastEvent = "Registered";
		LastEventTime = TimeRegistered;

		// register WindowUpdated, WindowFocused, and WindowUnregistered
		window.WindowUpdated += Window_Updated;
		window.WindowFocused += Window_Focused;
		window.WindowUnregistered += Window_Unregistered;
	}

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	internal void Window_Focused(object sender, Whim.WindowEventArgs args) => RegisterEvent(args.Window, "Focused");

	internal void Window_Updated(object sender, Whim.WindowUpdateEventArgs args) => RegisterEvent(args.Window, args.UpdateType.ToString());

	/// <summary>
	/// Handles the <see cref="Whim.Window.WindowUnregisterEventHandler"/> event. This calls
	/// <see cref="RegisterEvent(IWindow, string)"/> to register the event, and triggers its own
	/// <see cref="WindowUnregistered"/> event.
	/// </summary>
	/// <param name="window"></param>
	internal void Window_Unregistered(object sender, Whim.WindowEventArgs args)
	{
		RegisterEvent(args.Window, "Unregistered");
		args.Window.WindowUpdated -= Window_Updated;
		args.Window.WindowFocused -= Window_Focused;
		args.Window.WindowUnregistered -= Window_Unregistered;
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
