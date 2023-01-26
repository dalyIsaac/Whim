using System.ComponentModel;

namespace Whim.CommandPalette;

/// <summary>
/// Represents an option of a <see cref="SelectVariantConfig"/>
/// </summary>
public record SelectOption : INotifyPropertyChanged
{
	/// <summary>
	/// The unique identifier of this option.
	/// </summary>
	public required string Id { get; init; }

	/// <summary>
	/// The title to display for this option.
	/// </summary>
	public required string Title { get; init; }

	private bool _isSelected;

	/// <summary>
	/// Whether this option is selected.
	/// </summary>
	public required bool IsSelected
	{
		get => _isSelected;
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));
			}
		}
	}

	private bool _isEnabled = true;

	/// <summary>
	/// Whether this option is enabled.
	/// </summary>
	public required bool IsEnabled
	{
		get => _isEnabled;
		set
		{
			if (_isEnabled != value)
			{
				_isEnabled = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEnabled)));
			}
		}
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		PropertyChanged?.Invoke(this, e);
	}
}
