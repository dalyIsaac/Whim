using System;

namespace Whim.FocusIndicator;

/// <summary>
/// FocusIndicatorPlugin is the plugin that displays a focus indicator on the focused window.
/// </summary>
public interface IFocusIndicatorPlugin : IPlugin, IDisposable
{
	/// <summary>
	/// Whether the focus indicator is visible.
	/// </summary>
	bool IsVisible { get; }

	/// <summary>
	/// Shows the focus indicator.
	/// </summary>
	/// <param name="window">
	/// The window to show the focus indicator on. Defaults to the focused window.
	/// </param>
	void Show(IWindow? window = null);

	/// <summary>
	/// Toggles the focus indicator.
	/// </summary>
	void Toggle();

	/// <summary>
	/// Toggles whether the focus indicator fades.
	/// </summary>
	void ToggleFade();

	/// <summary>
	/// Toggles whether the focus indicator is enabled.
	/// </summary>
	/// <remarks>
	/// This prevents the focus indicator from showing based on events. It will
	/// not prevent the focus indicator from being manually shown.
	/// </remarks>
	void ToggleEnabled();
}
