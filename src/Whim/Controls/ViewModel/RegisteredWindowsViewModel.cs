using System.Collections.ObjectModel;
using System.ComponentModel;
using Whim.Core.ConfigContext;

namespace Whim.Controls.ViewModel;

/// <summary>
/// View model used for for <see cref="RegisteredWindows"/>'s <c>DataContext</c>. This wraps the
/// <see cref="Windows"/> <see cref="ObservableCollection{T}"/> and <see cref="Count"/> properties,
/// exposing them for data binding.
/// </summary>
internal class RegisteredWindowsViewModel : INotifyPropertyChanged
{
	public ObservableCollection<Model.Window> Windows { get; } = new();
	public int Count { get => Windows.Count; }

	public RegisteredWindowsViewModel(IConfigContext configContext)
	{
		configContext.WindowManager.WindowRegistered += OnWindowRegistered;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void OnWindowRegistered(Core.Window.IWindow window)
	{
		Model.Window model = new(window);
		model.WindowUnregistered += OnWindowUnregistered;
		Windows.Add(model);
		OnPropertyChanged(nameof(Count)); // Count is a derived property.
	}

	private void OnWindowUnregistered(Model.Window window)
	{
		Windows.Remove(window);
		OnPropertyChanged(nameof(Count)); // Count is a derived property.
	}
}
