using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Whim.Dashboard.Windows;

/// <summary>
/// View model used for for <see cref="WindowsDashboardView"/>'s <c>DataContext</c>. This wraps the
/// <see cref="Windows"/> <see cref="ObservableCollection{T}"/> and <see cref="Count"/> properties,
/// exposing them for data binding.
/// </summary>
internal class WindowsDashboardViewModel : INotifyPropertyChanged, IDisposable
{
	private bool disposedValue;
	private readonly IConfigContext _configContext;

	public ObservableCollection<Window> Windows { get; } = new();
	public int Count { get => Windows.Count; }

	public WindowsDashboardViewModel(IConfigContext configContext)
	{
		_configContext = configContext;
		configContext.WindowManager.WindowRegistered += WindowManager_WindowRegistered;
		configContext.WindowManager.WindowUpdated += WindowManager_WindowUpdated;
		configContext.WindowManager.WindowFocused += WindowManager_WindowFocused;
		configContext.WindowManager.WindowUnregistered += WindowManager_WindowUnregistered;
		configContext.WorkspaceManager.WorkspaceRouted += WorkspaceManager_Routed;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private Window? FindWindow(IWindow window) => Windows.FirstOrDefault(w => w.Handle == window.Handle.Value);

	/// <summary>
	/// Overload of <see cref="WindowManager_WindowRegistered(object, WindowEventArgs)"/> which
	/// returns the <see cref="Window"/> created.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	private Window WindowManager_WindowRegistered(IWindow window)
	{
		Window? model = Windows.FirstOrDefault(w => w.Handle == window.Handle.Value);
		if (model != null)
		{
			return model;
		}

		model = new(window);
		Windows.Add(model);
		OnPropertyChanged(nameof(Count)); // Count is a derived property.
		return model;
	}

	private void WindowManager_WindowRegistered(object? sender, Whim.WindowEventArgs args)
	{
		WindowManager_WindowRegistered(args.Window);
	}

	private void WindowManager_WindowUpdated(object? sender, Whim.WindowUpdateEventArgs args)
	{
		Window? model = FindWindow(args.Window);
		if (model == null)
		{
			return;
		}

		model.RegisterEvent(args.Window, args.UpdateType.ToString());
	}

	private void WindowManager_WindowFocused(object? sender, Whim.WindowEventArgs args)
	{
		Window? model = FindWindow(args.Window);
		if (model == null)
		{
			return;
		}

		model.RegisterEvent(args.Window, "Focused");
	}

	private void WindowManager_WindowUnregistered(object? sender, WindowEventArgs args)
	{
		Window? model = FindWindow(args.Window);

		if (model == null)
		{
			return;
		}

		Windows.Remove(model);
		OnPropertyChanged(nameof(Count)); // Count is a derived property.
	}

	private void WorkspaceManager_Routed(object? sender, RouteEventArgs args)
	{
		// Find the Model.Window that corresponds to the routed window.
		Window model = WindowManager_WindowRegistered(args.Window);

		// Update the workspace name.
		model.WorkspaceName = args.CurrentWorkspace?.Name ?? "🔃";
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_configContext.WindowManager.WindowRegistered -= WindowManager_WindowRegistered;
				_configContext.WindowManager.WindowUpdated -= WindowManager_WindowUpdated;
				_configContext.WindowManager.WindowFocused -= WindowManager_WindowFocused;
				_configContext.WindowManager.WindowUnregistered -= WindowManager_WindowUnregistered;
				_configContext.WorkspaceManager.WorkspaceRouted -= WorkspaceManager_Routed;
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
