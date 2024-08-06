using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Whim;

/// <summary>
/// A custom <see cref="ObservableCollection{T}"/> which also subscribes to changes in its children.
/// A normal <see cref="ObservableCollection{T}"/> only raises events when the collection itself changes.
/// </summary>
/// <typeparam name="T"></typeparam>
public class VeryObservableCollection<T> : ObservableCollection<T>
{
	/// <inheritdoc/>
	public VeryObservableCollection()
		: base() { }

	/// <inheritdoc/>
	protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		base.OnCollectionChanged(e);
		if (e.NewItems != null)
		{
			foreach (T item in e.NewItems)
			{
				if (item is INotifyPropertyChanged notifyPropertyChanged)
				{
					notifyPropertyChanged.PropertyChanged += NotifyPropertyChanged_PropertyChanged;
				}
			}
		}
		if (e.OldItems != null)
		{
			foreach (T item in e.OldItems)
			{
				if (item is INotifyPropertyChanged notifyPropertyChanged)
				{
					notifyPropertyChanged.PropertyChanged -= NotifyPropertyChanged_PropertyChanged;
				}
			}
		}
	}

	private void NotifyPropertyChanged_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		OnCollectionChanged(
			new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
				System.Collections.Specialized.NotifyCollectionChangedAction.Reset
			)
		);
	}
}
