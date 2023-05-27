using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// View model containing some text.
/// </summary>
public class TextWidgetViewModel : INotifyPropertyChanged
{
	private string _value;

	/// <summary>
	/// The text to display.
	/// </summary>
	public string Value
	{
		get => _value;
		set
		{
			if (_value != value)
			{
				_value = value;
				OnPropertyChanged(nameof(Value));
			}
		}
	}

	/// <summary>
	/// Creates a new instance of <see cref="TextWidgetViewModel"/> with the given text.
	/// </summary>
	/// <param name="value"></param>
	public TextWidgetViewModel(string? value)
	{
		_value = value ?? string.Empty;
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
