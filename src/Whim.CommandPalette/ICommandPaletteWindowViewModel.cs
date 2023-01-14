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
	IVariantControl? ActiveVariant { get; }
	CommandPalettePlugin Plugin { get; }
	Visibility SaveButtonVisibility { get; }
	System.Windows.Input.ICommand SaveCommand { get; }
	event EventHandler<EventArgs>? HideRequested;
	event EventHandler<EventArgs>? SetWindowPosRequested;
	UIElement? Activate(BaseVariantConfig config, IMonitor? monitor);
	void RequestHide();
	void OnKeyDown(VirtualKey key);
	void Update();
	double GetViewMaxHeight();
	bool IsVariantActive(BaseVariantConfig config);
}
