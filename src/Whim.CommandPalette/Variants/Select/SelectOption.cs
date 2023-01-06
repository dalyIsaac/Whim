using System.ComponentModel;

namespace Whim.CommandPalette;

/// <summary>
/// Represents an option in a <see cref="SelectVariantConfig"/> select.
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

	private bool _isDisabled;

	/// <summary>
	/// Whether this option is disabled.
	/// </summary>
	public required bool IsDisabled
	{
		get => _isDisabled;
		set
		{
			if (_isDisabled != value)
			{
				_isDisabled = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsDisabled)));
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
