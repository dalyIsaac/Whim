using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

internal sealed partial class CommandPaletteWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly IWindow _window;

	public CommandPaletteWindowViewModel ViewModel { get; private set; }

	public CommandPaletteWindow(IConfigContext configContext, CommandPalettePlugin plugin)
	{
		_configContext = configContext;

		ViewModel = new(_configContext, plugin);
		ViewModel.HideRequested += ViewModel_HideRequested;
		ViewModel.SetWindowPosRequested += ViewModel_SetWindowPosRequested;

		_window = this.InitializeBorderlessWindow(_configContext, "Whim.CommandPalette", "CommandPaletteWindow");
		// TODO: Undo
		// this.SetIsShownInSwitchers(false);
		Activated += CommandPaletteWindow_Activated;
		Title = CommandPaletteConfig.Title;
	}

	private void ViewModel_HideRequested(object? sender, EventArgs e)
	{
		Logger.Debug("Hiding command palette");
		_window.Hide();
	}

	private void ViewModel_SetWindowPosRequested(object? sender, EventArgs e)
	{
		SetWindowPos();
	}

	private void CommandPaletteWindow_Activated(object sender, WindowActivatedEventArgs e)
	{
		if (e.WindowActivationState == WindowActivationState.Deactivated)
		{
			// Hide the window when it loses focus.
			// TODO: Undo
			// ViewModel.RequestHide();
		}
	}

	/// <summary>
	/// Activate the command palette.
	/// </summary>
	/// <param name="config">The configuration for activation.</param>
	/// <param name="items">
	/// The items to activate the command palette with. These items will be passed to the
	/// <see cref="ICommandPaletteMatcher"/> to filter the results.
	/// When the text is empty, typically all items are shown.
	/// </param>
	/// <param name="monitor">The monitor to display the command palette on.</param>
	public void Activate(
		BaseCommandPaletteActivationConfig config,
		IEnumerable<CommandItem>? items = null,
		IMonitor? monitor = null
	)
	{
		Logger.Debug("Activating command palette");
		ViewModel.Activate(config, items, monitor);

		TextEntry.SelectAll();
		Activate();
		TextEntry.Focus(FocusState.Programmatic);
		_window.FocusForceForeground();
	}

	/// <summary>
	/// Toggle the visibility of the command palette.
	/// </summary>
	public void Toggle()
	{
		Logger.Debug("Toggling command palette");
		if (ViewModel.IsVisible)
		{
			ViewModel.RequestHide();
		}
		else
		{
			Activate();
		}
	}

	/// <summary>
	/// Handler for when the user presses down a key.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void TextEntry_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		bool scrollIntoView = ViewModel.OnKeyDown(sender, e.Key);
		if (scrollIntoView)
		{
			ListViewItems.ScrollIntoView(ListViewItems.SelectedItem);
		}
	}

	private void TextEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		ViewModel.Text = TextEntry.Text;
		ViewModel.UpdateMatches();
	}

	/// <summary>
	/// Sets the position of the command palette window.
	/// </summary>
	private void SetWindowPos()
	{
		if (ViewModel.Monitor == null)
		{
			Logger.Error("Attempted to activate the command palette without a monitor.");
			return;
		}

		int width = ViewModel.Plugin.Config.MaxWidthPixels;
		int height = ViewModel.MaxHeight;

		if (NoMatchingCommandsTextBlock.Visibility == Visibility.Visible)
		{
			height = (int)(TextEntry.ActualHeight * 2) + 12;
		}
		else if (ListViewItems.Items.Count > 0)
		{
			DependencyObject? container = ListViewItems.ContainerFromIndex(0);
			if (container is ListViewItem item)
			{
				int fullHeight = (int)(TextEntry.ActualHeight + (item.ActualHeight * ListViewItems.Items.Count));
				height = Math.Min(ViewModel.MaxHeight, fullHeight);
			}
		}

		int scaleFactor = ViewModel.Monitor.ScaleFactor;
		double scale = scaleFactor / 100.0;
		height = (int)(height * scale);

		int x = (ViewModel.Monitor.WorkingArea.Width / 2) - (width / 2);
		int y = (int)(ViewModel.Monitor.WorkingArea.Height * ViewModel.Plugin.Config.YPositionPercent / 100.0);

		ILocation<int> windowLocation = new Location<int>()
		{
			X = ViewModel.Monitor.WorkingArea.X + x,
			Y = ViewModel.Monitor.WorkingArea.Y + y,
			Width = width,
			Height = height
		};

		WindowContainer.MaxHeight = height;

		WindowDeferPosHandle.SetWindowPosFixScaling(
			_configContext,
			windowState: new WindowState()
			{
				Window = _window,
				Location = windowLocation,
				WindowSize = WindowSize.Normal
			},
			monitor: ViewModel.Monitor,
			hwndInsertAfter: _window.Handle
		);
	}

	private void CommandListItems_ItemClick(object sender, ItemClickEventArgs e)
	{
		Logger.Debug("Command palette item clicked");
		ListViewItems.SelectedItem = e.ClickedItem;
		ViewModel.ExecuteCommand();
	}
}
