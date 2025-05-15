using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Whim.Bar;

/// <summary>
/// A custom drop-down control that uses a <see cref="RadioMenuFlyoutItem"/> for each item.
/// This is used instead of a <see cref="ComboBox"/> because the <see cref="ComboBox"/>
/// doesn't easily support variable-width buttons, or have data binding.
/// </summary>
public sealed partial class DropDown : UserControl
{
	/// <summary>
	/// The selected item in the drop-down.
	/// </summary>
	public string SelectedItem
	{
		get => (string)GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	/// <summary>
	/// The selected item in the drop-down.
	/// </summary>
	public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
		nameof(SelectedItem),
		typeof(string),
		typeof(DropDown),
		new PropertyMetadata("")
	);

	/// <summary>
	/// The items to display in the drop-down.
	/// </summary>
	public object ItemsSource
	{
		get => GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	/// <summary>
	/// The items to display in the drop-down.
	/// </summary>
	public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
		nameof(ItemsSource),
		typeof(object),
		typeof(DropDown),
		new PropertyMetadata(null)
	);

	/// <summary>
	/// Initializes a new instance of the <see cref="DropDown"/> class.
	/// </summary>
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

	/// <summary>
	/// Rebuild the drop-down when the items source or selected item changes.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="dp"></param>
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

	/// <summary>
	/// Update the selected item when a radio item is clicked.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void RadioItem_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not RadioMenuFlyoutItem item)
		{
			return;
		}

		SelectedItem = item.Text;
	}
}
