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
		configContext.WorkspaceManager.WorkspaceRouted += WorkspaceManager_Routed;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

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
		model.WindowUnregistered += ModelWindow_WindowUnregistered;
		Windows.Add(model);
		OnPropertyChanged(nameof(Count)); // Count is a derived property.
		return model;
	}

	private void WindowManager_WindowRegistered(object sender, Whim.WindowEventArgs args)
	{
		WindowManager_WindowRegistered(args.Window);
	}

	private void ModelWindow_WindowUnregistered(object? sender, WindowEventArgs args)
	{
		Windows.Remove(args.Window);
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
