using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Whim;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Whim.Bar;

// TODO: Move this justification
// The ComboBox doesn't easily support variable-width buttons.
// The DropDownButton + MenuFlyout + MenuFlyoutItem doesn't have data binding.


public sealed partial class DropDown : UserControl
{
	public string SelectedItem
	{
		get => (string)GetValue(SelectedItemProperty);
		set
		{
			SetValue(SelectedItemProperty, value);

			foreach (RadioMenuFlyoutItem item in MenuFlyout.Items.Cast<RadioMenuFlyoutItem>())
			{
				item.IsChecked = item.Text == value;
			}
		}
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

	// TODO: Indicate selection
	public DropDown()
	{
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "DropDown");
		RegisterPropertyChangedCallback(ItemsSourceProperty, OnItemsSourceChanged);
	}

	private void OnItemsSourceChanged(DependencyObject d, DependencyProperty dp)
	{
		if (d is not DropDown || dp != ItemsSourceProperty)
		{
			return;
		}

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
