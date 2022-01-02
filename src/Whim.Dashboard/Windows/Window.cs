using System;
using System.ComponentModel;

namespace Whim.Dashboard.Windows;


/// <summary>
/// Window model used by <see cref="WindowsDashboardViewModel"/> and <see cref="WindowsDashboard"/>.
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
	}

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	internal void Window_Focused(object sender, Whim.WindowEventArgs args) => RegisterEvent(args.Window, "Focused");

	internal void Window_Updated(object sender, Whim.WindowUpdateEventArgs args) => RegisterEvent(args.Window, args.UpdateType.ToString());

	/// <summary>
	/// Updates the title and event properties with information from the latest event.
	/// </summary>
	/// <param name="window"></param>
	/// <param name="updateType">
	/// A string describing the latest event/update. This encompasses <see cref="WindowUpdateType"/>
	/// and events like focusing.
	/// </param>
	public void RegisterEvent(IWindow window, string updateType)
	{
		LastEventTime = Now();
		LastEvent = updateType;
		Title = window.Title;

		OnPropertyChanged(nameof(LastEventTime));
		OnPropertyChanged(nameof(LastEvent));
		OnPropertyChanged(nameof(Title));
	}
}
