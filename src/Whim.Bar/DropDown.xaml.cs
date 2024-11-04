using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Whim.Bar;

// TODO: Move this justification
// The ComboBox doesn't easily support variable-width buttons.
// The DropDownButton + MenuFlyout + MenuFlyoutItem doesn't have data binding.


public sealed partial class DropDown : UserControl
{
	public string SelectedItem
	{
		get => (string)GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
		nameof(SelectedItem),
		typeof(string),
		typeof(DropDown),
		new PropertyMetadata("")
	);

	public object ItemsSource
	{
		get => GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
		nameof(ItemsSource),
		typeof(object),
		typeof(DropDown),
		new PropertyMetadata(null)
	);

	public DropDown()
	{
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "DropDown");

		RegisterPropertyChangedCallback(ItemsSourceProperty, OnPropertyChanged);
		RegisterPropertyChangedCallback(SelectedItemProperty, OnPropertyChanged);

		// Lazy way to ensure that the selected item is always checked.
		// Sometimes, on first load the selected item would not be checked.
		GettingFocus += DropDown_GettingFocus;
	}

	private void DropDown_GettingFocus(UIElement sender, GettingFocusEventArgs args) =>
		OnPropertyChanged(this, ItemsSourceProperty);

	private void OnPropertyChanged(DependencyObject sender, DependencyProperty dp)
	{
		if (ItemsSource is not IEnumerable<string> items)
		{
			return;
		}

		// Clear the old items and remove their events.
		foreach (RadioMenuFlyoutItem oldRadioItem in MenuFlyout.Items.Cast<RadioMenuFlyoutItem>())
		{
			oldRadioItem.Click -= RadioItem_Click;
		}
		MenuFlyout.Items.Clear();

		// Set up the new items.
		foreach (string item in items)
		{
			RadioMenuFlyoutItem radioItem = new() { Text = item, IsChecked = SelectedItem == item };
			radioItem.Click += RadioItem_Click;
			MenuFlyout.Items.Add(radioItem);
		}
	}

	private void RadioItem_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not RadioMenuFlyoutItem item)
		{
			return;
		}

		SelectedItem = item.Text;
	}
}
