using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using Windows.System;

namespace Whim.CommandPalette;

internal interface ICommandPaletteWindowViewModel : INotifyPropertyChanged
{
	int MaxHeight { get; set; }
	IMonitor? Monitor { get; }
	string Text { get; set; }
	string PlaceholderText { get; set; }
	bool IsVisible { get; }
	BaseVariantConfig ActivationConfig { get; }
	IVariantControl? ActiveVariant { get; }
	ICommandPalettePlugin Plugin { get; }
	string ConfirmButtonText { get; }
	System.Windows.Input.ICommand ConfirmCommand { get; }
	event EventHandler<EventArgs>? HideRequested;
	event EventHandler<EventArgs>? FocusTextBoxRequested;
	event EventHandler<EventArgs>? SetWindowPosRequested;
	UIElement? Activate(BaseVariantConfig config, IMonitor? monitor);
	void RequestHide();
	void RequestFocusTextBox();
	void OnKeyDown(VirtualKey key);
	void Update();
	double GetViewMaxHeight();
	bool IsConfigActive(BaseVariantConfig config);
}
