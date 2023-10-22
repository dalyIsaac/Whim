using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Whim.CommandPalette;

internal sealed partial class CommandPaletteWindow : Microsoft.UI.Xaml.Window
{
	public static double TextEntryHeight => 32;

	private readonly IContext _context;
	private readonly IWindow _window;

	public ICommandPaletteWindowViewModel ViewModel { get; private set; }

	public CommandPaletteWindow(IContext context, CommandPalettePlugin plugin)
	{
		_context = context;

		ViewModel = new CommandPaletteWindowViewModel(_context, plugin);
		ViewModel.HideRequested += ViewModel_HideRequested;
		ViewModel.FocusTextBoxRequested += ViewModel_FocusTextBoxRequested;
		ViewModel.SetWindowPosRequested += ViewModel_SetWindowPosRequested;

		_window = this.InitializeBorderlessWindow(_context, "Whim.CommandPalette", "CommandPaletteWindow");
		this.SetIsShownInSwitchers(false);
		this.SetSystemBackdrop();

		Activated += CommandPaletteWindow_Activated;
		Title = CommandPaletteConfig.Title;
	}

	private void ViewModel_HideRequested(object? sender, EventArgs e)
	{
		Logger.Debug("Hiding command palette");
		_window.Hide();
	}

	private void ViewModel_FocusTextBoxRequested(object? sender, EventArgs e)
	{
		// Focus the text box.
		TextEntry.Focus(FocusState.Programmatic);
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
			ViewModel.RequestHide();
		}
	}

	/// <summary>
	/// Activate the command palette.
	/// </summary>
	/// <param name="config">The configuration for activation.</param>
	/// <param name="monitor">The monitor to display the command palette on.</param>
	public void Activate(BaseVariantConfig config, IMonitor? monitor = null)
	{
		Logger.Debug("Activating command palette");
		UIElement? control = ViewModel.Activate(config, monitor);

		PaletteContent.Children.Clear();
		if (control == null)
		{
			Logger.Error("No control to activate");
			return;
		}

		PaletteContent.Children.Add(control);

		TextEntry.SelectAll();
		Activate();
		TextEntry.Focus(FocusState.Programmatic);
		_window.FocusForceForeground();
	}

	/// <summary>
	/// Hide the command palette.
	/// </summary>
	public void Hide()
	{
		Logger.Debug("Hiding command palette");
		ViewModel.RequestHide();
	}

	/// <summary>
	/// Handler for when the user presses down a key.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void TextEntry_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		ViewModel.OnKeyDown(e.Key);
	}

	private void TextEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		ViewModel.Text = TextEntry.Text;
		ViewModel.Update();
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

		double height = TextEntryHeight + ViewModel.GetViewMaxHeight();
		height = Math.Min(ViewModel.MaxHeight, height);

		int scaleFactor = ViewModel.Monitor.ScaleFactor;
		double scale = scaleFactor / 100.0;
		height *= scale;

		int x = (ViewModel.Monitor.WorkingArea.Width / 2) - (width / 2);
		int y = (int)(ViewModel.Monitor.WorkingArea.Height * ViewModel.Plugin.Config.YPositionPercent / 100.0);

		ILocation<int> windowLocation = new Location<int>()
		{
			X = ViewModel.Monitor.WorkingArea.X + x,
			Y = ViewModel.Monitor.WorkingArea.Y + y,
			Width = width,
			Height = (int)height
		};

		WindowContainer.MaxHeight = height;

		using DeferWindowPosHandle handle = _context.NativeManager.DeferWindowPos();
		handle.DeferWindowPos(
			new WindowState()
			{
				Window = _window,
				Location = windowLocation,
				WindowSize = WindowSize.Normal
			},
			_window.Handle
		);
	}
}
