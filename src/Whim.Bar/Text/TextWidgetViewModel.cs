using System.ComponentModel;

namespace Whim.Bar;

public class TextWidgetViewModel : INotifyPropertyChanged
{
	private string _value;
	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				OnPropertyChanged(nameof(Value));
			}
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public TextWidgetViewModel(string? value)
	{
		_value = value ?? string.Empty;
	}
}
