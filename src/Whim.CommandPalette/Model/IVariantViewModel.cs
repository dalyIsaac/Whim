using System.ComponentModel;
using Windows.System;

namespace Whim.CommandPalette;

/// <summary>
/// Base interface for all variants of palette view models.
/// </summary>
public interface IVariantViewModel : INotifyPropertyChanged
{
	/// <summary>
	/// Whether to show the save button.
	/// </summary>
	bool ShowSaveButton { get; }

	/// <summary>
	/// Activate this variant.
	/// </summary>
	void Activate(BaseVariantConfig activationConfig);

	/// <summary>
	/// Update what is displayed in the palette.
	/// </summary>
	void Update();

	/// <summary>
	/// Hide this variant.
	/// </summary>
	void Hide();

	/// <summary>
	/// Handle a key down event.
	/// </summary>
	void OnKeyDown(VirtualKey key);
}
