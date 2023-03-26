using System.ComponentModel;
using Windows.System;

namespace Whim.CommandPalette;

/// <summary>
/// Base interface for all variants of palette view models.
/// </summary>
public interface IVariantViewModel : INotifyPropertyChanged
{
	/// <summary>
	/// The text for the confirm button. Typically "Save" or "Execute".
	/// </summary>
	string? ConfirmButtonText { get; }

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

	/// <summary>
	/// Handle the confirm button being pressed.
	/// </summary>
	void Confirm();
}
